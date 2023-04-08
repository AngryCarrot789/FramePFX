using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using FramePFX.Core.Services;

namespace FramePFX.Utils {
    public class DispatcherCallback {
        private volatile bool isScheduled;
        private readonly Action action;
        private readonly Action actuallyInvoke;
        private readonly Dispatcher dispatcher;
        private volatile DispatcherOperation operation;

        public DispatcherOperation Operation => this.operation;

        public DispatcherCallback(Action action, Dispatcher dispatcher) {
            this.action = action;
            this.dispatcher = dispatcher;
            this.actuallyInvoke = this.DoInvokeAction;
        }

        public bool InvokeAsync() {
            if (this.isScheduled) {
                return false;
            }

            this.isScheduled = true;
            this.operation = this.dispatcher.InvokeAsync(this.actuallyInvoke);
            return true;
        }

        private void DoInvokeAction() {
            try {
                this.action();
            }
            finally {
                this.isScheduled = false;
            }
        }
    }
}