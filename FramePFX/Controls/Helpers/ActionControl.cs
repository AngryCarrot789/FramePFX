using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core.Actions;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Utils;

namespace FramePFX.Controls.Helpers {
    public static class ActionControl {
        public static readonly DependencyProperty TargetActionIdProperty = DependencyProperty.RegisterAttached("TargetActionId", typeof(string), typeof(ActionControl), new PropertyMetadata(null, PropertyChangedCallback));
        public static readonly DependencyProperty CanControlBecomeCollapsedProperty = DependencyProperty.RegisterAttached("CanControlBecomeCollapsed", typeof(bool), typeof(ActionControl), new PropertyMetadata(true));
        private static readonly DependencyPropertyKey PresentationUpdateHandlerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("PresentationUpdateHandler", typeof(UpdateHandler), typeof(ActionControl), new PropertyMetadata(default(GlobalPresentationUpdateHandler)));

        public static void SetTargetActionId(DependencyObject element, string value) => element.SetValue(TargetActionIdProperty, value);
        public static string GetTargetActionId(DependencyObject element) => (string) element.GetValue(TargetActionIdProperty);

        /// <summary>
        /// Sets whether this control can be collapsed or not when the action's presentation says it must become
        /// invisible. If the control cannot be collapsed, then it will just be marked as disabled
        /// </summary>
        public static void SetCanControlBecomeCollapsed(DependencyObject element, bool value) {
            element.SetValue(CanControlBecomeCollapsedProperty, value);
        }

        /// <summary>
        /// Gets whether this control can be collapsed or not when the action's presentation says it must become
        /// invisible. If the control cannot be collapsed, then it will just be marked as disabled
        /// </summary>
        public static bool GetCanControlBecomeCollapsed(DependencyObject element) {
            return (bool) element.GetValue(CanControlBecomeCollapsedProperty);
        }

        private static UpdateHandler GetPresentationUpdateHandler(DependencyObject element) {
            return (UpdateHandler) element.GetValue(PresentationUpdateHandlerPropertyKey.DependencyProperty);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is ButtonBase button) {
                button.Click -= OnControlClickForInvokeAction;
                if (GetPresentationUpdateHandler(button) is UpdateHandler oldHandler) {
                    ActionManager.Instance.RemovePresentationUpdateHandler(oldHandler.ActionId, oldHandler.Handler);
                    button.ClearValue(PresentationUpdateHandlerPropertyKey);
                }

                if (e.NewValue is string newId && !string.IsNullOrWhiteSpace(newId)) {
                    button.Click += OnControlClickForInvokeAction;
                    SetupHandler(button, newId);
                }
            }
            else if (d is MenuItem menuItem) {
                menuItem.Click -= OnControlClickForInvokeAction;
                if (GetPresentationUpdateHandler(menuItem) is UpdateHandler oldHandler) {
                    ActionManager.Instance.RemovePresentationUpdateHandler(oldHandler.ActionId, oldHandler.Handler);
                    menuItem.ClearValue(PresentationUpdateHandlerPropertyKey);
                }

                if (e.NewValue is string newId && !string.IsNullOrWhiteSpace(newId)) {
                    menuItem.Click += OnControlClickForInvokeAction;
                    SetupHandler(menuItem, newId);
                }
            }
        }

        private static void SetupHandler(UIElement element, string actionId) {
            UpdateHandler newHandler = new UpdateHandler(actionId, (id, action, args, p) => {
                bool visible = p.IsVisible;
                if (visible) {
                    element.IsEnabled = p.IsEnabled;
                }
                else if (GetCanControlBecomeCollapsed(element)) {
                    element.Visibility = Visibility.Collapsed;
                }
                else {
                    element.IsEnabled = false;
                    visible = true;
                }

                // this may cause an infinite loop if modifying the IsChecked property causes an action to be executed
                // if (visible && action is ToggleAction toggle) {
                //     bool? isToggled = toggle.GetIsToggled(args);
                //     if (isToggled.HasValue) {
                //         if (element is MenuItem mi) {
                //             mi.SetCurrentValue(MenuItem.IsCheckedProperty, isToggled.Value);
                //         }
                //         else if (element is ToggleButton tb) {
                //             tb.IsChecked = isToggled;
                //         }
                //     }
                // }
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
                object dc = element.DataContext;
                if (dc != null) {
                    context.AddContext(dc);
                }

                context.AddContext(element);
                ItemsControl itemsControl = ItemsControl.ItemsControlFromItemContainer(element);
                if (itemsControl != null && itemsControl.IsItemItsOwnContainer(element)) {
                    context.AddContext(itemsControl);
                }

                if (Window.GetWindow(element) is Window win) {
                    object winDc = win.DataContext;
                    if (winDc != null)
                        context.AddContext(winDc);
                    context.AddContext(win);
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

            public GlobalPresentationUpdateHandler Handler { get; }

            public UpdateHandler(string actionId, GlobalPresentationUpdateHandler handler) {
                this.ActionId = actionId;
                this.Handler = handler;
            }
        }
    }
}