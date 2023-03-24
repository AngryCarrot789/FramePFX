using System;

namespace FramePFX.Core.Utils {
    /// <summary>
    /// A class for helping invoke a callback, or skipping if it has not been completed
    /// </summary>
    public class RapidDispatchCallback {
        private volatile bool isScheduled;

        public bool Invoke(Action action) {
            lock (this) {
                if (this.isScheduled) {
                    return false;
                }

                this.isScheduled = true;
                CoreIoC.Dispatcher.Invoke(() => {
                    action();
                    this.isScheduled = false;
                });

                return true;
            }
        }
    }
}