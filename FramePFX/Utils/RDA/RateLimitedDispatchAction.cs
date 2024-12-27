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

using System.Diagnostics;

namespace FramePFX.Utils.RDA;

/// <summary>
/// The base class for a rate-limited dispatch action
/// </summary>
public abstract class RateLimitedDispatchActionBase {
    protected static readonly TimeSpan DefaultTimeout = TimeSpan.FromMicroseconds(250);

    protected const int S_CONTINUE = 1; // Keeps the task running
    protected const int S_RUNNING = 2; // Whether or not the task is running
    protected const int S_EXECUTING = 4; // Whether or not we're executing the callback
    protected const int S_CONTINUE_CRITICAL = 8; // InvokeAsync was called while executing

    protected readonly object stateLock; // Used to guard state modifications
    protected volatile int state; // Stores the current state of this object
    private long lastExecutionTime; // The time at which the callback execution completed
    private long minIntervalTicks; // The minimum interval per callbacks

    /// <summary>
    /// Gets or sets the minimum callback interval, that is, the smallest amount of time
    /// that must pass before the callback function can be invoked and awaited
    /// </summary>
    public TimeSpan MinimumInterval {
        get => new TimeSpan(Interlocked.Read(ref this.minIntervalTicks));
        set {
            long ticks = value.Ticks;
            if (ticks <= 0)
                throw new ArgumentOutOfRangeException(nameof(value), ticks, "Value must represent more than zero time");

            Interlocked.Exchange(ref this.minIntervalTicks, ticks);
        }
    }

    public string? DebugName { get; set; }

#if DEBUG
    private string CreationStackTrace { get; } = new StackTrace().GetToString();
#endif

    protected RateLimitedDispatchActionBase() : this(DefaultTimeout) { }

    protected RateLimitedDispatchActionBase(TimeSpan minimumInterval) {
        if (minimumInterval.Ticks < 0)
            throw new ArgumentOutOfRangeException(nameof(minimumInterval), "Minimum interval must represent zero or more time");

        this.MinimumInterval = minimumInterval;
        this.stateLock = new object();
    }

    /// <summary>
    /// Triggers this RLDA, possibly starting a new <see cref="Task"/>, or notifying the existing internal task that there's new input
    /// </summary>
    protected void InvokeAsyncCore() {
        lock (this.stateLock) {
            int myState = this.state;
            if ((myState & S_EXECUTING) == 0) {
                // We are not executing, so append CONTINUE to let the task continue running
                myState |= S_CONTINUE;
            }
            else {
                // The callback is currently being processed, so the critical condition is set
                myState |= S_CONTINUE_CRITICAL;
            }

            if ((myState & S_RUNNING) == 0) {
                // We are not running, so start a new task and append RUNNING
                this.state = myState | S_RUNNING;
                goto StartTask;
            }
            else {
                // Task is already running, so just volatile write the state
                this.state = myState;
            }
        }

        return;

        StartTask:
        Task.Run(this.TaskMain);
    }

    private async Task TaskMain() {
        long lastExecTime = Interlocked.Read(ref this.lastExecutionTime);
        long currInterval = Time.GetSystemTicks() - lastExecTime;

        do {
            // We will sleep at least twice, even if InvokeAsync is only called once.
            // This is so that we don't need to keep creating lots of tasks when
            // InvokeAsync is called very often

            long minInterval = Interlocked.Read(ref this.minIntervalTicks);
            if (currInterval < minInterval) {
                await Task.Delay(new TimeSpan(minInterval - currInterval));
            }

            int myState;
            lock (this.stateLock) {
                if (((myState = this.state) & S_CONTINUE) == 0) {
                    this.state = myState & ~S_RUNNING;
                    Interlocked.Exchange(ref this.lastExecutionTime, lastExecTime);
                    return;
                }
                else {
                    // Not sure if CONTINUE needs to be removed here... I don't think it does
                    this.state = (myState & ~S_CONTINUE) | S_EXECUTING;
                }
            }

            try {
                // state at this point is S_EXECUTING possibly with S_CONTINUE_CRITICAL
                await this.ExecuteCore();
            }
            catch (OperationCanceledException) {
                // ignored
            }
            catch (Exception e) {
                Application.Instance.Dispatcher.Post(() => throw new Exception("Exception while awaiting operation", e));
            }
            finally {
                // This sets CONTINUE to false, indicating that there is no more work required.
                // However, there is a window between when the task finishes and condition being set to false
                // where another thread can set condition to true:
                //     Task just completes, another thread sets condition from false to true,
                //     but then that change is overwritten and condition is set to false here
                //
                // That might mean that whatever work the task does will lose out on the absolute latest
                // update (that occurred a few microseconds~ after the task completed)
                // So hopefully, the usage of EXECUTING and CONTINUE_CRITICAL will help against that
                lock (this.stateLock) {
                    if (((myState = this.state) & S_CONTINUE_CRITICAL) != 0) {
                        // Critical condition is active, so: Remove CRITICAL and append CONTINUE
                        myState = (myState & ~S_CONTINUE_CRITICAL) | S_CONTINUE;
                    }
                    else {
                        // Critical condition not met, so just remove continue,
                        // allowing the task to possibly exit normally
                        myState &= ~S_CONTINUE;
                    }

                    // Remove EXECUTING flag, meaning the critical condition cannot be met,
                    // which is good since we just processed it.
                    // RUNNING will still be present
                    this.state = myState & ~S_EXECUTING;
                }
            }

            lastExecTime = Time.GetSystemTicks();
            currInterval = 0;
        } while (true);
    }

    /// <summary>
    /// Clears the critical continuation state.
    /// <para>
    /// This state is used to re-schedule the callback when <see cref="InvokeAsync"/> is invoked during
    /// execution of the callback.
    /// </para>
    /// <para>
    /// By clearing the state, it means the callback won't be rescheduled if <see cref="InvokeAsync"/> was invoked during
    /// the callback execution. This is useful if you have code to handle similar 're-scheduling' behaviour manually
    /// </para>
    /// </summary>
    public void ClearCriticalState() {
        lock (this.stateLock) {
            int myState = this.state;
            if ((myState & S_CONTINUE_CRITICAL) != 0) {
                // Critical condition is active, so remove it, as if it was never activated
                myState &= ~S_CONTINUE_CRITICAL;
            }

            this.state = myState;
        }
    }

    /// <summary>
    /// The core method for doing work. This method is called in unlocked code,
    /// therefore, care must be taken to synchronize internal values
    /// </summary>
    /// <returns>A task that completes when the work is done</returns>
    protected abstract Task ExecuteCore();

    #region Static Helper Constructors

    public static RateLimitedDispatchAction ForDispatcherAsync(Func<Task> callback, TimeSpan minInterval) => ForDispatcherAsync(callback, minInterval, DispatchPriority.Send);
    public static RateLimitedDispatchAction ForDispatcherAsync(Func<Task> callback, TimeSpan minInterval, DispatchPriority priority) => ForDispatcherAsync(callback, minInterval, Application.Instance.Dispatcher, priority);
    public static RateLimitedDispatchAction ForDispatcherAsync(Func<Task> callback, TimeSpan minInterval, IDispatcher dispatcher) => ForDispatcherAsync(callback, minInterval, dispatcher, DispatchPriority.Send);

    public static RateLimitedDispatchAction ForDispatcherSync(Action callback, TimeSpan minInterval) => ForDispatcherSync(callback, minInterval, DispatchPriority.Send);
    public static RateLimitedDispatchAction ForDispatcherSync(Action callback, TimeSpan minInterval, DispatchPriority priority) => ForDispatcherSync(callback, minInterval, Application.Instance.Dispatcher, priority);
    public static RateLimitedDispatchAction ForDispatcherSync(Action callback, TimeSpan minInterval, IDispatcher dispatcher) => ForDispatcherSync(callback, minInterval, dispatcher, DispatchPriority.Send);

    public static RateLimitedDispatchAction<T> ForDispatcherAsync<T>(Func<T, Task> callback, TimeSpan minInterval) where T : class => ForDispatcherAsync(callback, minInterval, DispatchPriority.Send);
    public static RateLimitedDispatchAction<T> ForDispatcherAsync<T>(Func<T, Task> callback, TimeSpan minInterval, DispatchPriority priority) where T : class => ForDispatcherAsync(callback, minInterval, Application.Instance.Dispatcher, priority);
    public static RateLimitedDispatchAction<T> ForDispatcherAsync<T>(Func<T, Task> callback, TimeSpan minInterval, IDispatcher dispatcher) where T : class => ForDispatcherAsync(callback, minInterval, dispatcher, DispatchPriority.Send);

    public static RateLimitedDispatchAction<T> ForDispatcherSync<T>(Action<T> callback, TimeSpan minInterval) where T : class => ForDispatcherSync(callback, minInterval, DispatchPriority.Send);
    public static RateLimitedDispatchAction<T> ForDispatcherSync<T>(Action<T> callback, TimeSpan minInterval, DispatchPriority priority) where T : class => ForDispatcherSync(callback, minInterval, Application.Instance.Dispatcher, priority);
    public static RateLimitedDispatchAction<T> ForDispatcherSync<T>(Action<T> callback, TimeSpan minInterval, IDispatcher dispatcher) where T : class => ForDispatcherSync(callback, minInterval, dispatcher, DispatchPriority.Send);

    public static RateLimitedDispatchAction ForDispatcherSync(Action callback, TimeSpan minInterval, IDispatcher dispatcher, DispatchPriority priority) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));
        // No need to use async, since we can just directly access the DispatcherOperation's task,
        // which will become completed after the callback returns
        return new RateLimitedDispatchAction(() => dispatcher.InvokeAsync(callback, priority), minInterval);
    }

    public static RateLimitedDispatchAction<T> ForDispatcherSync<T>(Action<T> callback, TimeSpan minInterval, IDispatcher dispatcher, DispatchPriority priority) where T : class {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));
        return new RateLimitedDispatchAction<T>((t) => dispatcher.InvokeAsync(() => callback(t), priority), minInterval);
    }

    public static RateLimitedDispatchAction ForDispatcherAsync(Func<Task> callback, TimeSpan minInterval, IDispatcher dispatcher, DispatchPriority priority) {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));
        return new RateLimitedDispatchAction(async () => {
            try {
                await dispatcher.InvokeAsync(callback, priority);
            }
            catch (Exception e) {
                dispatcher.Post(() => throw new Exception("Exception while awaiting operation", e));
            }
        }, minInterval);
    }

    public static RateLimitedDispatchAction<T> ForDispatcherAsync<T>(Func<T, Task> callback, TimeSpan minInterval, IDispatcher dispatcher, DispatchPriority priority) where T : class {
        Validate.NotNull(callback, nameof(callback));
        Validate.NotNull(dispatcher, nameof(dispatcher));
        return new RateLimitedDispatchAction<T>(async (t) => {
            try {
                await dispatcher.InvokeAsync(() => callback(t), priority);
            }
            catch (Exception e) {
                dispatcher.Post(() => throw new Exception("Exception while awaiting operation", e));
            }
        }, minInterval);
    }

    #endregion

    public sealed override string ToString() {
        return this.GetType().Name + (string.IsNullOrWhiteSpace(this.DebugName) ? "" : $" ({this.DebugName})");
    }
}

/// <summary>
/// A class that is similar to <see cref="RapidDispatchActionEx"/>, but has a set amount of time
/// that has to pass before the callback is scheduled, ensuring the callback is not executed too quickly.
/// <para>
/// This class cannot add a delay to the first or first-in-a-while callback. If you need this, then consider
/// using <see cref="RapidDispatchActionEx"/> and awaiting <see cref="Task.Delay(int)"/> at the start of the callback
/// </para>
/// </summary>
public sealed class RateLimitedDispatchAction : RateLimitedDispatchActionBase, IDispatchAction {
    private readonly Func<Task> callback;

    public RateLimitedDispatchAction(Func<Task> callback) : this(callback, DefaultTimeout) {
    }

    public RateLimitedDispatchAction(Func<Task> callback, TimeSpan minimumInterval) : base(minimumInterval) {
        Validate.NotNull(callback, nameof(callback));
        this.callback = callback;
    }

    public void InvokeAsync() => base.InvokeAsyncCore();

    protected override Task ExecuteCore() => this.callback();
}

/// <summary>
/// A parameter version of <see cref="RateLimitedDispatchAction"/>. 
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed class RateLimitedDispatchAction<T> : RateLimitedDispatchActionBase, IDispatchAction<T> where T : class {
    private class ObjectWrapper {
        public readonly T value;

        public ObjectWrapper(T value) {
            this.value = value;
        }
    }

    private readonly Func<T, Task> callback;
    private ObjectWrapper? currentValue;

    public RateLimitedDispatchAction(Func<T, Task> callback) : this(callback, DefaultTimeout) {
        
    }

    public RateLimitedDispatchAction(Func<T, Task> callback, TimeSpan minimumInterval) : base(minimumInterval) {
        Validate.NotNull(callback, nameof(callback));
        this.callback = callback;
    }

    // An object wrapper is required in order to permit InvokeAsync being called with a null value.
    // When ExecuteCore() comes around, a null value is fine, but a null wrapper is a problem,
    // which should never occur if the exchange of values is implemented correctly

    public void InvokeAsync(T param) {
        _ = Interlocked.Exchange(ref this.currentValue, new ObjectWrapper(param));
        base.InvokeAsyncCore();
    }

    protected override Task ExecuteCore() {
        // THE BUG :
        // -> InvokeAsync(value)
        // -> ExecuteCore, value not exchanged yet ---
        // -> InvokeAsync(another_value)
        // ... ExecuteCore exchanges the value, but now the critical state is met due to the above InvokeAsync
        // ... ExecuteCore completes and then reschedules execution due to the critical state
        // ExecuteCore runs again but with a null currentValue

        // THE FIX :
        // We need to clear the critical state after reading the value.
        // This way, we still get the latest value but ExecuteCore won't get called again, unless InvokeAsync
        // is invoked right after the critical state is cleared which is fine since we got the value.
        // Technically ExecuteCore should get called twice, but I can't think of a better solution currently, and
        // no actual user work gets done in those few microseconds (maybe even 100s of nanos), so it's fine.
        ObjectWrapper? value = Interlocked.Exchange(ref this.currentValue, null);
        this.ClearCriticalState();

        if (value == null) {
            Debug.Assert(false, "Did not expect current value to be null");
            return Task.CompletedTask;
        }

        return this.callback(value.value);
    }
}