using System;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.TaskSystem {
    public delegate Task TaskFunc(IProgressTracker progress);

    /// <summary>
    /// A class which wraps an action, and stores information used to track the progress and cancel the task
    /// </summary>
    public class TaskAction {
        public TaskFunc Action { get; }

        public IProgressTracker Tracker { get; }

        public CancellationTokenSource CancellationToken { get; }

        public Task Task { get; set; }

        public TaskAction(TaskFunc action) : this(action, null, new CancellationTokenSource()) {
        }

        public TaskAction(TaskFunc action, CancellationTokenSource cancellationToken) : this(action, null, cancellationToken) {
        }

        public TaskAction(TaskFunc action, IProgressTracker progressTracker, CancellationTokenSource cancellationToken) {
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            this.Tracker = progressTracker ?? new AsyncProgressTracker(this);
            this.CancellationToken = cancellationToken ?? throw new ArgumentNullException(nameof(cancellationToken));
        }

        public void CancelTask() => this.CancellationToken.Cancel();

        public void OnTaskCompleted() {
        }
    }
}