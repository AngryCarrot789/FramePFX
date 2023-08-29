using System.Windows;

namespace FramePFX.WPF.Controls {
    public static class FocusHelper {
        public static readonly DependencyProperty FocusOnLoadedProperty = DependencyProperty.RegisterAttached("FocusOnLoaded", typeof(bool), typeof(FocusHelper), new PropertyMetadata(false, OnPropChanged));

        public static bool GetFocusOnLoaded(DependencyObject obj) {
            return (bool) obj.GetValue(FocusOnLoadedProperty);
        }

        public static void SetFocusOnLoaded(DependencyObject obj, bool value) {
            obj.SetValue(FocusOnLoadedProperty, value);
        }

        private static void OnPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is FrameworkElement element) {
                element.Loaded -= Element_Loaded;
                if (e.NewValue is bool b && b) {
                    element.Loaded += Element_Loaded;
                }
            }
        }

        private static void Element_Loaded(object sender, RoutedEventArgs e) {
            if (sender is FrameworkElement element && element.Focusable && GetFocusOnLoaded(element)) {
                element.Focus();
            }
        }
    }
}