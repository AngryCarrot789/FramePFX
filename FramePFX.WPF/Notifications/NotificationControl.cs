using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Notifications;

namespace FramePFX.WPF.Notifications {
    public class NotificationControl : ContentControl {
        public NotificationControl() {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            if (this.DataContext is NotificationViewModel notification) {
                notification.CancelAutoHideTask();
            }

            this.Opacity = 1d;
        }

        // protected override void OnMouseLeave(MouseEventArgs e) {
        //     base.OnMouseLeave(e);
        //     if (this.DataContext is NotificationViewModel notification) {
        //         notification.StartAutoHideTask();
        //     }
        // }
    }
}