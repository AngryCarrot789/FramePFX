using System;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly object locker;

        private readonly string debugId; // allows debugger breakpoint to match this
        private volatile bool invokeLater;
        private readonly ExecutionPriority priority;

        public RapidDispatchCallback(Action action, ExecutionPriority priority = ExecutionPriority.Send, string debugId = null) {
            this.debugId = debugId;
            this.locker = new object();
            this.action = action;
            this.priority = priority;
            this.executeAction = () => {
                this.action();
                this.state = 0;
            };

            this.CachedInvoke = this.Invoke;
            this.CachedInvokeNoRet = () => this.Invoke();
        }

        public RapidDispatchCallback(Action action, string debugId) : this(action, ExecutionPriority.Send, debugId) {
        }

        public bool Invoke() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                Services.Application.Invoke(this.executeAction, this.priority);
                return true;
            }

            return false;
        }

        public Task<bool> InvokeAsync() {
            if (Interlocked.CompareExchange(ref this.state, 1, 0) == 0) {
                // i hope this works...
                return Services.Application.InvokeAsync(this.executeAction, this.priority).ContinueWith(t => true);
            }

            return Task.FromResult(false);
        }
    }
}