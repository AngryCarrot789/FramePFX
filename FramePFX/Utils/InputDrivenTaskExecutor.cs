using System;
using System.Threading.Tasks;

namespace FramePFX.Utils {
    /// <summary>
    /// A class used for executing a tasks when an input signal is received, and ensuring the task is not
    /// executed too quickly (time since last execution will exceed <see cref="MinimumInterval"/>)
    /// </summary>
    public class InputDrivenTaskExecutor {
        private volatile bool condition;
        private volatile bool isTaskRunning;
        private volatile bool isExecutingTask;
        private volatile bool conditionCritical;
        private DateTime lastExecutionTime = DateTime.MinValue;
        private readonly object locker = new object();

        /// <summary>
        /// A function that returns a task that is awaited when required
        /// </summary>
        public Func<Task> Execute { get; set; }

        /// <summary>
        /// The smallest amount of time that must pass before the <see cref="Execute"/> task may be executed again
        /// </summary>
        public TimeSpan MinimumInterval { get; set; }

        public InputDrivenTaskExecutor() : this(null) { }

        public InputDrivenTaskExecutor(Func<Task> userTask) : this(userTask, TimeSpan.FromMilliseconds(250)) { }

        public InputDrivenTaskExecutor(Func<Task> execute, TimeSpan minimumInterval) {
            this.Execute = execute;
            this.MinimumInterval = minimumInterval;
        }

        /// <summary>
        /// Triggers this executor, possibly starting a new <see cref="Task"/>, or notifying the existing internal task that there's new input
        /// </summary>
        public void OnInput() {
            lock (this.locker) {
                if (this.isExecutingTask) {
                    this.conditionCritical = true;
                }
                else {
                    this.condition = true;
                }

                if (!this.isTaskRunning) {
                    this.isTaskRunning = true;
                    Task.Run(this.TaskMain);
                }
            }
        }

        private async Task TaskMain() {
            do {
                // Ensure maximum interval
                TimeSpan timeSinceLastExecute = DateTime.Now - this.lastExecutionTime;
                TimeSpan minInterval = this.MinimumInterval;
                if (timeSinceLastExecute < minInterval)
                    await Task.Delay(minInterval - timeSinceLastExecute);

                lock (this.locker) {
                    if (this.condition) {
                        this.condition = false;
                    }
                    else {
                        this.isTaskRunning = false;
                        return;
                    }
                }

                this.isExecutingTask = true;
                try {
                    Func<Task> func = this.Execute;
                    Task task = func?.Invoke();
                    if (task != null && !task.IsCompleted) {
                        await task;
                    }
                }
                finally {
                    // This sets condition to false, indicating that there is no more work required.
                    // However there is a window between when the task finishes and condition being set to false
                    // where another thread can set condition to true:
                    //     Task just completes, another thread sets condition from false to true,
                    //     but then that change is overwritten and condition is set to false here
                    //
                    // That might mean that whatever work the task does will lose out on the absolute latest
                    // update (that occurred a few microseconds~ after the task completed)
                    // So hopefully, the usage of isExecutingTask and isCriticalCondition will help against that
                    lock (this.locker) {
                        if (this.conditionCritical) {
                            this.condition = true;
                            this.conditionCritical = false;
                        }
                        else {
                            this.condition = false;
                        }
                    }

                    this.isExecutingTask = false;
                }

                this.lastExecutionTime = DateTime.Now;
            } while (true);
        }
    }
}