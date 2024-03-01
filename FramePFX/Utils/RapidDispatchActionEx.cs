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

namespace FramePFX.Utils {
    /// <summary>
    /// An extended version of <see cref="RapidDispatchAction"/> that tries to re-scheduled the action
    /// after it has completed if our invoke method was called during the execution of the execute callback action.
    /// <para>
    /// This is a more reliable version of <see cref="RapidDispatchAction"/>. Which that is designed primarily for single
    /// thread usage, this class is designed to handle multiple threads calling <see cref="InvokeAsync"/>, ensuring that
    /// the callback handles is always 'up to date' if, for example, the execute callback updates a progress bar when
    /// asynchronous progress is made on another thread
    /// </para>
    /// </summary>
    public sealed class RapidDispatchActionEx {
        private const int STATE_NOT_SCHEDULED = 0;
        private const int STATE_RUNNING = 1;
        private const int STATE_SCHEDULED = 2;
        private const int STATE_RESCHEDULED = 3;

        // allows debugger breakpoint to match this. Field, so the debugger knows
        // there are no possible side effects (hopefully??? Am I thinking too hard?)
        private readonly string debugId;

        private volatile int state;        // The current state
        private readonly object stateLock; // A guard when reading/writing the state

        private readonly Action executeAction;  // user execute callback
        private readonly Dispatcher dispatcher; // the dispatcher that owns this RDA

        // just for debugging really
        private volatile DispatcherOperation lastOperation;

        public DispatcherPriority Priority { get; }

        /// <summary>
        /// Constructor for a RDA-Ex
        /// </summary>
        /// <param name="action">The callback action</param>
        /// <param name="priority">The dispatcher priority</param>
        /// <param name="debugId">A debugging ID</param>
        public RapidDispatchActionEx(Action action, DispatcherPriority priority = DispatcherPriority.Normal, string debugId = null) {
            this.dispatcher = Dispatcher.CurrentDispatcher;
            this.debugId = debugId;
            this.Priority = priority;
            this.stateLock = new object();
            this.executeAction = () => {
                int myState;
                lock (this.stateLock) {
                    switch (myState = this.state) {
                        case STATE_SCHEDULED:
                        case STATE_RESCHEDULED:
                            this.state = STATE_RUNNING;
                            break;
                        default: throw new InvalidOperationException($"Invalid state: not scheduled ({myState})");
                    }
                }

                try {
                    action();
                }
                finally {
                    lock (this.stateLock) {
                        if ((myState = this.state) == STATE_RUNNING) {
                            this.state = STATE_NOT_SCHEDULED;
                            myState = -1;
                        }
                    }
                }

                // We process this outside finally just in case InvokeCore throws, or there's an invalid state.
                // If the below is 'exceptional', then this not-so-good oopsie can't really be handled easily,
                // so it must crash WPF, or it can get handled in the DispatcherUnhandledException event

                switch (myState) {
                    case -1:
                        break;
                    case STATE_RESCHEDULED:
                        this.ScheduleExecute();
                        break;
                    default: throw new InvalidOperationException($"Invalid final state: not running or rescheduled ({myState})");
                }
            };
        }

        /// <summary>
        /// Constructor for a RDA-Ex
        /// </summary>
        /// <param name="action">The callback action</param>
        /// <param name="debugId">A debugging ID</param>
        public RapidDispatchActionEx(Action action, string debugId) : this(action, DispatcherPriority.Loaded, debugId) {
        }

        /// <summary>
        /// Tries to schedule our action to be invoked asynchronously
        /// </summary>
        /// <returns>True if the action was scheduled, otherwise false meaning it is already scheduled</returns>
        public bool InvokeAsync() {
            lock (this.stateLock) {
                switch (this.state) {
                    // Default state of the object: not scheduled
                    case STATE_NOT_SCHEDULED:
                        this.state = STATE_SCHEDULED;
                        break;
                    // The actual action passed to the constructor is currently in the middle of running,
                    // so we mark ourself as re-scheduled so that the finally block dispatches our action.
                    // There is a possibility that it HAS finished and the locker is being
                    // acquired, meaning we end up re-scheduling, which is fine though
                    case STATE_RUNNING:
                        this.state = STATE_RESCHEDULED;
                        break;
                    default:
                        return false;
                }
            }

            this.ScheduleExecute();
            return true;
        }

        private void ScheduleExecute() {
            this.lastOperation = this.dispatcher.InvokeAsync(this.executeAction, this.Priority);
        }
    }
}