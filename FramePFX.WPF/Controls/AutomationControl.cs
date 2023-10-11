using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Automation.ViewModels.Keyframe;

namespace FramePFX.WPF.Controls {
    /// <summary>
    /// A content control that listens to focus events in order to make an automation sequence active
    /// </summary>
    public class AutomationControl : ContentControl {
        public static readonly DependencyProperty FocusableAutomationSequenceProperty = DependencyProperty.RegisterAttached("FocusableAutomationSequence", typeof(AutomationSequenceViewModel), typeof(AutomationControl), new PropertyMetadata(null, OnAutomationSequencePropertyChanged));
        public static readonly DependencyProperty MousePressAutomationSequenceProperty = DependencyProperty.RegisterAttached("MousePressAutomationSequence", typeof(AutomationSequenceViewModel), typeof(AutomationControl), new PropertyMetadata(null, OnMouseClickAutomationSequencePropertyChanged));

        private static readonly RoutedEventHandler GotFocusHandler;
        private static readonly MouseButtonEventHandler PreviewMouseDownHandler;

        public AutomationControl() {
            this.Focusable = true;
        }

        static AutomationControl() {
            GotFocusHandler = OnGotFocus;
            PreviewMouseDownHandler = OnPreviewMouseDown;
        }

        private static void OnAutomationSequencePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null)
                ((UIElement) d).GotFocus -= GotFocusHandler;
            if (e.NewValue != null)
                ((UIElement) d).GotFocus += GotFocusHandler;
        }

        private static void OnMouseClickAutomationSequencePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue != null)
                ((UIElement) d).PreviewMouseLeftButtonDown -= PreviewMouseDownHandler;
            if (e.NewValue != null)
                ((UIElement) d).PreviewMouseLeftButtonDown += PreviewMouseDownHandler;
        }

        public static void SetFocusableAutomationSequence(DependencyObject element, AutomationSequenceViewModel value) {
            element.SetValue(FocusableAutomationSequenceProperty, value);
        }

        public static AutomationSequenceViewModel GetFocusableAutomationSequence(DependencyObject element) {
            return (AutomationSequenceViewModel) element.GetValue(FocusableAutomationSequenceProperty);
        }

        public static void SetMousePressAutomationSequence(DependencyObject element, AutomationSequenceViewModel value) {
            element.SetValue(MousePressAutomationSequenceProperty, value);
        }

        public static AutomationSequenceViewModel GetMousePressAutomationSequence(DependencyObject element) {
            return (AutomationSequenceViewModel) element.GetValue(MousePressAutomationSequenceProperty);
        }

        private static void OnGotFocus(object sender, RoutedEventArgs e) {
            if (((DependencyObject) sender).GetValue(FocusableAutomationSequenceProperty) is AutomationSequenceViewModel sequence) {
                sequence.IsActiveSequence = true;
            }
        }

        private static void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.LeftButton == MouseButtonState.Pressed) {
                if (((DependencyObject) sender).GetValue(MousePressAutomationSequenceProperty) is AutomationSequenceViewModel sequence) {
                    sequence.IsActiveSequence = true;
                }
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            if (this.IsMouseOver && this.Focusable) {
                this.Focus();
            }
        }
    }
}