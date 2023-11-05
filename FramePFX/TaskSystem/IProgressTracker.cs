using System.ComponentModel;

namespace FramePFX.TaskSystem {
    /// <summary>
    /// An interface for a class that manages the progress state of a task. This implements <see cref="INotifyPropertyChanged"/>,
    /// which fires when any of our properties change
    /// </summary>
    public interface IProgressTracker : INotifyPropertyChanged {
        /// <summary>
        /// Gets the task associated with this tracker
        /// </summary>
        TaskAction Task { get; }

        /// <summary>
        /// Gets if the task is running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets or sets if this indicator's progress is indeterminate, meaning there's no way of knowing the completion percentage
        /// </summary>
        bool IsIndeterminate { get; set; }

        /// <summary>
        /// Gets if <see cref="Cancel"/> was called
        /// </summary>
        bool IsCancelled { get; }

        /// <summary>
        /// Gets or sets the completion value, which is a value between 0.0 and 1.0 that indicates the rough completion of the task.
        /// Throws if <see cref="IsIndeterminate"/> is true
        /// </summary>
        double CompletionValue { get; set; }

        /// <summary>
        /// Gets or sets this indicator's header text, which is usually a brief explanation of the
        /// task's state, e.g. "Processing Files" for a task that iterates a list of files
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets this indicator's footer text, which usually contains more in-depth
        /// information, e.g. a file name for a task that iterates a list of files
        /// </summary>
        string FooterText { get; set; }

        /// <summary>
        /// Called just before the task has actually started on the thread that called <see cref="TaskManager.Run"/>
        /// </summary>
        void OnPreStarted();

        /// <summary>
        /// Called after the task has actually started
        /// </summary>
        void OnStarted();

        /// <summary>
        /// Called just after the task has actually finished
        /// </summary>
        void OnFinished();

        /// <summary>
        /// Indicates to the task manager that the task should be cancelled. Some
        /// tasks may not handle the cancellation state, and calling this does effectively nothing
        /// </summary>
        void Cancel();
    }
}