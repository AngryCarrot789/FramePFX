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
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Utils;

namespace FramePFX.Tasks {
    public delegate void TaskManagerTaskEventHandler(TaskManager taskManager, ActivityTask task, int index);

    public class TaskManager : IDisposable {
        public static TaskManager Instance => IoC.TaskManager;

        private readonly ThreadLocal<ActivityTask> threadToTask;
        private readonly List<ActivityTask> tasks;
        private readonly object locker;

        public event TaskManagerTaskEventHandler TaskStarted;
        public event TaskManagerTaskEventHandler TaskCompleted;

        public IReadOnlyList<ActivityTask> ActiveTasks => this.tasks;

        public TaskManager() {
            this.threadToTask = new ThreadLocal<ActivityTask>();
            this.tasks = new List<ActivityTask>();
            this.locker = new object();
        }

        public ActivityTask RunTask(Func<Task> action) {
            return ActivityTask.InternalRun(this, action, null, CancellationToken.None);
        }

        public ActivityTask RunTask(Func<Task> action, IActivityProgress progress) {
            return ActivityTask.InternalRun(this, action, progress, CancellationToken.None);
        }

        /// <summary>
        /// Runs the given action in a background thread
        /// </summary>
        /// <param name="action">The action to invoke</param>
        /// <param name="progress">The task progress used to represent the task's completion</param>
        /// <param name="cancellationToken">A token used to signal task cancellation</param>
        /// <returns>The task</returns>
        public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, CancellationToken cancellationToken) {
            return ActivityTask.InternalRun(this, action, progress, cancellationToken);
        }

        /// <summary>
        /// Tries to get the activity task associated with the current caller thread
        /// </summary>
        /// <param name="task">The task associated with the current thread</param>
        /// <returns>True if this thread is running a task</returns>
        public bool TryGetCurrentTask(out ActivityTask task) {
            if (this.threadToTask.IsValueCreated && (task = this.threadToTask.Value) != null) {
                return true;
            }

            task = null;
            return false;
        }

        public ActivityTask CurrentTask => this.threadToTask.Value;

        /// <summary>
        /// Gets either the current task's activity progress tracker, or the <see cref="EmptyActivityProgress"/> instance (for convenience over null-checks)
        /// </summary>
        /// <returns></returns>
        public IActivityProgress GetCurrentProgressOrEmpty() {
            if (this.TryGetCurrentTask(out ActivityTask task)) {
                return task.Progress;
            }

            return EmptyActivityProgress.Instance;
        }

        public void Dispose() {
            this.threadToTask.Dispose();
        }

        internal static void InternalBeginActivateTask_BGTHREAD(TaskManager taskManager, ActivityTask task) {
            taskManager.threadToTask.Value = task;
            IoC.Dispatcher.Invoke(() => {
                InternalOnTaskStartedSafe(taskManager, task);
                ActivityTask.InternalActivate(task);
            });
        }

        internal static void InternalOnTaskCompleted_BGTHREAD(TaskManager taskManager, ActivityTask task, int state) {
            taskManager.threadToTask.Value = null;
            Application.Current.Dispatcher.BeginInvoke(new Action(() => {
                ActivityTask.InternalComplete(task, state);
                InternalOnTaskCompletedSafe(taskManager, task);
                if (task.Exception is Exception e) {
                    const string msg = "An exception occurred while running a task";
                    if (Debugger.IsAttached) {
                        throw new Exception(msg, e);
                    }
                    else {
                        IoC.MessageService.ShowMessage("Task Error", msg, e.GetToString());
                    }
                }
            }), DispatcherPriority.Send);
        }

        internal static void InternalOnTaskStartedSafe(TaskManager taskManager, ActivityTask task) {
            lock (taskManager.locker) {
                int index = taskManager.tasks.Count;
                taskManager.tasks.Insert(index, task);
                taskManager.TaskStarted?.Invoke(taskManager, task, index);
            }
        }

        internal static void InternalOnTaskCompletedSafe(TaskManager taskManager, ActivityTask task) {
            lock (taskManager.locker) {
                int index = taskManager.tasks.IndexOf(task);
                taskManager.tasks.RemoveAt(index);
                taskManager.TaskCompleted?.Invoke(taskManager, task, index);
            }
        }
    }
}