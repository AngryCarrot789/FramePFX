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
    /// <summary>
    /// A class that is used to dispatch work onto the application dispatcher asynchronously, ensuring that the same action
    /// cannot be enqueued more than once before it has completed. This class is thread safe, however, calling invoke from
    /// other threads is not recommended, since this class does not track when invoke is called during the actual execute callback
    /// <para>
    /// This is a simpler version of <see cref="RapidDispatchActionEx"/>. While that version is designed
    /// for multiple threads calling to invoke, this version is not, and using this class for multiple
    /// threads may not yield the best results
    /// </para>
    /// </summary>
    public class RapidDispatchAction {
        protected const int STATE_INACTIVE = 0;
        protected const int STATE_SCHEDULED = 1;

        protected readonly string debugId; // allows debugger breakpoint to match this
        private readonly Action executeAction;
        private volatile int state;

        public string DebugId => this.debugId;

        public DispatcherPriority Priority { get; }

        public RapidDispatchAction(Action action, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.Priority = priority;
            this.executeAction = () => {
                try {
                    action();
                }
                finally {
                    this.state = STATE_INACTIVE;
                }
            };
        }

        public RapidDispatchAction(Action action, string debugId) : this(action, DispatcherPriority.Send, debugId) {

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
            if (Interlocked.CompareExchange(ref this.state, STATE_SCHEDULED, STATE_INACTIVE) != STATE_INACTIVE)
                return false;
            IoC.Dispatcher.InvokeAsync(this.executeAction, this.Priority, cancellationToken);
            return true;
        }
    }
}