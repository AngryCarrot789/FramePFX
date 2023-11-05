using System;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.App;
using FramePFX.ServiceManaging;

namespace FramePFX.Utils {
    /// <summary>
    /// A class for helping invoke a callback, or skipping if it has not been completed
    /// </summary>
    public class RapidDispatchCallback {
        private volatile int state;
        private readonly Action action;
        private readonly Action executeAction;

        public readonly Func<bool> CachedInvoke;
        public readonly Action CachedInvokeNoRet;

        private readonly string debugId; // allows debugger breakpoint to match this
        private readonly DispatchPriority priority;

        public RapidDispatchCallback(Action action, DispatchPriority priority = DispatchPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.action = action;
            this.priority = priority;
            this.executeAction = () => {
                this.action();
                this.state = 0;
            };

            this.CachedInvoke = this.Invoke;
            this.CachedInvokeNoRet = () => this.Invoke();
        }

        public RapidDispatchCallback(Action action, string debugId) : this(action, DispatchPriority.Send, debugId) {
        }

        public bool Invoke() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                IoC.Dispatcher.Invoke(this.executeAction, this.priority);
                return true;
            }

            return false;
        }

        public Task<bool> InvokeAsync() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Task task = IoC.Dispatcher.InvokeAsync(this.executeAction, this.priority);
                if (!task.IsCompleted)
                    return task.ContinueWith(t => true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }

    public class RapidDispatchCallback<T> {
        private volatile int state;
        private readonly Action<T> action;
        private readonly Action<T> executeAction;

        public readonly Func<T, bool> CachedInvoke;
        public readonly Action<T> CachedInvokeNoRet;

        private readonly string debugId; // allows debugger breakpoint to match this
        private readonly DispatchPriority priority;

        public RapidDispatchCallback(Action<T> action, DispatchPriority priority = DispatchPriority.Send, string debugId = null) {
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

        public RapidDispatchCallback(Action<T> action, string debugId) : this(action, DispatchPriority.Send, debugId) {
        }

        public bool Invoke(T parameter) {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                IoC.Dispatcher.Invoke(this.executeAction, parameter, this.priority);
                return true;
            }

            return false;
        }

        public Task<bool> InvokeAsync(T parameter) {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Task task = IoC.Dispatcher.InvokeAsync(this.executeAction, parameter, this.priority);
                if (!task.IsCompleted)
                    return task.ContinueWith(t => true);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}