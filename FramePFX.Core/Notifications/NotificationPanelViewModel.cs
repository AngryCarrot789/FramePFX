using System;
using System.Collections.ObjectModel;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Notifications {
    /// <summary>
    /// A view model for a panel that can display notifications
    /// </summary>
    public class NotificationPanelViewModel : BaseViewModel {
        private readonly ObservableCollectionEx<NotificationViewModel> notifications;

        /// <summary>
        /// A collection of notifications that are currently on screen
        /// </summary>
        public ReadOnlyObservableCollection<NotificationViewModel> Notifications { get; }

        public INotificationHandler Handler { get; }

        public NotificationPanelViewModel(INotificationHandler handler) {
            this.Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            this.notifications = new ObservableCollectionEx<NotificationViewModel>();
            this.Notifications = new ReadOnlyObservableCollection<NotificationViewModel>(this.notifications);
        }

        public void PushNotification(NotificationViewModel notification, bool autoStartHideTask = true) {
            if (notification == null)
                throw new ArgumentNullException(nameof(notification));
            if (notification.Panel != null && !ReferenceEquals(notification.Panel, this))
                throw new Exception("The given notification was already placed in a panel");

            this.notifications.Add(notification);
            notification.Panel = this;
            if (autoStartHideTask) {
                notification.StartAutoHideTask();
            }

            this.Handler.OnNotificationPushed(notification);
        }

        public void RemoveNotification(NotificationViewModel notification) {
            if (this.notifications.Remove(notification)) {
                this.Handler.OnNotificationRemoved(notification);
            }
        }

        public bool IsNotificationPresent(NotificationViewModel notification) {
            return this.notifications.Contains(notification);
        }
    }
}