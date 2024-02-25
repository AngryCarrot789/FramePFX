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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Tasks {
    /// <summary>
    /// Represents a task that can be run by a <see cref="TaskManager"/> on a background thread
    /// </summary>
    public class ActivityTask {
        private readonly TaskManager taskManager;
        private readonly Func<Task> action;
        private Task taskMain;
        private Exception exception;

        //  0 = waiting for activation
        //  1 = running
        //  2 = completed
        //  3 = cancelled
        private volatile int state;

        /// <summary>
        /// Returns true if the task is currently still running
        /// </summary>
        public bool IsRunning => this.state == 1;

        /// <summary>
        /// Returns true if the task is completed. <see cref="Exception"/> may be non-null when this is true
        /// </summary>
        public bool IsCompleted => this.state > 1;

        /// <summary>
        /// Gets the exception that was thrown during the execution of the user action
        /// </summary>
        public Exception Exception => this.exception;

        /// <summary>
        /// Gets the progress handler associated with this task. Will always be non-null
        /// </summary>
        public IActivityProgress Progress { get; }

        public CancellationToken CancellationToken { get; }

        private ActivityTask(TaskManager taskManager, Func<Task> action, CancellationToken cancellationToken, IActivityProgress activityProgress) {
            this.taskManager = taskManager ?? throw new ArgumentNullException(nameof(taskManager));
            this.action = action ?? throw new ArgumentNullException(nameof(action));
            this.Progress = activityProgress ?? throw new ArgumentNullException(nameof(activityProgress));
            this.CancellationToken = cancellationToken;
        }

        private async Task TaskMain() {
            try {
                TaskManager.InternalBeginActivateTask_BGTHREAD(this.taskManager, this);
                while (this.state != 1) {
                    await Task.Delay(1, this.CancellationToken);
                }

                this.CheckCancelled();
                await (this.action() ?? Task.CompletedTask);
                this.OnCompleted(null);
            }
            catch (TaskCanceledException) {
                this.OnCancelled();
            }
            catch (OperationCanceledException) {
                this.OnCancelled();
            }
            catch (Exception e) {
                this.OnCompleted(e);
            }
        }

        public void CheckCancelled() {
            if (this.CancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();
        }

        private void OnCancelled() {
            TaskManager.InternalOnTaskCompleted_BGTHREAD(this.taskManager, this, 3);
        }

        private void OnCompleted(Exception e) {
            this.exception = e;
            TaskManager.InternalOnTaskCompleted_BGTHREAD(this.taskManager, this, 2);
        }

        internal static ActivityTask InternalRun(TaskManager taskManager, Func<Task> action, IActivityProgress progress, CancellationToken cancellationToken) {
            ActivityTask task = new ActivityTask(taskManager, action, cancellationToken, progress ?? new DefaultProgressTracker());
            task.taskMain = Task.Run(task.TaskMain);
            return task;
        }

        public static void InternalActivate(ActivityTask task) {
            task.state = 1;
        }

        public static void InternalComplete(ActivityTask task, int state) {
            task.state = state;
        }
    }
}