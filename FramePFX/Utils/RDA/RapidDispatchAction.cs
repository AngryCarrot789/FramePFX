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

/// <summary>
/// The base class for a regular RDA implementation
/// </summary>
public abstract class RapidDispatchActionBase {
    public readonly string DebugId; // allows debugger breakpoint to match this
    private readonly Action doExecuteCallback;
    private bool isScheduled;

    public DispatchPriority Priority { get; }

    protected RapidDispatchActionBase(DispatchPriority priority, string debugId = null) {
        this.DebugId = debugId;
        this.Priority = priority;
        this.doExecuteCallback = this.DoExecute;
    }

    /// <summary>
    /// Tries to schedule this RDA for execution on the current dispatcher
    /// </summary>
    /// <returns>True if scheduled, false if already scheduled</returns>
    protected bool BeginInvoke() {
        if (this.isScheduled)
            return false;

        this.isScheduled = true;
        Application.Instance.Dispatcher.InvokeAsync(this.doExecuteCallback, this.Priority);
        return true;
    }

    protected static void VerifyThreadAccess() {
        if (!Application.Instance.Dispatcher.CheckAccess())
            throw new InvalidOperationException("Cannot invoke when not on the main thread. Use " + nameof(RapidDispatchActionEx) + " for multi-threading");
    }

    private void DoExecute() {
        try {
            this.Execute();
        }
        finally {
            this.isScheduled = false;
        }
    }

    protected abstract void Execute();
}

/// <summary>
/// A class that is used to execute a callback later on the application dispatcher asynchronously,
/// ensuring that the callback cannot be enqueued more than once before it has completed.
/// <para>
/// This class is not thread safe. The <see cref="InvokeAsync"/> method must be called from the main thread.
/// For multi-threaded use, see <see cref="RapidDispatchActionEx"/>
/// </para>
/// </summary>
public class RapidDispatchAction : RapidDispatchActionBase, IDispatchAction {
    private readonly Action callback;

    public RapidDispatchAction(Action callback, string debugId = null) : this(callback, DispatchPriority.Normal, debugId) {
    }

    public RapidDispatchAction(Action callback, DispatchPriority priority, string debugId = null) : base(priority, debugId) {
        this.callback = callback;
    }

    /// <summary>
    /// Tries to schedule our action to be invoked asynchronously
    /// </summary>
    /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
    public void InvokeAsync() {
        VerifyThreadAccess();
        this.BeginInvoke();
    }

    protected override void Execute() => this.callback();
}

/// <summary>
/// A parameterised version of <see cref="RapidDispatchAction"/> that passes a custom parameter to the callback method
/// </summary>
/// <typeparam name="T">The type of parameter</typeparam>
public class RapidDispatchAction<T> : RapidDispatchActionBase, IDispatchAction<T> {
    private readonly Action<T> callback;
    private T parameter;

    public RapidDispatchAction(Action<T> callback, string debugId = null) : this(callback, DispatchPriority.Normal, debugId) {
    }

    public RapidDispatchAction(Action<T> callback, DispatchPriority priority, string debugId = null) : base(priority, debugId) {
        this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
    }

    protected override void Execute() {
        T param = this.parameter;
        this.parameter = default;
        this.callback(param);
    }

    /// <summary>
    /// Tries to schedule our action to be invoked asynchronously
    /// </summary>
    /// <param name="param">
    ///     The parameter to use. If the execution is already scheduled,
    ///     then this becomes the new parameter passed to the callback
    /// </param>
    /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
    public void InvokeAsync(T param) {
        VerifyThreadAccess();

        this.parameter = param;
        this.BeginInvoke();
    }
}