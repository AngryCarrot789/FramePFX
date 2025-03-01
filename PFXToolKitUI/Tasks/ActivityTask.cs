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

using System.Runtime.CompilerServices;

namespace PFXToolKitUI.Tasks;

/// <summary>
/// Represents a task that can be run by a <see cref="ActivityManager"/> on a background thread
/// </summary>
public class ActivityTask {
    private readonly ActivityManager activityManager;
    private readonly Func<Task> action;
    private volatile Exception? exception;

    //  0 = waiting for activation
    //  1 = running
    //  2 = completed
    //  3 = cancelled
    private volatile int state;
    private volatile Task? userTask; // task from action()
    protected Task? theMainTask; // task we created

    protected Task? UserTask => this.userTask;

    /// <summary>
    /// Returns true if the task is currently still running
    /// </summary>
    public bool IsRunning => this.state == 1;

    /// <summary>
    /// Returns true if the task is completed. <see cref="Exception"/> may be non-null when this is true
    /// </summary>
    public bool IsCompleted => this.state > 1;

    /// <summary>
    /// Returns true when this activity was completed due to cancellation
    /// </summary>
    public bool IsCancelled => this.state == 3;

    /// <summary>
    /// Gets the exception that was thrown during the execution of the user action
    /// </summary>
    public Exception? Exception => this.exception;

    /// <summary>
    /// Gets the progress handler associated with this task. Will always be non-null
    /// </summary>
    public IActivityProgress Progress { get; }

    /// <summary>
    /// Gets the first token created from our <see cref="CancellationTokenSource"/>
    /// </summary>
    public CancellationToken CancellationToken { get; }

    /// <summary>
    /// Gets this activity's task, which can be used to await completion. This task is a proxy of
    /// the user task function, and will not throw <see cref="OperationCanceledException"/> when
    /// awaited if our <see cref="CancellationToken"/>'s is cancelled
    /// </summary>
    public Task Task {
        get => this.theMainTask!;
        private set => this.theMainTask = value;
    }

    // internal int OwningThreadId;

    protected ActivityTask(ActivityManager activityManager, Func<Task> action, IActivityProgress activityProgress, CancellationToken cancellationToken) {
        this.activityManager = activityManager ?? throw new ArgumentNullException(nameof(activityManager));
        this.action = action ?? throw new ArgumentNullException(nameof(action));
        this.Progress = activityProgress ?? throw new ArgumentNullException(nameof(activityProgress));
        this.CancellationToken = cancellationToken;
    }

    protected virtual Task CreateTask(TaskCreationOptions creationOptions) {
        // We don't provide the cancellation token, because we want to handle it
        // separately. Awaiting this activity task should never throw an exceptino
        return Task.Factory.StartNew(this.TaskMain, creationOptions).Unwrap();
    }

    /// <summary>
    /// Gets this activity's awaiter that can be used to await the activity. This calls <see cref="System.Threading.Tasks.Task.GetAwaiter"/>
    /// on our internal task, which cannot be cancelled in the standard manner
    /// </summary>
    /// <returns>The awaiter</returns>
    public TaskAwaiter GetAwaiter() => this.Task.GetAwaiter();

    protected async Task TaskMain() {
        // This Dispatcher usage here was used to have a synchronisation context so that async callbacks
        // would be fired on the dispatcher thread meaning ThreadLocal would work. However, AsyncLocal works nicely

        // this.OwningThreadId = Thread.CurrentThread.ManagedThreadId;
        // Dispatcher.CurrentDispatcher.InvokeAsync(async () => {
        try {
            await ActivityManager.InternalPreActivateTask(this.activityManager, this);
            this.CheckCancelled();
            await ((this.userTask = this.action()) ?? Task.CompletedTask);
            await this.OnCompleted(null);
        }
        catch (OperationCanceledException) { // gets TaskCancelledException too
            await this.OnCancelled();
        }
        catch (Exception e) {
            await this.OnCompleted(e);
        }
        // });
        // Dispatcher.Run();
    }

    public void CheckCancelled() => this.CancellationToken.ThrowIfCancellationRequested();

    private Task OnCancelled() => ActivityManager.InternalOnActivityCompleted(this.activityManager, this, 3);

    protected virtual async Task OnCompleted(Exception? e) {
        this.exception = e;
        await ActivityManager.InternalOnActivityCompleted(this.activityManager, this, 2);
    }

    internal static ActivityTask InternalStartActivity(ActivityManager activityManager, Func<Task> action, IActivityProgress? progress, CancellationToken token, TaskCreationOptions creationOptions) {
        return InternalStartActivityImpl(new ActivityTask(activityManager, action, progress ?? new DefaultProgressTracker(), token), creationOptions);
    }

    internal static ActivityTask InternalStartActivityImpl(ActivityTask task, TaskCreationOptions creationOptions) {
        task.Task = task.CreateTask(creationOptions);
        return task;
    }

    public static void InternalPostActivate(ActivityTask task) {
        task.state = 1;
    }

    public static void InternalComplete(ActivityTask task, int state) {
        task.state = state;
    }
}

// This system isn't great but it just about works... i'd rather not use public new ... methods but oh well

public class ActivityTask<T> : ActivityTask {
    public T? Result { get; private set; }

    public new Task<T> Task => (Task<T>) this.theMainTask!;

    protected ActivityTask(ActivityManager activityManager, Func<Task<T>> action, IActivityProgress activityProgress, CancellationToken cancellationToken) : base(activityManager, action, activityProgress, cancellationToken) {
    }

    internal static ActivityTask<T> InternalStartActivity(ActivityManager activityManager, Func<Task<T>> action, IActivityProgress? progress, CancellationToken token, TaskCreationOptions creationOptions) {
        return (ActivityTask<T>) InternalStartActivityImpl(new ActivityTask<T>(activityManager, action, progress ?? new DefaultProgressTracker(), token), creationOptions);
    }

    /// <inheritdoc cref="ActivityTask.GetAwaiter"/>
    public new TaskAwaiter<T> GetAwaiter() => this.Task.GetAwaiter();

    protected override Task CreateTask(TaskCreationOptions creationOptions) {
        return System.Threading.Tasks.Task.Factory.StartNew(this.TaskMain, creationOptions).Unwrap().ContinueWith(x => this.Result, TaskContinuationOptions.ExecuteSynchronously);
    }

    protected override Task OnCompleted(Exception? e) {
        if (this.UserTask is Task<T> t && e == null) {
            this.Result = t.Result ?? default;
        }

        return base.OnCompleted(e);
    }
}