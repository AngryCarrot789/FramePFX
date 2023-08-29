using System.Windows;
using System.Windows.Controls;
using FramePFX.Editor.Notifications;
using FramePFX.History;
using FramePFX.Notifications.Types;

namespace FramePFX.WPF.Notifications {
    public class NotificationDataTemplateSelector : DataTemplateSelector {
        public DataTemplate MessageNotificationTemplate { get; set; }
        public DataTemplate SavingProjectNotificationTemplate { get; set; }
        public DataTemplate HistoryNotificationTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (item) {
                case SavingProjectNotification _: return this.SavingProjectNotificationTemplate;
                case HistoryNotification _: return this.HistoryNotificationTemplate;
                case MessageNotification _: return this.MessageNotificationTemplate;
                default: {
                    return base.SelectTemplate(item, container);
                }
            }
        }
    }
}