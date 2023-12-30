using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Logger;
using FramePFX.Utils;

namespace FramePFX.TaskSystem {
    public delegate void TaskStartedEventHandler(TaskManager manager, TaskProgram task);
    public delegate void TaskFinishedEventHandler(TaskManager manager, TaskProgram task);

    /// <summary>
    /// A class that manages registered tasks
    /// </summary>
    public class TaskManager {
        public static TaskManager Instance { get; } = new TaskManager();

        private readonly object RunLock;
        private readonly List<TaskProgram> tasks;
        private readonly List<IProgressTracker> cancellableTrackersForWritePriority;
        private readonly ThreadLocal<TaskProgram> threadProgram;

        public event TaskStartedEventHandler TaskStarted;
        public event TaskFinishedEventHandler TaskFinished;

        /// <summary>
        /// Gets the task running on the current thread. May be null if there is no task
        /// </summary>
        public TaskProgram CurrentTask {
            get => this.threadProgram.IsValueCreated ? this.threadProgram.Value : null;
        }

        public TaskManager() {
            this.RunLock = new object();
            this.tasks = new List<TaskProgram>();
            this.cancellableTrackersForWritePriority = new List<IProgressTracker>();
            this.threadProgram = new ThreadLocal<TaskProgram>();
        }

        public void OnApplicationWriteActionStarting() {
            lock (this.RunLock) {
                foreach (IProgressTracker tracker in this.cancellableTrackersForWritePriority)
                    tracker.Cancel();
                this.cancellableTrackersForWritePriority.Clear();
            }
        }

        private class TaskStartState {
            public volatile int state;
        }

        /// <summary>
        /// Schedules the task for execution on a task scheduler thread
        /// </summary>
        /// <param name="task">The task to execute</param>
        /// <exception cref="ArgumentNullException">Task is null</exception>
        /// <exception cref="Exception">Task's OnPreStart method threw</exception>
        public void Run(TaskProgram task, bool cancelOnAppWriteAction = false) {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            lock (this.RunLock) {
                try {
                    task.Tracker.OnPreStarted();
                }
                catch (Exception e) {
                    throw new Exception("Indicator threw during pre-setup", e);
                }

                // replace reference just in case...
                this.tasks.Add(task);
                if (cancelOnAppWriteAction) {
                    this.cancellableTrackersForWritePriority.Add(task.Tracker);
                }

                TaskStartState state = new TaskStartState();
                task.Task = Task.Run(async () => {
                    CancellationToken token = task.CancellationTokenSource.Token;
                    this.threadProgram.Value = task;
                    while (state.state == 0) {
                        await Task.Delay(1, token);
                    }

                    using (ErrorList list = new ErrorList(false)) {
                        try {
                            task.Tracker.OnStarted();
                        }
                        catch (Exception e) {
                            list.Add(new Exception("Failed to call OnStarted on the indicator", e));
                        }

                        try {
                            if (!token.IsCancellationRequested) {
                                await task.Action(task.Tracker);
                            }
                        }
                        catch (TaskCanceledException) { }
                        catch (Exception e) {
                            list.Add(new Exception("Exception occurred while executing task", e));
                        }

                        try {
                            task.Tracker.OnFinished();
                        }
                        catch (Exception e) {
                            list.Add(new Exception("Failed to call OnFinished on the indicator", e));
                        }

                        this.OnCompletedInternal(task, list);
                    }
                });

                try {
                    this.TaskStarted?.Invoke(this, task);
                }
                finally {
                    state.state = 1;
                }
            }
        }

        /// <summary>
        /// Runs the given task, and awaits its completion
        /// </summary>
        public async Task RunAsync(TaskProgram task, bool cancelOnAppWriteAction = false) {
            this.Run(task, cancelOnAppWriteAction);
            await task.Task;
        }

        private void OnCompletedInternal(TaskProgram task, ErrorList errorList) {
            lock (this.RunLock) {
                this.threadProgram.Value = null;
                this.tasks.Remove(task);
                this.cancellableTrackersForWritePriority.Remove(task.Tracker);
                if (errorList.TryGetException(out Exception exception)) {
                    AppLogger.WriteLine("Exception while running task: " + exception.GetToString());
                }

                this.TaskFinished?.Invoke(this, task);
            }
        }

        public void Cancel(IProgressTracker tracker) {
            lock (this.RunLock) {
                int index = this.tasks.FindIndex(x => x.Tracker == tracker);
                if (index != -1) {
                    this.tasks[index].CancelTask();
                }
            }
        }
    }
}