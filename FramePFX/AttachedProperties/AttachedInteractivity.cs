using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Utils;

namespace FramePFX.AttachedProperties {
    public static class AttachedInteractivity {
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached("DoubleClickCommand", typeof(ICommand), typeof(AttachedInteractivity), new PropertyMetadata(null, OnDoubleClickCommandChanged));
        public static readonly DependencyProperty UseICGForParameterProperty = DependencyProperty.RegisterAttached("UseICGForParameter", typeof(bool), typeof(AttachedInteractivity), new PropertyMetadata(BoolBox.False));
        public static readonly DependencyProperty UseDataContextAsParameterProperty = DependencyProperty.RegisterAttached("UseDataContextAsParameter", typeof(bool), typeof(AttachedInteractivity), new PropertyMetadata(BoolBox.True));

        public static void SetDoubleClickCommand(DependencyObject element, ICommand value) => element.SetValue(DoubleClickCommandProperty, value);
        public static ICommand GetDoubleClickCommand(DependencyObject element) => (ICommand) element.GetValue(DoubleClickCommandProperty);

        public static void SetUseICGForParameter(DependencyObject element, bool value) => element.SetValue(UseICGForParameterProperty, value.Box());
        public static bool GetUseICGForParameter(DependencyObject element) => (bool) element.GetValue(UseICGForParameterProperty);

        public static void SetUseDataContextAsParameter(DependencyObject element, bool value) => element.SetValue(UseDataContextAsParameterProperty, value.Box());
        public static bool GetUseDataContextAsParameter(DependencyObject element) => (bool) element.GetValue(UseDataContextAsParameterProperty);

        private static readonly MouseButtonEventHandler Handler = ControlOnMouseDown;

        private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is UIElement control) {
                // if (e.OldValue != null) {
                //     int index = 0;
                //     foreach (InputBinding item in control.InputBindings) {
                //         if (item.Command == e.OldValue) {
                //             control.InputBindings.RemoveAt(index);
                //             break;
                //         }
                //         index++;
                //     }
                // }
                // control.InputBindings.Add(new MouseBinding((ICommand) e.NewValue, new MouseGesture(MouseAction.LeftDoubleClick)));
                control.PreviewMouseDown -= Handler;
                if (e.NewValue != null) {
                    control.PreviewMouseDown += Handler;
                }
            }
        }

        private static void ControlOnMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2 && sender is UIElement control) {
                object parameter;
                ICommand command = GetDoubleClickCommand(control);
                if (command != null && command.CanExecute(parameter = GetParamForCommand(control))) {
                    command.Execute(parameter);
                    e.Handled = true;
                }
            }
        }

        private static object GetParamForCommand(DependencyObject control) {
            FrameworkElement element = control as FrameworkElement;
            object result = null;
            if (element != null && GetUseICGForParameter(control)) {
                if (element.Parent is ItemsControl x1) {
                    result = x1.ItemContainerGenerator.ItemFromContainer(control);
                }
                else if (element.TemplatedParent is ItemsControl x2) {
                    result = x2.ItemContainerGenerator.ItemFromContainer(control);
                }
            }

            if (result == DependencyProperty.UnsetValue) {
                result = null;
            }

            if (result != null) {
                return result;
            }

            return GetUseDataContextAsParameter(control) ? element?.DataContext : null;
        }
    }
}