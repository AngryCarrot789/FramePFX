using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using SharpPadV2.Core.Shortcuts.Inputs;
using SharpPadV2.Core.Shortcuts.Managing;
using SharpPadV2.Core.Utils;

namespace SharpPadV2.Shortcuts {
    public class WPFShortcutManager : ShortcutManager {
        public const int BUTTON_WHEEL_UP = 143;   // Away from the user
        public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
        public const string DEFAULT_USAGE_ID = "DEF";

        public static WPFShortcutManager Instance { get; } = new WPFShortcutManager();

        /// <summary>
        /// Maps an action ID to a dictionary which maps a custom usage ID to the callback functions
        /// </summary>
        public static Dictionary<string, Dictionary<string, List<ActivationHandlerReference>>> InputBindingCallbackMap { get; }

        static WPFShortcutManager() {
            InputBindingCallbackMap = new Dictionary<string, Dictionary<string, List<ActivationHandlerReference>>>();
            KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
            KeyStroke.ModifierToStringProvider = (x, s) => {
                StringJoiner joiner = new StringJoiner(new StringBuilder(), s ? " + " : "+");
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

        public WPFShortcutManager() {

        }

        public static void UnregisterHandler(string shortcutId, string usageId) {
            ShortcutUtils.EnforceIdFormat(shortcutId, nameof(shortcutId));
            ShortcutUtils.EnforceIdFormat(usageId, nameof(usageId));
            if (InputBindingCallbackMap.TryGetValue(shortcutId, out Dictionary<string, List<ActivationHandlerReference>> usageMap)) {
                usageMap.Remove(usageId);
            }
        }

        public static void RegisterHandler(string shortcutId, string usageId, ShortcutActivateHandler handler, bool weak = true) {
            ShortcutUtils.EnforceIdFormat(shortcutId, nameof(shortcutId));
            ShortcutUtils.EnforceIdFormat(usageId, nameof(usageId));
            if (!InputBindingCallbackMap.TryGetValue(shortcutId, out Dictionary<string, List<ActivationHandlerReference>> usageMap)) {
                InputBindingCallbackMap[shortcutId] = usageMap = new Dictionary<string, List<ActivationHandlerReference>>();
            }

            if (!usageMap.TryGetValue(usageId, out List<ActivationHandlerReference> list)) {
                usageMap[usageId] = list = new List<ActivationHandlerReference>();
            }

            list.Add(new ActivationHandlerReference(handler, weak));
        }

        private static WPFShortcutProcessor GetShortcutProcessorForUIObject(object sender) {
            return sender is Window window ? UIFocusGroup.GetShortcutProcessor(window) : null;
        }

        private static readonly MouseButtonEventHandler RootMouseDownHandlerPreview = (s, args) => HandleRootMouseDown(s, args, true);
        private static readonly MouseButtonEventHandler RootMouseDownHandlerNonPreview = (s, args) => HandleRootMouseDown(s, args, false);
        private static readonly MouseButtonEventHandler RootMouseUpHandlerPreview = (s, args) => HandleRootMouseUp(s, args, true);
        private static readonly MouseButtonEventHandler RootMouseUpHandlerNonPreview = (s, args) => HandleRootMouseUp(s, args, false);
        private static readonly KeyEventHandler RootKeyDownHandlerPreview = (s, args) => HandleRootKeyDown(s, args, true);
        private static readonly KeyEventHandler RootKeyDownHandlerNonPreview = (s, args) => HandleRootKeyDown(s, args, false);
        private static readonly KeyEventHandler RootKeyUpHandlerPreview = (s, args) => HandleRootKeyUp(s, args, true);
        private static readonly KeyEventHandler RootKeyUpHandlerNonPreview = (s, args) => HandleRootKeyUp(s, args, false);
        private static readonly MouseWheelEventHandler RootWheelHandlerPreview = (s, args) => HandleRootMouseWheel(s, args, true);
        private static readonly MouseWheelEventHandler RootWheelHandlerNonPreview = (s, args) => HandleRootMouseWheel(s, args, false);

        // Typically applied only to windows
        public static void OnIsGlobalShortcutFocusTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is UIElement element) {
                element.MouseDown -= RootMouseDownHandlerNonPreview;
                element.PreviewMouseDown -= RootMouseDownHandlerPreview;
                element.MouseUp -= RootMouseUpHandlerNonPreview;
                element.PreviewMouseUp -= RootMouseUpHandlerPreview;
                element.KeyDown -= RootKeyDownHandlerNonPreview;
                element.PreviewKeyDown -= RootKeyDownHandlerPreview;
                element.KeyUp -= RootKeyUpHandlerNonPreview;
                element.PreviewKeyUp -= RootKeyUpHandlerPreview;
                element.MouseWheel -= RootWheelHandlerNonPreview;
                element.PreviewMouseWheel -= RootWheelHandlerPreview;
                if (e.NewValue != e.OldValue && (bool) e.NewValue) {
                    element.MouseDown += RootMouseDownHandlerNonPreview;
                    element.PreviewMouseDown += RootMouseDownHandlerPreview;
                    element.MouseUp += RootMouseUpHandlerNonPreview;
                    element.PreviewMouseUp += RootMouseUpHandlerPreview;
                    element.KeyDown += RootKeyDownHandlerNonPreview;
                    element.PreviewKeyDown += RootKeyDownHandlerPreview;
                    element.KeyUp += RootKeyUpHandlerNonPreview;
                    element.PreviewKeyUp += RootKeyUpHandlerPreview;
                    element.MouseWheel += RootWheelHandlerNonPreview;
                    element.PreviewMouseWheel += RootWheelHandlerPreview;
                    element.SetValue(UIFocusGroup.ShortcutProcessorProperty, new WPFShortcutProcessor(Instance));
                }
                else {
                    element.ClearValue(UIFocusGroup.ShortcutProcessorProperty);
                }
            }
            else {
                throw new Exception("This property must be applied to type UIElement only, not " + (d?.GetType()));
            }
        }

        private static void HandleRootMouseDown(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (isPreviewEvent && e.OriginalSource is DependencyObject hit) {
                UIFocusGroup.ProcessFocusGroupChange(hit);
            }

            WPFShortcutProcessor processor = GetShortcutProcessorForUIObject(sender);
            processor?.OnWindowMouseDown(sender, e, isPreviewEvent);
        }

        private static void HandleRootMouseUp(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            // if (e.OriginalSource is DependencyObject hit) {
            //     string focusedPath = UIFocusGroup.ProcessFocusGroupChange(hit);
            //     MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, e.ClickCount);
            //     if (Instance.OnMouseStroke(focusedPath, stroke)) {
            //         e.Handled = true;
            //     }
            // }
        }

        private static void HandleRootMouseWheel(object sender, MouseWheelEventArgs e, bool isPreviewEvent) {
            WPFShortcutProcessor processor = GetShortcutProcessorForUIObject(sender);
            processor?.OnWindowMouseWheel(sender, e, isPreviewEvent);
        }

        private static void HandleRootKeyDown(object sender, KeyEventArgs e, bool isPreviewEvent) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, false, isPreviewEvent);
        }

        private static void HandleRootKeyUp(object sender, KeyEventArgs e, bool isPreviewEvent) {
            OnKeyEvent(sender, e.OriginalSource as DependencyObject, e, true, isPreviewEvent);
        }

        public static void OnKeyEvent(object window, DependencyObject focused, KeyEventArgs e, bool isRelease, bool isPreviewEvent) {
            WPFShortcutProcessor processor = GetShortcutProcessorForUIObject(window);
            processor?.OnKeyEvent(window, focused, e, isRelease, isPreviewEvent);
        }

        public override ShortcutProcessor NewProcessor() {
            return new WPFShortcutProcessor(this);
        }

        public void SetRoot(ShortcutGroup @group) {
            this.Root = @group;
        }
    }
}