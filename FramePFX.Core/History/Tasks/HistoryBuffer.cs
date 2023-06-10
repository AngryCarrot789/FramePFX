using System;
using System.ComponentModel;
using System.Diagnostics;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.History.Tasks {
    /// <summary>
    /// <para>
    /// Delayed enqueuement is a misleading name; it should be "delayed final history-action-state"
    /// </para>
    /// <para>
    /// This class allows an instance of a history action to be modifiable for a certain amount of time, before
    /// being locked and requiring a new history action to be created. This is useful for text boxes where you don't
    /// really want to undo each character typed, and instead, undo blocks of changes.
    /// </para>
    /// <para>
    /// Modifying the history action before the time is up will push the time forward, meaning if the action was a
    /// text box undo action, then constantly typing non-stop would mean there would only be 1 action to undo until you
    /// stop typing for N amount of time
    /// </para>
    /// <para>
    /// This class should typically be treated as a singleton for a specific property, e.g. a
    /// single instance for a single text box, and another instance for another text box
    /// </para>
    /// </summary>
    public class DelayedEnqueuement<T> where T : class, IHistoryAction {
        private readonly PropertyChangedEventHandler propertyChangedEventHandler;
        private readonly HistoryActionModel.RemovedEventHandler removedHandler;
        private readonly HistoryActionModel.UndoEventHandler undoHandler;
        private readonly bool canAttachPropertyChangedEvent;
        private readonly long pushBackGapTime;
        private readonly long pushBackTime;
        private readonly long timeoutTime;

        private long expiryTime;
        private HistoryActionModel currentAction;
        private INotifyPropertyChanged propertyChanged;

        private bool HasTimeExpired => this.TimeUntilExpiry <= 0;
        private long TimeUntilExpiry => this.expiryTime - Time.GetSystemMillis();
        private bool IsActive => this.currentAction != null;

        /// <summary>
        /// Creates a new <see cref="DelayedEnqueuement{T}"/>
        /// </summary>
        /// <param name="timeoutTime">The amount of time until the action expires and cannot be modified anymore</param>
        /// <param name="pushBackTime">The amount of time to be added to the internal expiry time when the action is modified again</param>
        /// <param name="pushBackGapTime">Only add <see cref="pushBackTime"/> when the time until expiry is less than this value</param>
        /// <param name="canAttachPropertyChangedEvent">Attempt to hook onto the given action's <see cref="INotifyPropertyChanged"/> event, if possible, and then invoke <see cref="OnModified"/> each time a property is modified</param>
        public DelayedEnqueuement(long timeoutTime = 3000L, long pushBackTime = 1500L, long pushBackGapTime = 1000L, bool canAttachPropertyChangedEvent = false) {
            this.timeoutTime = timeoutTime;
            this.pushBackTime = pushBackTime;
            this.pushBackGapTime = pushBackGapTime;
            this.canAttachPropertyChangedEvent = canAttachPropertyChangedEvent;
            this.propertyChangedEventHandler = this.OnPropertyChanged;
            this.undoHandler = this.OnActionUndo;
            this.removedHandler = this.OnActionRemoved;
        }

        public bool TryGetAction(out T action) {
            if (this.currentAction == null) {
                action = default;
                return false;
            }

            if (this.HasTimeExpired) {
                this.OnExpired();
                action = default;
                return false;
            }
            else {
                Debug.Assert(!this.currentAction.IsRemoved, "Did not expect the current action to be removed; the event should have been fired");
                action = (T) this.currentAction.Action;
                if (this.TimeUntilExpiry <= this.pushBackGapTime) {
                    this.expiryTime += this.pushBackTime;
                }

                return true;
            }
        }

        /// <summary>
        /// Increments the internal expiry time by the "pushBackTime" supplied to <see cref="PushAction"/>. This is called
        /// by <see cref="TryGetAction"/> when it returns true, so there typically isn't a need to call this method
        /// </summary>
        public void OnModified() {
            if (this.currentAction == null) {
                return;
            }

            long time = this.TimeUntilExpiry;
            if (time <= 0) {
                this.OnExpired();
            }
            else if (time <= this.pushBackGapTime) {
                this.expiryTime += this.pushBackTime;
            }
        }

        /// <summary>
        /// Adds the given action to the given manager
        /// </summary>
        /// <param name="manager">Target manager</param>
        /// <param name="action">Action to push</param>
        /// <param name="information">The information to pass to the manager</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void PushAction(HistoryManagerViewModel manager, T action, string information = null) {
            if (this.currentAction != null) {
                throw new InvalidOperationException($"Action time has not expired. {nameof(this.TryGetAction)} should be invoked before this function");
            }

            this.currentAction = manager.AddAction(action, information).Model;
            this.currentAction.Undo += this.undoHandler;
            this.currentAction.Removed += this.removedHandler;
            this.expiryTime = Time.GetSystemMillis() + this.timeoutTime;
            if (this.canAttachPropertyChangedEvent && action is INotifyPropertyChanged changed) {
                this.propertyChanged = changed;
                this.propertyChanged.PropertyChanged += this.propertyChangedEventHandler;
            }

            // if (this.task != null) {
            //     this.taskCancel?.Cancel();
            // }
            // this.taskCancel = new CancellationTokenSource();
            // this.task = Task.Factory.StartNew(async () => {
            //     await this.TimerMain(this.taskCancel);
            // }, this.taskCancel.Token, TaskCreationOptions.LongRunning, TaskScheduler.Current);
        }

        private void OnActionUndo(HistoryActionModel action) {
            Debug.Assert(ReferenceEquals(action, this.currentAction), "Expected action and current action to match");
            this.OnExpired();
        }

        private void OnActionRemoved(HistoryActionModel action) {
            Debug.Assert(ReferenceEquals(action, this.currentAction), "Expected action and current action to match");
            this.OnExpired();
        }

        private void OnExpired() {
            if (this.propertyChanged != null) {
                this.propertyChanged.PropertyChanged -= this.propertyChangedEventHandler;
                this.propertyChanged = null;
            }

            this.currentAction.Undo -= this.undoHandler;
            this.currentAction.Removed -= this.removedHandler;
            this.currentAction = null;
            this.expiryTime = 0;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e) {
            this.OnModified();
        }
    }
}