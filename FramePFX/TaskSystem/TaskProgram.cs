using System;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.TaskSystem {
    public delegate Task TaskAction(IProgressTracker progress);

    /// <summary>
    /// A class which wraps an action, and stores information used to track the progress and cancel the task
    /// </summary>
    public class TaskProgram {
        public TaskAction Action { get; }

        public IProgressTracker Tracker { get; }

        /// <summary>
        /// A token source used to store the cancellation state of this program
        /// </summary>
        public CancellationTokenSource CancellationTokenSource { get; }

        /// <summary>
        /// Gets the task associated with this program (as in, the task that is being run)
        /// </summary>
        public Task Task { get; internal set; }

        public TaskProgram(TaskAction action) : this(action, null, new CancellationTokenSource()) {
        }

        public TaskProgram(TaskAction action, CancellationTokenSource cancellationToken) : this(action, null, cancellationToken) {
        }

        public TaskProgram(TaskAction action, IProgressTracker progressTracker, CancellationTokenSource cancellationToken) {
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            this.Tracker = progressTracker ?? new AsyncProgressTracker(this);
            this.CancellationTokenSource = cancellationToken ?? throw new ArgumentNullException(nameof(cancellationToken));
        }

        public void CancelTask() => this.CancellationTokenSource.Cancel();
    }
}