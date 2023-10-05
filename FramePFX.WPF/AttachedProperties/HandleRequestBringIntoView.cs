using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;

namespace FramePFX.WPF.AttachedProperties {
    public static class HandleRequestBringIntoView {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(HandleRequestBringIntoView),
                new PropertyMetadata(BoolBox.False, PropertyChangedCallback));

        public static void SetIsEnabled(DependencyObject element, bool value) {
            element.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(DependencyObject element) {
            return (bool) element.GetValue(IsEnabledProperty);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Grid grid) {
                grid.RequestBringIntoView -= GridOnRequestBringIntoView;
                if ((bool) e.NewValue) {
                    grid.RequestBringIntoView += GridOnRequestBringIntoView;
                }
            }
        }

        private static void GridOnRequestBringIntoView(object sender, RequestBringIntoViewEventArgs e) {
            // Prevent the timeline scrolling when you select a clip
            e.Handled = true;
        }
    }
}