using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MCNBTViewer.Core.Shortcuts.Inputs;
using MCNBTViewer.Core.Shortcuts.Managing;
using MCNBTViewer.Core.Utils;

namespace MCNBTViewer.Shortcuts {
    public class AppShortcutManager : ShortcutManager {
        public const int BUTTON_WHEEL_UP = 143;   // Away from the user
        public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
        public const string DEFAULT_USAGE_ID = "DEF";

        public static AppShortcutManager Instance { get; } = new AppShortcutManager();

        /// <summary>
        /// Maps an action ID to a dictionary which maps a custom usage ID to the callback functions
        /// </summary>
        public static Dictionary<string, Dictionary<string, List<ShortcutActivateHandler>>> InputBindingCallbackMap { get; }

        static AppShortcutManager() {
            InputBindingCallbackMap = new Dictionary<string, Dictionary<string, List<ShortcutActivateHandler>>>();
            KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
            KeyStroke.ModifierToStringProvider = (x) => {
                StringJoiner joiner = new StringJoiner(new StringBuilder(), " + ");
                ModifierKeys keys = (ModifierKeys) x;
                if ((keys & ModifierKeys.Control) != 0) joiner.Append("Ctrl");
                if ((keys & ModifierKeys.Alt) != 0)     joiner.Append("Alt");
                if ((keys & ModifierKeys.Shift) != 0)   joiner.Append("Shift");
                if ((keys & ModifierKeys.Windows) != 0) joiner.Append("Win");
                return joiner.ToString();
            };

            MouseStroke.MouseButtonToStringProvider = (x) => {
                switch (x) {
                    case 0: return "LMB";
                    case 1: return "MWB";
                    case 2: return "RMB";
                    case 3: return "X1";
                    case 4: return "X2";
                    case BUTTON_WHEEL_UP:   return "WHEEL_UP";
                    case BUTTON_WHEEL_DOWN: return "WHEEL_DOWN";
                    default: return $"UNKNOWN_MB[{x}]";
                }
            };

            MouseStroke.ModifierToStringProvider = KeyStroke.ModifierToStringProvider;
        }

        public AppShortcutManager() {

        }

        public static void UnregisterHandler(string shortcutId, string usageId) {
            ShortcutUtils.EnforceIdFormat(shortcutId, nameof(shortcutId));
            ShortcutUtils.EnforceIdFormat(usageId, nameof(usageId));
            if (InputBindingCallbackMap.TryGetValue(shortcutId, out Dictionary<string, List<ShortcutActivateHandler>> usageMap)) {
                usageMap.Remove(usageId);
            }
        }

        public static void RegisterHandler(string shortcutId, string usageId, ShortcutActivateHandler handler) {
            ShortcutUtils.EnforceIdFormat(shortcutId, nameof(shortcutId));
            ShortcutUtils.EnforceIdFormat(usageId, nameof(usageId));
            if (!InputBindingCallbackMap.TryGetValue(shortcutId, out Dictionary<string, List<ShortcutActivateHandler>> usageMap)) {
                InputBindingCallbackMap[shortcutId] = usageMap = new Dictionary<string, List<ShortcutActivateHandler>>();
            }

            if (!usageMap.TryGetValue(usageId, out List<ShortcutActivateHandler> list)) {
                usageMap[usageId] = list = new List<ShortcutActivateHandler>();
            }

            list.Add(handler);
        }

        // Typically applied only to windows
        public static void OnIsGlobalShortcutFocusTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is UIElement element) {
                element.PreviewMouseDown -= OnWindowPreviewMouseDown;
                element.MouseDown -= OnWindowMouseDown;
                element.MouseUp -= OnWindowMouseUp;
                element.KeyDown -= OnWindowKeyDown;
                element.KeyUp -= OnWindowKeyUp;
                element.MouseWheel -= OnWindowMouseWheel;
                if (e.NewValue != e.OldValue && (bool) e.NewValue) {
                    element.PreviewMouseDown += OnWindowPreviewMouseDown;
                    element.MouseDown += OnWindowMouseDown;
                    element.MouseUp += OnWindowMouseUp;
                    element.KeyDown += OnWindowKeyDown;
                    element.KeyUp += OnWindowKeyUp;
                    element.MouseWheel += OnWindowMouseWheel;
                    element.SetValue(UIFocusGroup.ShortcutProcessorProperty, new AppShortcutProcessor(Instance));
                }
                else {
                    element.ClearValue(UIFocusGroup.ShortcutProcessorProperty);
                }
            }
            else {
                throw new Exception("This property must be applied to type UIElement only, not " + (d?.GetType()));
            }
        }

        private static void OnWindowPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource is DependencyObject hit) {
                UIFocusGroup.ProcessFocusGroupChange(hit);
            }
        }

        private static async void OnWindowMouseDown(object sender, MouseButtonEventArgs e) {
            AppShortcutProcessor processor = GetWindowProcessor(sender);
            processor?.OnWindowMouseDown(sender, e);
        }

        private static AppShortcutProcessor GetWindowProcessor(object sender) {
            return sender is Window window ? UIFocusGroup.GetShortcutProcessor(window) : null;
        }

        private static void OnWindowMouseUp(object sender, MouseButtonEventArgs e) {
            // if (e.OriginalSource is DependencyObject hit) {
            //     string focusedPath = UIFocusGroup.ProcessFocusGroupChange(hit);
            //     MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, e.ClickCount);
            //     if (Instance.OnMouseStroke(focusedPath, stroke)) {
            //         e.Handled = true;
            //     }
            // }
        }

        private static async void OnWindowMouseWheel(object sender, MouseWheelEventArgs e) {
            AppShortcutProcessor processor = GetWindowProcessor(sender);
            processor?.OnWindowMouseWheel(sender, e);
        }

        private static void OnWindowKeyUp(object sender, KeyEventArgs e) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, true);
        }

        private static void OnWindowKeyDown(object sender, KeyEventArgs e) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, false);
        }

        public static async void OnKeyEvent(object window, DependencyObject focused, KeyEventArgs e, bool isRelease) {
            AppShortcutProcessor processor = GetWindowProcessor(window);
            processor?.OnKeyEvent(window, focused, e, isRelease);
        }

        public override ShortcutProcessor NewProcessor() {
            return new AppShortcutProcessor(this);
        }
    }
}