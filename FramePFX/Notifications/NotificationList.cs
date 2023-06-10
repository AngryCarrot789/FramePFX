using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Notifications;

namespace FramePFX.Notifications {
    public class NotificationList : ItemsControl, INotificationHandler {
        public NotificationList() {

        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is NotificationControl;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new NotificationControl();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
        }

        public void OnNotificationPushed(NotificationViewModel notification) {
            throw new NotImplementedException();
        }

        public void OnNotificationRemoved(NotificationViewModel notification) {
            throw new NotImplementedException();
        }

        public void BeginNotificationFadeOutAnimation(NotificationViewModel notification, Action<NotificationViewModel> onCompleteCallback = null) {
            throw new NotImplementedException();
        }

        public void CancelNotificationFadeOutAnimation(NotificationViewModel notification) {
            throw new NotImplementedException();
        }
    }
}