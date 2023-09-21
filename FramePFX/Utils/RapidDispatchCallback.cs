using System;
using FramePFX.ServiceManaging;

namespace FramePFX.Utils {
    /// <summary>
    /// A class for helping invoke a callback, or skipping if it has not been completed
    /// </summary>
    public class RapidDispatchCallback {
        private volatile bool isScheduled;
        private readonly Action action;

        public bool InvokeLater { get; set; }

        public RapidDispatchCallback() {
        }

        public RapidDispatchCallback(Action action) : this() {
            this.action = action;
        }

        public bool Invoke() {
            lock (this) {
                if (this.isScheduled) {
                    return false;
                }

                Action cb = () => {
                    this.action();
                    this.isScheduled = false;
                };

                this.isScheduled = true;
                Services.Application.Invoke(cb, this.InvokeLater ? ExecutionPriority.Normal : ExecutionPriority.Send);
                return true;
            }
        }

        public bool Invoke(Action action) {
            lock (this) {
                if (this.isScheduled) {
                    return false;
                }

                Action cb = () => {
                    action();
                    this.isScheduled = false;
                };

                this.isScheduled = true;
                Services.Application.Invoke(cb, this.InvokeLater ? ExecutionPriority.Normal : ExecutionPriority.Send);
                return true;
            }
        }
    }
}