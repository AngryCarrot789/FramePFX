using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.AdvancedMenuService.WPF {
    /// <summary>
    /// A menu item that captures the data context of the keyboard's focused element before this menu item shows its items,
    /// by processing the preview mouse down event (where focus has not been transferred to this instance yet).
    /// Child menu items can access the <see cref="CapturedContextDataProperty"/> (which is inherited) to get the data
    /// </summary>
    public class ContextCapturingMenuItem : MenuItem {
        public static readonly DependencyProperty CapturedContextDataProperty = DependencyProperty.RegisterAttached("CapturedContextData", typeof(IDataContext), typeof(ContextCapturingMenuItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public ContextCapturingMenuItem() {
            this.Unloaded += this.OnUnloaded;
        }

        public static void SetCapturedContextData(DependencyObject element, IDataContext value) {
            element.SetValue(CapturedContextDataProperty, value);
        }

        public static IDataContext GetCapturedContextData(DependencyObject element) {
            return (IDataContext) element.GetValue(CapturedContextDataProperty);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            if (Keyboard.FocusedElement is DependencyObject focused && focused != this) {
                SetCapturedContextData(this, DataManager.EvaluateContextData(focused));
            }
            else {
                this.ClearValue(CapturedContextDataProperty);
            }
        }

        // Prevent possible memory leaks by dereferencing the object when this menu item closes
        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.ClearValue(CapturedContextDataProperty);
        }
    }
}