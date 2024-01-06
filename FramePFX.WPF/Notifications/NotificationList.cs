using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.WPF.DataTemplates;

namespace FramePFX.WPF.Notifications {
    public class NotificationList : ItemsControl {
        public static DataTemplateManager NotificationTemplateManager { get; } = new DataTemplateManager();

        public NotificationList() {
            try {
                this.Resources.MergedDictionaries.Add(NotificationTemplateManager.ResourceDictionary);
            }
            catch (Exception e) {
                throw new Exception("Failed to add merged dictionary", e);
            }
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is NotificationControl;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new NotificationControl();
        }
    }
}