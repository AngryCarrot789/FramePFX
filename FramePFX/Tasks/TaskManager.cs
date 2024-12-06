// 
// Copyright (c) 2024-2024 REghZy
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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System.Diagnostics;
using FramePFX.Utils;

namespace FramePFX.Tasks;

public delegate void TaskManagerTaskEventHandler(TaskManager taskManager, ActivityTask task, int index);

public class TaskManager : IDisposable {
    public static TaskManager Instance => IoC.TaskManager;

    // private readonly ThreadLocal<ActivityTask> threadToTask;
    private readonly AsyncLocal<ActivityTask?> threadToTask;
    private readonly List<ActivityTask> tasks;
    private readonly object locker;

    public event TaskManagerTaskEventHandler? TaskStarted;
    public event TaskManagerTaskEventHandler? TaskCompleted;

    public IReadOnlyList<ActivityTask> ActiveTasks => this.tasks;

    public TaskManager() {
        this.threadToTask = new AsyncLocal<ActivityTask?>();
        this.tasks = new List<ActivityTask>();
        this.locker = new object();
    }

    public ActivityTask RunTask(Func<Task> action) => this.RunTask(action, CancellationToken.None);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress) => this.RunTask(action, progress, CancellationToken.None);

    public ActivityTask RunTask(Func<Task> action, CancellationToken token) => this.RunTask(action, new DefaultProgressTracker(), token);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, CancellationToken cancellationToken) {
        return ActivityTask.InternalStartActivity(this, action, progress, cancellationToken);
    }
    
    public ActivityTask<T> RunTask<T>(Func<Task<T>> action) => this.RunTask(action, CancellationToken.None);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress) => this.RunTask(action, progress, CancellationToken.None);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, CancellationToken token) => this.RunTask(action, new DefaultProgressTracker(), token);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, CancellationToken cancellationToken) {
        return ActivityTask<T>.InternalStartActivity(this, action, progress, cancellationToken);
    }

    /// <summary>
    /// Tries to get the activity task associated with the current caller thread
    /// </summary>
    /// <param name="task">The task associated with the current thread</param>
    /// <returns>True if this thread is running a task</returns>
    public bool TryGetCurrentTask(out ActivityTask? task) {
        return (task = this.threadToTask.Value) != null;
    }

    /// <summary>
    /// Gets the activity running on this thread
    /// </summary>
    /// <exception cref="InvalidOperationException">Not called from the activity's startup thread</exception>
    public ActivityTask CurrentTask => this.threadToTask.Value ?? throw new InvalidOperationException("No task running on this thread");

    /// <summary>
    /// Gets either the current task's activity progress tracker, or the <see cref="EmptyActivityProgress"/> instance (for convenience over null-checks)
    /// </summary>
    /// <returns></returns>
    public IActivityProgress GetCurrentProgressOrEmpty() {
        return this.TryGetCurrentTask(out ActivityTask? task) ? task!.Progress : EmptyActivityProgress.Instance;
    }

    public void Dispose() {
        // this.threadToTask.Dispose();
    }

    // Activity thread

    internal static Task InternalPreActivateTask(TaskManager taskManager, ActivityTask task) {
        taskManager.threadToTask.Value = task;
        return RZApplication.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskStarted(taskManager, task));
    }

    internal static Task InternalOnActivityCompleted(TaskManager taskManager, ActivityTask task, int state) {
        taskManager.threadToTask.Value = null;

        // Before AsyncLocal, I was trying out a dispatcher for each task XD
        // Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatchPriority.Background);
        return RZApplication.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskCompleted(taskManager, task, state));
    }

    // Main Thread

    internal static void InternalOnTaskStarted(TaskManager taskManager, ActivityTask task) {
        lock (taskManager.locker) {
            int index = taskManager.tasks.Count;
            taskManager.tasks.Insert(index, task);
            taskManager.TaskStarted?.Invoke(taskManager, task, index);
        }

        ActivityTask.InternalPostActivate(task);
    }

    internal static void InternalOnTaskCompleted(TaskManager taskManager, ActivityTask task, int state) {
        ActivityTask.InternalComplete(task, state);
        lock (taskManager.locker) {
            int index = taskManager.tasks.IndexOf(task);
            if (index == -1) {
                const string msg = "Completed activity task did not exist in this task manager's internal task list";
                Debug.WriteLine("[FATAL] " + msg);
                Debugger.Break();
                return;
            }

            taskManager.tasks.RemoveAt(index);
            taskManager.TaskCompleted?.Invoke(taskManager, task, index);
        }

        if (task.Exception is Exception e) {
            // TODO: raise event for task completion error maybe?
            Debug.WriteLine($"Task Error\nAn exception occurred while running a task\n{e.GetToString()}");
            Debugger.Break();
        }
    }
}