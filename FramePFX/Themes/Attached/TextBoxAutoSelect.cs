using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace FramePFX.Themes.Attached {
    public static class TextBoxAutoSelect {
        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.RegisterAttached(
                "SelectAllOnFocus",
                typeof(bool),
                typeof(TextBoxAutoSelect),
                new FrameworkPropertyMetadata(false, OnSelectAllOnFocusPropertyChanged));

        public static void SetSelectAllOnFocus(DependencyObject obj, bool value) {
            obj.SetValue(SelectAllOnFocusProperty, value);
        }

        public static bool GetSelectAllOnFocus(DependencyObject obj) {
            return (bool) obj.GetValue(SelectAllOnFocusProperty);
        }

        private static readonly RoutedEventHandler FocusHandler = BoxOnGotFocus;

        private static void OnSelectAllOnFocusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is TextBoxBase tb) {
                tb.GotFocus -= FocusHandler;
                if ((bool) e.NewValue) {
                    tb.GotFocus += FocusHandler;
                }
            }
        }

        private static void BoxOnGotFocus(object sender, RoutedEventArgs e) {
            if (sender is TextBoxBase tb) {
                tb.Dispatcher.InvokeAsync(() => {
                    if (GetSelectAllOnFocus(tb)) {
                        tb.SelectAll();
                    }
                });
            }
        }
    }
}