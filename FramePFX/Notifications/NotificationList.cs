using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Notifications
{
    public class NotificationList : ItemsControl
    {
        public NotificationList()
        {
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is NotificationControl;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new NotificationControl();
        }
    }
}