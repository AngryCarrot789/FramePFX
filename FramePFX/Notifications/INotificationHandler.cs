using System;

namespace FramePFX.Notifications {
    // TODO: remove this and replace with something a bit less explicit, e.g. main window handling notification added/remove events
    public interface INotificationHandler {
        void OnNotificationPushed(NotificationViewModel notification);
        void OnNotificationRemoved(NotificationViewModel notification);
        void BeginNotificationFadeOutAnimation(NotificationViewModel notification, Action<NotificationViewModel, bool> onCompleteCallback = null);
        void CancelNotificationFadeOutAnimation(NotificationViewModel notification);
    }
}