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
using System.Diagnostics.CodeAnalysis;

namespace PFXToolKitUI.Tasks;

public delegate void TaskManagerTaskEventHandler(ActivityManager activityManager, ActivityTask task, int index);

public sealed class ActivityManager : IDisposable {
    public static ActivityManager Instance => Application.Instance.ServiceManager.GetService<ActivityManager>();

    // private readonly ThreadLocal<ActivityTask> threadToTask;
    private readonly AsyncLocal<ActivityTask?> threadToTask;
    private readonly List<ActivityTask> tasks;
    private readonly object locker;

    public event TaskManagerTaskEventHandler? TaskStarted;
    public event TaskManagerTaskEventHandler? TaskCompleted;

    public IReadOnlyList<ActivityTask> ActiveTasks => this.tasks;

    public ActivityManager() {
        this.threadToTask = new AsyncLocal<ActivityTask?>();
        this.tasks = new List<ActivityTask>();
        this.locker = new object();
    }

    public ActivityTask RunTask(Func<Task> action, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, CancellationToken.None, creationOptions);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, progress, CancellationToken.None, creationOptions);

    public ActivityTask RunTask(Func<Task> action, CancellationToken token, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, new DefaultProgressTracker(), token, creationOptions);

    public ActivityTask RunTask(Func<Task> action, IActivityProgress progress, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None) {
        return ActivityTask.InternalStartActivity(this, action, progress, cancellationToken, creationOptions);
    }

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, CancellationToken.None, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, progress, CancellationToken.None, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, CancellationToken token, TaskCreationOptions creationOptions = TaskCreationOptions.None) => this.RunTask(action, new DefaultProgressTracker(), token, creationOptions);

    public ActivityTask<T> RunTask<T>(Func<Task<T>> action, IActivityProgress progress, CancellationToken cancellationToken, TaskCreationOptions creationOptions = TaskCreationOptions.None) {
        return ActivityTask<T>.InternalStartActivity(this, action, progress, cancellationToken, creationOptions);
    }

    /// <summary>
    /// Tries to get the activity task associated with the current caller thread
    /// </summary>
    /// <param name="task">The task associated with the current thread</param>
    /// <returns>True if this thread is running a task</returns>
    public bool TryGetCurrentTask([NotNullWhen(true)] out ActivityTask? task) {
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
        return this.TryGetCurrentTask(out ActivityTask? task) ? task.Progress : EmptyActivityProgress.Instance;
    }

    public void Dispose() {
        // this.threadToTask.Dispose();
    }

    // Activity thread

    internal static Task InternalPreActivateTask(ActivityManager activityManager, ActivityTask task) {
        activityManager.threadToTask.Value = task;
        return Application.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskStarted(activityManager, task));
    }

    internal static Task InternalOnActivityCompleted(ActivityManager activityManager, ActivityTask task, int state) {
        activityManager.threadToTask.Value = null;

        // Before AsyncLocal, I was trying out a dispatcher for each task XD
        // Dispatcher.CurrentDispatcher.BeginInvokeShutdown(DispatchPriority.Background);
        return Application.Instance.Dispatcher.InvokeAsync(() => InternalOnTaskCompleted(activityManager, task, state));
    }

    // Main Thread

    internal static void InternalOnTaskStarted(ActivityManager activityManager, ActivityTask task) {
        lock (activityManager.locker) {
            int index = activityManager.tasks.Count;
            activityManager.tasks.Insert(index, task);
            activityManager.TaskStarted?.Invoke(activityManager, task, index);
        }

        ActivityTask.InternalPostActivate(task);
    }

    internal static void InternalOnTaskCompleted(ActivityManager activityManager, ActivityTask task, int state) {
        ActivityTask.InternalComplete(task, state);
        lock (activityManager.locker) {
            int index = activityManager.tasks.IndexOf(task);
            if (index == -1) {
                const string msg = "Completed activity task did not exist in this task manager's internal task list";
                Debug.WriteLine("[FATAL] " + msg);
                Debugger.Break();
                return;
            }

            activityManager.tasks.RemoveAt(index);
            activityManager.TaskCompleted?.Invoke(activityManager, task, index);
        }
    }
}