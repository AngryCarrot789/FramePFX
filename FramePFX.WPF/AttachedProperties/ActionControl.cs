using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Utils;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.AttachedProperties {
    public static class ActionControl {
        public static readonly DependencyProperty TargetActionIdProperty = DependencyProperty.RegisterAttached("TargetActionId", typeof(string), typeof(ActionControl), new PropertyMetadata(null, PropertyChangedCallback));
        private static readonly DependencyPropertyKey PresentationUpdateHandlerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("PresentationUpdateHandler", typeof(UpdateHandler), typeof(ActionControl), new PropertyMetadata(default(CanExecuteChangedEventHandler)));

        public static void SetTargetActionId(DependencyObject element, string value) => element.SetValue(TargetActionIdProperty, value);
        public static string GetTargetActionId(DependencyObject element) => (string) element.GetValue(TargetActionIdProperty);

        private static UpdateHandler GetPresentationUpdateHandler(DependencyObject element) {
            return (UpdateHandler) element.GetValue(PresentationUpdateHandlerPropertyKey.DependencyProperty);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ButtonBase button) {
                button.Click -= OnControlClickForInvokeAction;
                if (GetPresentationUpdateHandler(button) is UpdateHandler oldHandler) {
                    button.ClearValue(PresentationUpdateHandlerPropertyKey);
                    ActionManager.Instance.RemovePresentationUpdateHandler(oldHandler.ActionId, oldHandler.Handler);
                }

                if (e.NewValue is string newId && !string.IsNullOrWhiteSpace(newId)) {
                    button.Click += OnControlClickForInvokeAction;
                    SetupHandler(button, newId);
                }
            }
            else if (d is MenuItem menuItem) {
                menuItem.Click -= OnControlClickForInvokeAction;
                if (GetPresentationUpdateHandler(menuItem) is UpdateHandler oldHandler) {
                    menuItem.ClearValue(PresentationUpdateHandlerPropertyKey);
                    ActionManager.Instance.RemovePresentationUpdateHandler(oldHandler.ActionId, oldHandler.Handler);
                }

                if (e.NewValue is string newId && !string.IsNullOrWhiteSpace(newId)) {
                    menuItem.Click += OnControlClickForInvokeAction;
                    SetupHandler(menuItem, newId);
                }
            }
        }

        private static void SetupHandler(UIElement element, string actionId) {
            UpdateHandler newHandler = new UpdateHandler(actionId, (id, action, args, canExecute) => {
                element.IsEnabled = canExecute;
            });

            element.SetValue(PresentationUpdateHandlerPropertyKey, newHandler);
            ActionManager.Instance.AddPresentationUpdateHandler(actionId, newHandler.Handler);
        }

        private static async void OnControlClickForInvokeAction(object sender, RoutedEventArgs e) {
            if (sender is ButtonBase || sender is MenuItem) {
                Control element = (Control) sender;
                string actionId = GetTargetActionId(element);
                if (string.IsNullOrWhiteSpace(actionId)) {
                    return;
                }

                DataContext context = new DataContext();
                object o1, o2 = null, o3;
                if (VisualTreeUtils.GetDataContext(element, out o1)) {
                    context.AddContext(o1);
                }

                ItemsControl itemsControl = VisualTreeUtils.GetItemsControlFromObject(element);
                if (itemsControl != null && (o2 = itemsControl.DataContext) != null && !ReferenceEquals(o1, o2)) {
                    context.AddContext(o2);
                }

                if (Window.GetWindow(element) is Window window && VisualTreeUtils.GetDataContext(window, out o3)) {
                    if (!ReferenceEquals(o1, o3) && !ReferenceEquals(o2, o3)) {
                        context.AddContext(o3);
                    }
                }

                if (element is ToggleButton toggle) {
                    object obj = toggle.GetValue(ToggleButton.IsCheckedProperty);
                    if (obj is bool isChecked) {
                        context.Set(ToggleAction.IsToggledKey, isChecked.Box());
                    }
                }
                else if (element is MenuItem menuItem && menuItem.IsCheckable) {
                    context.Set(ToggleAction.IsToggledKey, menuItem.IsChecked.Box());
                }

                await ActionManager.Instance.Execute(actionId, context, true);
            }
        }

        private class UpdateHandler {
            public string ActionId { get; }

            public CanExecuteChangedEventHandler Handler { get; }

            public UpdateHandler(string actionId, CanExecuteChangedEventHandler handler) {
                this.ActionId = actionId;
                this.Handler = handler;
            }
        }
    }
}