using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Notifications.Types;

namespace FramePFX.Notifications {
    public class NotificationDataTemplateSelector : DataTemplateSelector {
        public DataTemplate MessageNotificationTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case MessageNotification _: return this.MessageNotificationTemplate;
                default: {
                    return base.SelectTemplate(item, container);
                }
            }
        }
    }
}