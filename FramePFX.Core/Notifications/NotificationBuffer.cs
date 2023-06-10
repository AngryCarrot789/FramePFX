using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FramePFX.Core.History;
using FramePFX.Core.History.ViewModels;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Notifications {
    public class NotificationBuffer<T> where T : NotificationViewModel {
        private readonly long pushBackGapTime;
        private readonly long pushBackTime;
        private readonly long timeoutTime;
        private readonly object locker;

        private long expiryTime;
        private volatile CancellationTokenSource cancellationTokenSource;
        private volatile Task expirationTask;
        private T currentNotification;

        private long TimeUntilExpiry => this.expiryTime - Time.GetSystemMillis();

        /// <summary>
        /// Creates a new <see cref="NotificationBuffer{T}"/>
        /// </summary>
        /// <param name="timeoutTime">The amount of time until the action expires and cannot be modified anymore</param>
        /// <param name="pushBackTime">The amount of time to be added to the internal expiry time when the action is modified again</param>
        /// <param name="pushBackGapTime">Only add <see cref="pushBackTime"/> when the time until expiry is less than this value</param>
        public NotificationBuffer(long timeoutTime = 3000L, long pushBackTime = 1500L, long pushBackGapTime = 1000L) {
            this.locker = new object();
            this.timeoutTime = timeoutTime;
            this.pushBackTime = pushBackTime;
            this.pushBackGapTime = pushBackGapTime;
        }

        /// <summary>
        /// Tries to get the current action, and if it exists, the expiry time may be pushed back a little bit
        /// </summary>
        /// <param name="action">The existing actions</param>
        /// <param name="useLockForPushActionCall">Locks the state of the object, expecting <see cref="PushAction"/> to be invoked if this method returns false</param>
        /// <returns>True if the action exists, setting the parameter to a non-null value. Otherwise false if the action expired at some point since the last invocation</returns>
        public bool TryGetAction(out T action) {
            lock (this.locker) {
                if (this.currentNotification == null) {
                    action = default;
                    return false;
                }

                long time = this.TimeUntilExpiry;
                if (time <= 0 || this.currentNotification.IsHidden) {
                    this.OnExpired();
                    action = default;
                    return false;
                }
                else {
                    action = this.currentNotification;
                    if (time <= this.pushBackGapTime) {
                        this.expiryTime += this.pushBackTime;
                    }

                    return true;
                }
            }
        }

        /// <summary>
        /// Adds the given action to the given manager
        /// </summary>
        /// <param name="panel">Target manager</param>
        /// <param name="action">Action to push</param>
        /// <param name="information">The information to pass to the manager</param>
        /// <exception cref="InvalidOperationException"></exception>
        public void PushAction(NotificationPanelViewModel panel, T action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (this.locker) {
                if (this.currentNotification != null) {
                    throw new InvalidOperationException($"Action time has not expired. {nameof(this.TryGetAction)} should be invoked before this function");
                }

                this.currentNotification = action;
                panel.PushNotification(action);
                this.expiryTime = Time.GetSystemMillis() + this.timeoutTime;
                if (this.expirationTask == null) {
                    this.cancellationTokenSource = new CancellationTokenSource();
                    this.expirationTask = Task.Run(() => this.ExpireActionAsync(this.cancellationTokenSource.Token));
                }
            }
        }

        /// <summary>
        /// Increments the internal expiry time by the "pushBackTime" supplied to <see cref="PushAction"/>. This is called
        /// by <see cref="TryGetAction"/> when it returns true, so there typically isn't a need to call this method
        /// </summary>
        public void OnModified() {
            lock (this.locker) {
                if (this.currentNotification == null) {
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
        }

        private async Task ExpireActionAsync(CancellationToken token) {
            do {
                long time;
                lock (this.locker) {
                    time = this.TimeUntilExpiry;
                }

                if (time > 0) {
                    await Task.Delay((int) time, token);
                }

                lock (this.locker) {
                    if (!token.IsCancellationRequested && this.TimeUntilExpiry <= 0) {
                        this.OnExpired();
                        return;
                    }
                }
            } while (true);
        }

        private void OnExpired() {
            this.currentNotification = null;
            this.expiryTime = 0;

            try {
                this.cancellationTokenSource?.Cancel();
            }
            catch { /* ignored */ }
            finally {
                this.cancellationTokenSource = null;
                this.expirationTask = null;
            }
        }
    }
}