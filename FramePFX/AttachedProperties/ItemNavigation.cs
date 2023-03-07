using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FrameControl.AttachedProperties {
    public static class ItemNavigation {
        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.RegisterAttached("DoubleClickCommand", typeof(ICommand), typeof(ItemNavigation), new PropertyMetadata(null, OnDoubleClickCommandChanged));

        public static void SetDoubleClickCommand(DependencyObject element, ICommand value) {
            element.SetValue(DoubleClickCommandProperty, value);
        }

        public static ICommand GetDoubleClickCommand(DependencyObject element) {
            return (ICommand) element.GetValue(DoubleClickCommandProperty);
        }

        private static readonly MouseButtonEventHandler Handler = ControlOnMouseDoubleClick;

        private static void OnDoubleClickCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Control control) {
                control.MouseDoubleClick -= Handler;
                if (e.NewValue != null) {
                    control.MouseDoubleClick += Handler;
                }
            }
        }

        private static void ControlOnMouseDoubleClick(object sender, MouseButtonEventArgs e) {
            if (sender is Control control) {
                object parameter = control.DataContext;
                ICommand command = GetDoubleClickCommand(control);
                if (command != null && command.CanExecute(parameter)) {
                    command.Execute(parameter);
                }
            }
        }
    }
}
