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
using System.Threading;
using System.Windows.Threading;

namespace FramePFX.Utils {
    public abstract class DispatcherMultiFireActionGuardBase {
        protected readonly string debugId; // allows debugger breakpoint to match this
        private volatile int state;

        public string DebugId => this.debugId;

        public DispatcherPriority Priority { get; }

        protected DispatcherMultiFireActionGuardBase(DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.Priority = priority;
        }

        protected DispatcherMultiFireActionGuardBase(string debugId) : this(DispatcherPriority.Send, debugId) {
        }

        protected bool BeginInvoke() {
            return Interlocked.CompareExchange(ref this.state, 1, 0) == 0;
        }

        protected void EndInvoke() {
            this.state = 0;
        }
    }

    /// <summary>
    /// A class that is used to dispatch work onto the application dispatcher asynchronously,
    /// ensuring that the same action cannot be enqueued more than once before it has completed
    /// </summary>
    public class DispatcherMultiFireActionGuard : DispatcherMultiFireActionGuardBase {
        private readonly Action executeAction;

        public DispatcherMultiFireActionGuard(Action action, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) : base(priority, debugId) {
            this.executeAction = () => {
                action();
                this.EndInvoke();
            };
        }

        public DispatcherMultiFireActionGuard(Action action, string debugId) : this(action, DispatcherPriority.Send, debugId) {
        }

        /// <summary>
        /// Tries to schedule our action to be invoked asynchronously
        /// </summary>
        /// <returns>See <see cref="InvokeAsync(CancellationToken)"/></returns>
        public bool InvokeAsync() => this.InvokeAsync(CancellationToken.None);

        /// <summary>
        /// Tries to schedule our action to be invoked asynchronously
        /// </summary>
        /// <param name="cancellationToken">A token used to signal the execution to be cancelled</param>
        /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
        public bool InvokeAsync(CancellationToken cancellationToken) {
            if (!this.BeginInvoke())
                return false;
            IoC.Dispatcher.InvokeAsync(this.executeAction, this.Priority, cancellationToken);
            return true;
        }
    }
}