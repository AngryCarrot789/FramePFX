using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Notifications {
    public abstract class NotificationViewModel : BaseViewModel {
        private long expiryTime;
        private TimeSpan timeout;
        private Task autoHideTask;
        private CancellationTokenSource cancellation;

        /// <summary>
        /// Gets or sets amount of time until this notification becomes hidden. An empty timespan (0 seconds)
        /// has an infinite timeout, and must be manually closed by the user
        /// </summary>
        public TimeSpan Timeout {
            get => this.timeout;
            set {
                this.timeout = value;
                this.expiryTime = value.Ticks;
            }
        }

        private bool isHidden;

        public bool IsHidden {
            get => this.isHidden;
            set => this.RaisePropertyChanged(ref this.isHidden, value);
        }

        private NotificationPanelViewModel panel;

        public NotificationPanelViewModel Panel {
            get => this.panel;
            set => this.RaisePropertyChanged(ref this.panel, value);
        }

        /// <summary>
        /// A command that forces this notification to be hidden
        /// </summary>
        public AsyncRelayCommand HideCommand { get; }

        protected NotificationViewModel() {
            this.HideCommand = new AsyncRelayCommand(this.HideAction, () => !this.IsHidden);
        }

        public void IncreaseTimeUntilExpiry(TimeSpan span) {
            this.expiryTime += span.Ticks;
        }

        public void StartAutoHideTask() {
            this.StartAutoHideTask(this.Timeout);
        }

        public virtual void StartAutoHideTask(TimeSpan span) {
            if (this.autoHideTask != null && !this.autoHideTask.IsCompleted) {
                return;
            }

            if (span.Ticks > 0) {
                this.expiryTime = span.Ticks;
                this.cancellation = new CancellationTokenSource();
                this.autoHideTask = Task.Run(() => this.HideTaskMain(this.cancellation.Token));
            }
        }

        public void CancelAutoHideTask() {
            this.Panel.Handler.CancelNotificationFadeOutAnimation(this);
            if (this.cancellation == null || this.autoHideTask == null) {
                return;
            }

            try {
                this.cancellation?.Cancel();
            }
            catch {
                /* ignored */
            }
            finally {
                this.cancellation = null;
                this.autoHideTask = null;
            }
        }

        private async Task HideTaskMain(CancellationToken cancel) {
            long oldTicks = this.expiryTime;
            while (oldTicks > 0) {
                try {
                    await Task.Delay(new TimeSpan(oldTicks), cancel);
                }
                catch (TaskCanceledException) {
                    return;
                }

                this.expiryTime -= oldTicks;
                oldTicks = this.expiryTime;
            }

            if (!cancel.IsCancellationRequested) {
                await IoC.Dispatcher.InvokeAsync(this.AutoHideAction);
            }
        }

        public virtual Task HideAction() {
            this.CancelAutoHideTask();
            this.Panel.RemoveNotification(this);
            this.IsHidden = true;
            return Task.CompletedTask;
        }

        private void AutoHideAction() {
            if (!this.IsHidden && this.panel.IsNotificationPresent(this)) {
                this.cancellation = null;
                this.autoHideTask = null;
                this.Panel.Handler.BeginNotificationFadeOutAnimation(this, this.OnNotificationNoLongerVisible);
            }
        }

        private void OnNotificationNoLongerVisible(NotificationViewModel obj, bool cancelled) {
            Debug.Assert(ReferenceEquals(obj, this), "Callback parameter does not match the current instance");
            if (cancelled) {
                return;
            }

            this.IsHidden = true;
            this.Panel.RemoveNotification(this);
        }
    }
}