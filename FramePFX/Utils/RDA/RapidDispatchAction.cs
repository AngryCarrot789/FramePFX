//
// Copyright (c) 2023-2024 REghZy
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

using System;
using System.Windows.Threading;

namespace FramePFX.Utils.RDA
{
    /// <summary>
    /// The base class for a regular RDA implementation
    /// </summary>
    public abstract class RapidDispatchActionBase
    {
        protected readonly string debugId; // allows debugger breakpoint to match this
        private readonly Action doExecuteCallback;
        private bool isScheduled;

        public string DebugId => this.debugId;

        public DispatcherPriority Priority { get; }

        protected RapidDispatchActionBase(DispatcherPriority priority = DispatcherPriority.Send, string debugId = null)
        {
            this.debugId = debugId;
            this.Priority = priority;
            this.doExecuteCallback = this.DoExecute;
        }

        /// <summary>
        /// Tries to schedule this RDA for execution on the current dispatcher
        /// </summary>
        /// <returns>True if scheduled, false if already scheduled</returns>
        protected bool BeginInvoke()
        {
            if (this.isScheduled)
                return false;

            this.isScheduled = true;
            IoC.Dispatcher.InvokeAsync(this.doExecuteCallback, this.Priority);
            return true;
        }

        protected static void VerifyThreadAccess()
        {
            if (!IoC.Dispatcher.IsOnOwnerThread)
                throw new InvalidOperationException("Cannot invoke when not on the main thread. Use " + nameof(RapidDispatchActionEx) + " for multi-threading");
        }

        private void DoExecute()
        {
            try
            {
                this.ExecuteCore();
            }
            finally
            {
                this.isScheduled = false;
            }
        }

        protected abstract void ExecuteCore();
    }

    /// <summary>
    /// A class that is used to execute a callback later on the application dispatcher asynchronously,
    /// ensuring that the callback cannot be enqueued more than once before it has completed.
    /// <para>
    /// This class is not thread safe. The <see cref="InvokeAsync"/> method must be called from the main thread.
    /// For multi-threaded use, see <see cref="RapidDispatchActionEx"/>
    /// </para>
    /// </summary>
    public class RapidDispatchAction : RapidDispatchActionBase, IRapidDispatchAction
    {
        private readonly Action callback;

        public RapidDispatchAction(Action callback, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) : base(priority, debugId)
        {
            this.callback = callback;
        }

        /// <summary>
        /// Tries to schedule our action to be invoked asynchronously
        /// </summary>
        /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
        public void InvokeAsync()
        {
            VerifyThreadAccess();
            this.BeginInvoke();
        }

        protected override void ExecuteCore() => this.callback();
    }

    /// <summary>
    /// A parameterised version of <see cref="RapidDispatchAction"/> that passes a custom parameter to the callback method
    /// </summary>
    /// <typeparam name="T">The type of parameter</typeparam>
    public class RapidDispatchAction<T> : RapidDispatchActionBase, IRapidDispatchAction<T>
    {
        private readonly Action<T> callback;
        private T parameter;

        public RapidDispatchAction(Action<T> callback, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) : base(priority, debugId)
        {
            this.callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        protected override void ExecuteCore()
        {
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
        public void InvokeAsync(T param)
        {
            VerifyThreadAccess();

            this.parameter = param;
            this.BeginInvoke();
        }
    }
}