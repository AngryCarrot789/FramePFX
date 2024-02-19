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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Utils {
    /// <summary>
    /// A class for helping invoke a callback, or skipping if it has not been completed
    /// </summary>
    public class RapidDispatchCallback {
        private volatile int state;
        private readonly Action executeAction;

        private readonly string debugId; // allows debugger breakpoint to match this
        private readonly DispatcherPriority priority;

        public RapidDispatchCallback(Action action, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.priority = priority;
            this.executeAction = () => {
                action();
                this.state = 0;
            };
        }

        public RapidDispatchCallback(Action action, string debugId) : this(action, DispatcherPriority.Send, debugId) {
        }

        public bool Invoke() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Application.Current.Dispatcher.Invoke(this.executeAction, this.priority);
                return true;
            }

            return false;
        }

        public Task<bool> InvokeAsync() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Task task = Application.Current.Dispatcher.InvokeAsync(this.executeAction, this.priority).Task;
                if (!task.IsCompleted)
                    return task.ContinueWith(t => true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T">The parameter passed to the callback action</typeparam>
    public class RapidDispatchCallback<T> {
        private volatile int state;
        private readonly Action<T> action;
        private readonly Action<T> executeAction;

        public readonly Func<T, bool> CachedInvoke;
        public readonly Action<T> CachedInvokeNoRet;

        private readonly string debugId; // allows debugger breakpoint to match this
        private readonly DispatcherPriority priority;

        public RapidDispatchCallback(Action<T> action, DispatcherPriority priority = DispatcherPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.action = action;
            this.priority = priority;
            this.executeAction = (t) => {
                this.action(t);
                this.state = 0;
            };

            this.CachedInvoke = this.Invoke;
            this.CachedInvokeNoRet = t => this.Invoke(t);
        }

        public RapidDispatchCallback(Action<T> action, string debugId) : this(action, DispatcherPriority.Send, debugId) {
        }

        public bool Invoke(T parameter) {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Application.Current.Dispatcher.Invoke(this.executeAction, this.priority, parameter);
                return true;
            }

            return false;
        }

        public Task<bool> InvokeAsync(T parameter) {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Task task = Application.Current.Dispatcher.InvokeAsync(() => this.executeAction(parameter), this.priority).Task;
                if (!task.IsCompleted)
                    return task.ContinueWith(t => true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}