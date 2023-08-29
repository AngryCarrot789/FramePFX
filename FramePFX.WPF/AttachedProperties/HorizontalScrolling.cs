using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Utils;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.AttachedProperties {
    public static class HorizontalScrolling {
        public static readonly DependencyProperty UseHorizontalScrollingProperty = DependencyProperty.RegisterAttached("UseHorizontalScrolling", typeof(bool), typeof(HorizontalScrolling), new PropertyMetadata(BoolBox.False, OnUseHorizontalScrollWheelPropertyChanged));
        public static readonly DependencyProperty IsRequireShiftForHorizontalScrollProperty = DependencyProperty.RegisterAttached("IsRequireShiftForHorizontalScroll", typeof(bool), typeof(HorizontalScrolling), new PropertyMetadata(BoolBox.True));
        public static readonly DependencyProperty ForceHorizontalScrollingProperty = DependencyProperty.RegisterAttached("ForceHorizontalScrolling", typeof(bool), typeof(HorizontalScrolling), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty HorizontalScrollingAmountProperty = DependencyProperty.RegisterAttached("HorizontalScrollingAmount", typeof(int), typeof(HorizontalScrolling), new PropertyMetadata(SystemParameters.WheelScrollLines));

        public static void SetUseHorizontalScrolling(DependencyObject element, bool value) => element.SetValue(UseHorizontalScrollingProperty, value);
        public static bool GetUseHorizontalScrolling(DependencyObject element) => (bool) element.GetValue(UseHorizontalScrollingProperty);

        public static void SetIsRequireShiftForHorizontalScroll(DependencyObject element, bool value) => element.SetValue(IsRequireShiftForHorizontalScrollProperty, value);
        public static bool GetIsRequireShiftForHorizontalScroll(DependencyObject element) => (bool) element.GetValue(IsRequireShiftForHorizontalScrollProperty);

        public static bool GetForceHorizontalScrollingValue(DependencyObject d) => (bool) d.GetValue(ForceHorizontalScrollingProperty);
        public static void SetForceHorizontalScrollingValue(DependencyObject d, bool value) => d.SetValue(ForceHorizontalScrollingProperty, value);

        public static int GetHorizontalScrollingAmountValue(DependencyObject d) => (int) d.GetValue(HorizontalScrollingAmountProperty);
        public static void SetHorizontalScrollingAmountValue(DependencyObject d, int value) => d.SetValue(HorizontalScrollingAmountProperty, value);

        private static void OnUseHorizontalScrollWheelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is UIElement element) {
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
                if ((bool) e.NewValue) {
                    element.PreviewMouseWheel += OnPreviewMouseWheel;
                }
            }
            else {
                throw new Exception("Attached property must be used with UIElement");
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            if (sender is UIElement element && e.Delta != 0) {
                ScrollViewer scroller = VisualTreeUtils.FindVisualChild<ScrollViewer>(element);
                if (scroller == null) {
                    return;
                }

                if (GetIsRequireShiftForHorizontalScroll(element) && scroller.HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled) {
                    return;
                }

                int amount = GetHorizontalScrollingAmountValue(element);
                if (amount < 1) {
                    amount = 3;
                }

                if (Keyboard.Modifiers == ModifierKeys.Shift || Mouse.MiddleButton == MouseButtonState.Pressed || GetForceHorizontalScrollingValue(element)) {
                    int count = (e.Delta / 120) * amount;
                    if (e.Delta < 0) {
                        for (int i = 0; i < count; i++) {
                            scroller.LineRight();
                        }
                    }
                    else {
                        for (int i = 0; i < count; i++) {
                            scroller.LineLeft();
                        }
                    }

                    e.Handled = true;
                }
            }
        }
    }
}