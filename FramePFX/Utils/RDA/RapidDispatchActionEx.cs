// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

namespace FramePFX.Utils.RDA;

public abstract class RapidDispatchActionExBase {
    private const int S_NOT_SCHEDULED = 0;
    private const int S_RUNNING = 1;
    private const int S_SCHEDULED = 2;
    private const int S_RESCHEDULED = 3;

    // allows debugger breakpoint to match this. Field, so the debugger knows
    // there are no possible side effects (hopefully??? Am I thinking too hard?)
    public readonly string DebugId;

    private volatile int myState; // The current state
    private readonly object stateLock; // A guard when reading/writing the state

    private readonly Action doExecuteCallback;
    private readonly IDispatcher dispatcher; // the dispatcher that owns this RDA

    /// <summary>
    /// Gets the dispatcher priority used for scheduling
    /// </summary>
    public DispatchPriority Priority { get; }

    /// <summary>
    /// An event that gets raised when an unhandled exception is thrown by the callback action
    /// </summary>
    public event ExceptionEventHandler? ExecutionException;

    /// <summary>
    /// Constructor for a RDA-Ex
    /// </summary>
    /// <param name="callback">The callback action</param>
    /// <param name="priority">The dispatcher priority</param>
    /// <param name="debugId">A debugging ID</param>
    protected RapidDispatchActionExBase(IDispatcher dispatcher, DispatchPriority priority, string debugId) {
        this.dispatcher = dispatcher;
        this.DebugId = debugId;
        this.Priority = priority;
        this.stateLock = new object();
        this.doExecuteCallback = this.DoExecuteAsync;
    }

    private async void DoExecuteAsync() {
        Exception exception = null;

        int state;
        lock (this.stateLock)
            this.myState = S_RUNNING;

        try {
            await this.ExecuteCore();
        }
        catch (Exception e) {
            exception = e;
        }
        finally {
            lock (this.stateLock) {
                switch (state = this.myState) {
                    // standard case; we were running, now we are not
                    case S_RUNNING: this.myState = S_NOT_SCHEDULED; break;

                    // InvokeAsync called while executing. We set the state to scheduled, then outside
                    // the finally block we do another comparison to actually do the scheduling
                    case S_RESCHEDULED: this.myState = S_SCHEDULED; break;

                    // this not-so-good oopsie can't really be handled easily, so it must just be ignored
                    default: this.myState = S_NOT_SCHEDULED; break;
                }
            }
        }

        if (exception != null)
            this.ExecutionException?.Invoke(this, new ExceptionEventArgs(exception));

        // Schedule outside of the lock, because ScheduleExecute is slightly expensive,
        // and we don't want to keep the lock acquired for a long time (and clogg up
        // any thread that calls InvokeAsync)
        if (state == S_RESCHEDULED)
            this.ScheduleExecute();
    }

    private void ScheduleExecute() => this.dispatcher.InvokeAsync(this.doExecuteCallback, this.Priority);

    protected bool BeginInvoke() {
        lock (this.stateLock) {
            switch (this.myState) {
                // Default state of the object: not scheduled
                case S_NOT_SCHEDULED: this.myState = S_SCHEDULED; break;

                // The actual action passed to the constructor is currently in the middle of running,
                // so we mark ourself as re-scheduled so that the finally block dispatches our action.
                // There is a possibility that it HAS finished and the locker is being
                // acquired, meaning we end up re-scheduling, which is fine though
                case S_RUNNING:
                    this.myState = S_RESCHEDULED;
                    return true;
                default: return false;
            }
        }

        // Not scheduling while lock is acquired since it's not necessary
        // and could MAYBE possibly result in a race condition deadlock
        this.ScheduleExecute();
        return true;
    }

    protected abstract Task ExecuteCore();

    /// <summary>
    /// Clears the rescheduled state.
    /// <para>
    /// This state is used to re-schedule the callback when the invoke method is called during execution of the callback.
    /// </para>
    /// <para>
    /// By clearing the state, it means the callback won't be rescheduled. This is useful if you have code to handle
    /// similar 're-scheduling' behaviour manually
    /// </para>
    /// </summary>
    public void ClearRescheduledState() {
        lock (this.stateLock) {
            if (this.myState == S_RESCHEDULED) {
                this.myState = S_RUNNING;
            }
        }
    }
}

/// <summary>
/// An extended version of <see cref="RapidDispatchAction"/> that is thread-safe and tries to re-scheduled the
/// callback after it has completed if our invoke method was called during the execution of the callback action.
/// <para>
/// This is a thread-safe version of <see cref="RapidDispatchAction"/>. This class is designed to handle multiple
/// threads calling <see cref="InvokeAsync"/>, ensuring that the callback handles is always 'up to date' if, for
/// example, the execute callback updates a progress bar when asynchronous progress is made on another thread
/// </para>
/// </summary>
public sealed class RapidDispatchActionEx : RapidDispatchActionExBase, IDispatchAction {
    private readonly Func<Task> callback;

    private RapidDispatchActionEx(IDispatcher dispatcher, Func<Task> callback, DispatchPriority priority, string debugId) : base(dispatcher, priority, debugId) {
        this.callback = callback;
    }

    public static RapidDispatchActionEx ForSync(Action callback, string debugId = null) =>
        ForSync(callback, DispatchPriority.Normal, debugId);

    public static RapidDispatchActionEx ForSync(Action callback, DispatchPriority priority, string debugId = null) =>
        ForSync(callback, RZApplication.Instance.Dispatcher, priority, debugId);

    public static RapidDispatchActionEx ForSync(Action callback, IDispatcher dispatcher, string debugId = null) =>
        ForSync(callback, dispatcher, DispatchPriority.Normal, debugId);

    /// <summary>
    /// Creates an instance of <see cref="RapidDispatchActionEx"/> that runs a non-async callback
    /// </summary>
    public static RapidDispatchActionEx ForSync(Action callback, IDispatcher dispatcher, DispatchPriority priority, string debugId = null) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));

        return new RapidDispatchActionEx(dispatcher, () => {
            callback();
            return Task.CompletedTask;
        }, priority, debugId);
    }

    public static RapidDispatchActionEx ForAsync(Func<Task> callback, string debugId = null) =>
        ForAsync(callback, DispatchPriority.Normal, debugId);

    public static RapidDispatchActionEx ForAsync(Func<Task> callback, DispatchPriority priority, string debugId = null) =>
        ForAsync(callback, RZApplication.Instance.Dispatcher, priority, debugId);

    public static RapidDispatchActionEx ForAsync(Func<Task> callback, IDispatcher dispatcher, string debugId = null) =>
        ForAsync(callback, dispatcher, DispatchPriority.Normal, debugId);

    /// <summary>
    /// Creates an instance of <see cref="RapidDispatchActionEx"/> that runs an async callback
    /// </summary>
    public static RapidDispatchActionEx ForAsync(Func<Task> callback, IDispatcher dispatcher, DispatchPriority priority, string debugId = null) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));

        return new RapidDispatchActionEx(dispatcher, callback, priority, debugId);
    }

    protected override Task ExecuteCore() => this.callback();

    /// <summary>
    /// Tries to schedule our action to be invoked asynchronously
    /// </summary>
    /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
    public void InvokeAsync() => this.BeginInvoke();
}

/// <summary>
/// A parameterised version of <see cref="RapidDispatchActionEx"/> that passes a custom parameter to the callback method
/// </summary>
/// <typeparam name="T">The type of parameter</typeparam>
public sealed class RapidDispatchActionEx<T> : RapidDispatchActionExBase, IDispatchAction<T> {
    private readonly Func<T, Task> callback;
    private readonly object paramLock;
    private T parameter;

    private RapidDispatchActionEx(IDispatcher dispatcher, Func<T, Task> callback, DispatchPriority priority, string debugId) : base(dispatcher, priority, debugId) {
        this.callback = callback;
        this.paramLock = new object();
    }

    public static RapidDispatchActionEx<T> ForSync(Action<T> callback, DispatchPriority priority, string debugId = null) {
        return ForSync(callback, RZApplication.Instance.Dispatcher, priority, debugId);
    }

    /// <summary>
    /// Creates an instance of <see cref="RapidDispatchActionEx"/> that runs a non-async callback
    /// </summary>
    public static RapidDispatchActionEx<T> ForSync(Action<T> callback, IDispatcher dispatcher, DispatchPriority priority, string debugId = null) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));

        return new RapidDispatchActionEx<T>(dispatcher, (t) => {
            callback(t);
            return Task.CompletedTask;
        }, priority, debugId);
    }

    /// <summary>
    /// Creates an instance of <see cref="RapidDispatchActionEx"/> that runs an async callback
    /// </summary>
    public static RapidDispatchActionEx<T> ForAsync(Func<T, Task> callback, DispatchPriority priority, string debugId = null) {
        return ForAsync(callback, RZApplication.Instance.Dispatcher, priority, debugId);
    }

    /// <summary>
    /// Creates an instance of <see cref="RapidDispatchActionEx"/> that runs an async callback
    /// </summary>
    public static RapidDispatchActionEx<T> ForAsync(Func<T, Task> callback, IDispatcher dispatcher, DispatchPriority priority, string debugId = null) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));

        return new RapidDispatchActionEx<T>(dispatcher, callback, priority, debugId);
    }

    protected override Task ExecuteCore() {
        T param;
        lock (this.paramLock) {
            param = this.parameter;
            this.parameter = default;
        }

        return this.callback(param);
    }

    public void InvokeAsync(T param) {
        lock (this.paramLock)
            this.parameter = param;

        this.BeginInvoke();
    }
}