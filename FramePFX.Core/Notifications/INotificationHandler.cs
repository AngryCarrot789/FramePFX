using System;

namespace FramePFX.Core.Notifications
{
    public interface INotificationHandler
    {
        void OnNotificationPushed(NotificationViewModel notification);
        void OnNotificationRemoved(NotificationViewModel notification);
        void BeginNotificationFadeOutAnimation(NotificationViewModel notification, Action<NotificationViewModel, bool> onCompleteCallback = null);
        void CancelNotificationFadeOutAnimation(NotificationViewModel notification);
    }
}