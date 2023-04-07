using System;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// A class for helping invoke a callback, or skipping if it has not been completed
    /// </summary>
    public class RapidDispatchCallback {
        private volatile bool isScheduled;
        private readonly Action action;
        private readonly Action<Action> callback;

        public bool InvokeLater { get; set; }

        public RapidDispatchCallback() {
            this.callback = (x) => {
                x();
                this.isScheduled = false;
            };
        }

        public RapidDispatchCallback(Action action) : this() {
            this.action = action;
        }

        public bool Invoke() {
            lock (this) {
                if (this.isScheduled) {
                    return false;
                }

                Action callback = () => {
                    this.action();
                    this.isScheduled = false;
                };

                this.isScheduled = true;
                if (this.InvokeLater) {
                    CoreIoC.Dispatcher.InvokeLater(callback);
                }
                else {
                    CoreIoC.Dispatcher.Invoke(callback);
                }

                return true;
            }

        }

        public bool Invoke(Action action) {
            lock (this) {
                if (this.isScheduled) {
                    return false;
                }

                Action callback = () => {
                    action();
                    this.isScheduled = false;
                };

                this.isScheduled = true;
                if (this.InvokeLater) {
                    CoreIoC.Dispatcher.InvokeLater(callback);
                }
                else {
                    CoreIoC.Dispatcher.Invoke(callback);
                }

                return true;
            }
        }
    }
}