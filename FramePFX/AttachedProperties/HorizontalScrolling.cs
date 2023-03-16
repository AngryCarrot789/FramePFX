using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FramePFX.AttachedProperties {
    /// <summary>
    /// A class for allowing horizontal scrolling on any control that has a scrollviewer
    /// </summary>
    public static class HorizontalScrolling {
        public static readonly DependencyProperty UseHorizontalScrollingProperty =
            DependencyProperty.RegisterAttached(
                "UseHorizontalScrolling",
                typeof(bool),
                typeof(HorizontalScrolling),
                new PropertyMetadata(false, OnHorizontalScrollingValueChanged));

        public static readonly DependencyProperty ForceHorizontalScrollingProperty =
            DependencyProperty.RegisterAttached(
                "ForceHorizontalScrolling",
                typeof(bool),
                typeof(HorizontalScrolling),
                new PropertyMetadata(OnHorizontalScrollingValueChanged));

        public static readonly DependencyProperty HorizontalScrollingAmountProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalScrollingAmount",
                typeof(int),
                typeof(HorizontalScrolling),
                new PropertyMetadata(3)); // Forms.SystemInformation.MouseWheelScrollLines == 3 by default

        public static bool GetUseHorizontalScrolling(DependencyObject d) {
            return (bool) d.GetValue(UseHorizontalScrollingProperty);
        }

        public static void SetUseHorizontalScrolling(DependencyObject d, bool value) {
            d.SetValue(UseHorizontalScrollingProperty, value);
        }

        public static bool GetForceHorizontalScrollingValue(DependencyObject d) {
            return (bool) d.GetValue(ForceHorizontalScrollingProperty);
        }

        public static void SetForceHorizontalScrollingValue(DependencyObject d, bool value) {
            d.SetValue(ForceHorizontalScrollingProperty, value);
        }

        public static int GetHorizontalScrollingAmountValue(DependencyObject d) {
            return (int) d.GetValue(HorizontalScrollingAmountProperty);
        }

        public static void SetHorizontalScrollingAmountValue(DependencyObject d, int value) {
            d.SetValue(HorizontalScrollingAmountProperty, value);
        }

        public static void OnHorizontalScrollingValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) {
            if (sender is UIElement element) {
                element.PreviewMouseWheel -= OnPreviewMouseWheel;
                if ((bool) e.NewValue) {
                    element.PreviewMouseWheel += OnPreviewMouseWheel;
                }
            }
            else {
                throw new Exception("Attached property must be used with UIElement");
            }
        }

        private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args) {
            if (sender is UIElement element) {
                ScrollViewer scollViewer = FindDescendant<ScrollViewer>(element);
                if (scollViewer == null) {
                    return;
                }

                int amount = GetHorizontalScrollingAmountValue(element);
                if (amount < 1) {
                    amount = 3;
                }

                if (Keyboard.Modifiers == ModifierKeys.Shift || Mouse.MiddleButton == MouseButtonState.Pressed || GetForceHorizontalScrollingValue(element)) {
                    if (args.Delta < 0) {
                        for (int i = 0; i < amount; i++) {
                            scollViewer.LineRight();
                        }
                    }
                    else {
                        for (int i = 0; i < amount; i++) {
                            scollViewer.LineLeft();
                        }
                    }

                    args.Handled = true;
                }
            }
        }

        private static T FindDescendant<T>(DependencyObject d) where T : DependencyObject {
            if (d == null) {
                return null;
            }
            else if (d is T t) {
                return t;
            }
            else {
                int count = VisualTreeHelper.GetChildrenCount(d);
                for (int i = 0; i < count; i++) {
                    DependencyObject child = VisualTreeHelper.GetChild(d, i);
                    T result = child as T ?? FindDescendant<T>(child);
                    if (result != null) {
                        return result;
                    }
                }

                return null;
            }
        }
    }
}