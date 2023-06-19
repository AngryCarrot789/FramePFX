using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Editor.Notifications;
using FramePFX.Core.Notifications.Types;

namespace FramePFX.Notifications {
    public class NotificationDataTemplateSelector : DataTemplateSelector {
        public DataTemplate MessageNotificationTemplate { get; set; }
        
        public DataTemplate SavingProjectNotificationTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case SavingProjectNotification _: return this.SavingProjectNotificationTemplate;
                case MessageNotification _: return this.MessageNotificationTemplate;
                default: {
                    return base.SelectTemplate(item, container);
                }
            }
        }
    }
}