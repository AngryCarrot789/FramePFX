using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameControlEx.Controls.Helpers {
    public static class ScrollViewerHelper {
        public static readonly DependencyProperty UseHorizontalScrollWheelProperty = DependencyProperty.RegisterAttached("UseHorizontalScrollWheel", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(false, OnUseHorizontalScrollWheelPropertyChanged));
        public static readonly DependencyProperty IsRequireShiftForHorizontalScrollProperty = DependencyProperty.RegisterAttached("IsRequireShiftForHorizontalScroll", typeof(bool), typeof(ScrollViewerHelper), new PropertyMetadata(true));

        public static void SetUseHorizontalScrollWheel(DependencyObject element, bool value) {
            element.SetValue(UseHorizontalScrollWheelProperty, value);
        }

        public static bool GetUseHorizontalScrollWheel(DependencyObject element) {
            return (bool) element.GetValue(UseHorizontalScrollWheelProperty);
        }

        public static void SetIsRequireShiftForHorizontalScroll(DependencyObject element, bool value) {
            element.SetValue(IsRequireShiftForHorizontalScrollProperty, value);
        }

        public static bool GetIsRequireShiftForHorizontalScroll(DependencyObject element) {
            return (bool) element.GetValue(IsRequireShiftForHorizontalScrollProperty);
        }

        private static void OnUseHorizontalScrollWheelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ScrollViewer scrollViewer && e.NewValue != e.OldValue && e.NewValue is bool use) {
                scrollViewer.PreviewMouseWheel -= OnScrollViewerPreviewMouseWheel;
                if (use) {
                    scrollViewer.PreviewMouseWheel += OnScrollViewerPreviewMouseWheel;
                }
            }
        }

        private static void OnScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is ScrollViewer view && view.HorizontalScrollBarVisibility != ScrollBarVisibility.Disabled) {
                if (GetIsRequireShiftForHorizontalScroll(view) && (Keyboard.Modifiers & ModifierKeys.Shift) == 0) {
                    return;
                }

                if (e.Delta > 0) {
                    view.LineLeft();
                    view.LineLeft();
                }
                else if (e.Delta < 0) {
                    view.LineRight();
                    view.LineRight();
                }

                e.Handled = true;
            }
        }
    }
}