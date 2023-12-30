using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Actions;
using FramePFX.Actions.Contexts;
using FramePFX.Utils;
using FramePFX.WPF.Actions;

namespace FramePFX.WPF.AttachedProperties {
    /// <summary>
    /// A utility class for associating an executable action ID with a control, and handing the relative
    /// "activation" event (e.g. ClickEvent) in order to actually execute the action. This class is more convenient
    /// as a custom "ActionButton" class is not required
    /// </summary>
    public static class ActionControl {
        // Cache for faster runtime
        private static readonly RoutedEventHandler OnControlLoadedHandler;
        private static readonly RoutedEventHandler OnControlUnloadedHandler;
        private static readonly RoutedEventHandler OnControlClickForInvokeActionHandler;

        public static readonly DependencyProperty TargetActionIdProperty = DependencyProperty.RegisterAttached("TargetActionId", typeof(string), typeof(ActionControl), new PropertyMetadata(null, OnTargetActionIdPropertyChanged));
        private static readonly DependencyPropertyKey PresentationUpdateHandlerPropertyKey = DependencyProperty.RegisterAttachedReadOnly("PresentationUpdateHandler", typeof(UpdateHandler), typeof(ActionControl), new PropertyMetadata(default(CanExecuteChangedEventHandler)));

        static ActionControl() {
            OnControlLoadedHandler = OnControlLoaded;
            OnControlUnloadedHandler = OnControlUnloaded;
            OnControlClickForInvokeActionHandler = OnControlClickForInvokeAction;
        }

        public static void SetTargetActionId(DependencyObject element, string value) => element.SetValue(TargetActionIdProperty, value);
        public static string GetTargetActionId(DependencyObject element) => (string) element.GetValue(TargetActionIdProperty);

        private static void OnTargetActionIdPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Control control = (Control) d;
            UpdateHandler.ClearHandlerInfo(control);
            RoutedEvent clickHandler;
            switch (d) {
                case ButtonBase _: clickHandler = ButtonBase.ClickEvent; break;
                case MenuItem _: clickHandler = MenuItem.ClickEvent; break;
                default: {
                    // don't throw for unsetting/removing value because meh
                    if (e.NewValue != null)
                        throw new Exception($"Unsupported control type: {d.GetType().Name} ({d})");
                    return;
                }
            }

            if (e.NewValue is string newId && !string.IsNullOrWhiteSpace(newId)) {
                UpdateHandler.SetupHandler(control, clickHandler, newId);
            }
        }

        private static async void OnControlClickForInvokeAction(object sender, RoutedEventArgs e) {
            if (sender is ButtonBase || sender is MenuItem) {
                Control element = (Control) sender;
                string actionId = GetTargetActionId(element);
                if (string.IsNullOrWhiteSpace(actionId)) {
                    return;
                }

                DataContext context = ActionContextProviderCollection.CreateContextFromTarget(element);
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
            private readonly Control element;
            private readonly RoutedEvent clickEvent;
            private readonly string ActionId;
            private readonly CanExecuteChangedEventHandler Handler;

            private UpdateHandler(string actionId, Control element, RoutedEvent clickEvent) {
                this.ActionId = actionId;
                this.element = element;
                this.clickEvent = clickEvent;
                this.Handler = this.OnCanUpdateChanged;
            }

            private void OnCanUpdateChanged(string id, ContextAction action, ContextActionEventArgs args, bool canexecute) {
                DataContext context = ActionContextProviderCollection.CreateContextFromTarget(this.element);
                ContextActionEventArgs newArgs = args.Manager.CreateArgs(id, context, args.IsUserInitiated);
                this.element.IsEnabled = action.CanExecute(newArgs);
            }

            public static void ClearHandlerInfo(Control control) {
                if (control.GetValue(PresentationUpdateHandlerPropertyKey.DependencyProperty) is UpdateHandler handler) {
                    control.RemoveHandler(handler.clickEvent, OnControlClickForInvokeActionHandler);
                    control.Loaded -= OnControlLoadedHandler;
                    control.Unloaded -= OnControlUnloadedHandler;
                    control.SetValue(PresentationUpdateHandlerPropertyKey, null);
                    ActionManager.Instance.RemoveCanUpdateHandler(handler.ActionId, handler.Handler);
                }
            }

            public static void SetupHandler(Control control, RoutedEvent clickEvent, string actionId) {
                UpdateHandler newHandler = new UpdateHandler(actionId, control, clickEvent);
                control.SetValue(PresentationUpdateHandlerPropertyKey, newHandler);
                control.AddHandler(clickEvent, OnControlClickForInvokeActionHandler);
                if (control.IsLoaded) {
                    newHandler.OnLoaded();
                }
                else {
                    control.Loaded += OnControlLoadedHandler;
                    Debug.WriteLine($"UpdateHandler: Control was unloaded. Loaded event added");
                }
            }

            public void OnLoaded() {
                ActionManager.Instance.AddCanUpdateHandler(this.ActionId, this.Handler);
                this.element.Loaded -= OnControlLoadedHandler;
                this.element.Unloaded += OnControlUnloadedHandler;
                Debug.WriteLine($"UpdateHandler: OnLoaded. Control = {this.element}, ActionId = {this.ActionId}");
            }

            public void OnUnloaded() {
                this.element.Unloaded -= OnControlUnloadedHandler;
                this.element.Loaded += OnControlLoadedHandler;
                Debug.WriteLine($"UpdateHandler: OnUnloaded. Control = {this.element}, ActionId = {this.ActionId}");
                ActionManager.Instance.RemoveCanUpdateHandler(this.ActionId, this.Handler);
            }
        }

        private static void OnControlLoaded(object sender, RoutedEventArgs e) {
            if (GetUpdateHandler((DependencyObject) sender, out UpdateHandler handler)) {
                handler.OnLoaded();
            }
        }

        private static void OnControlUnloaded(object sender, RoutedEventArgs e) {
            if (GetUpdateHandler((DependencyObject) sender, out UpdateHandler handler)) {
                handler.OnUnloaded();
            }
        }

        private static bool GetUpdateHandler(DependencyObject obj, out UpdateHandler handler) {
            handler = (UpdateHandler) obj.GetValue(PresentationUpdateHandlerPropertyKey.DependencyProperty);
            if (handler != null)
                return true;
            Debug.WriteLine("Did not expect UpdateHandler to be null. An event handler for the CanUpdateHandler may have leaked");
            return false;
        }
    }
}