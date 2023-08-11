using System;
using System.Threading.Tasks;

namespace FramePFX.Utils {
    public class ConditionMonitor {
        private volatile bool condition;
        private volatile bool isTaskRunning;
        private DateTime lastExecutionTime = DateTime.MinValue;
        private readonly object locker = new object();

        public Func<Task> UserTask { get; set; }

        public TimeSpan MinInterval { get; set; }

        public ConditionMonitor() : this(null) { }

        public ConditionMonitor(Func<Task> userTask) : this(TimeSpan.FromMilliseconds(250), userTask) { }

        public ConditionMonitor(TimeSpan minInterval, Func<Task> userTask) {
            this.UserTask = userTask;
            this.MinInterval = minInterval;
        }

        public void OnInput() {
            lock (this.locker) {
                this.condition = true;
                if (!this.isTaskRunning) {
                    this.isTaskRunning = true;
                    Task.Run(this.TaskMain);
                }
            }
        }

        private async Task TaskMain() {
            bool state;
            lock (this.locker) {
                state = this.condition;
            }

            while (state) {
                TimeSpan interval = DateTime.Now - this.lastExecutionTime;
                TimeSpan minInterval = this.MinInterval;
                if (interval < minInterval)
                    await Task.Delay(minInterval - interval);

                Func<Task> task = this.UserTask;
                if (task != null)
                    await task();

                this.lastExecutionTime = DateTime.Now;

                lock (this.locker) {
                    state = this.condition;
                    if (!state) {
                        this.isTaskRunning = false;
                        return;
                    }
                }
            }
        }
    }
}