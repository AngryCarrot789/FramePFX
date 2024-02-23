// #define PRINT_DEBUG_KEYSTROKES

using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.WPF {
    public class WPFShortcutInputManager : ShortcutInputManager {
        internal bool isProcessingKey;
        internal bool isProcessingMouse;

        public new WPFShortcutManager Manager => (WPFShortcutManager) base.Manager;

        /// <summary>
        /// The dependency object that was focused at the time of the input stroke
        /// </summary>
        public DependencyObject CurrentSource { get; private set; }

        private IContextData lazyCurrentContextData;

        public WPFShortcutInputManager(WPFShortcutManager manager) : base(manager) {
        }

        public static bool CanProcessEventType(DependencyObject obj, bool isPreviewEvent) {
            return UIInputManager.GetUsePreviewEvents(obj) == isPreviewEvent;
        }

        public static bool CanProcessMouseEvent(DependencyObject focused, MouseEventArgs e) {
            return !(focused is TextBoxBase) || UIInputManager.GetCanProcessTextBoxMouseStroke(focused);
        }

        public static bool CanProcessKeyEvent(DependencyObject focused, KeyEventArgs e) {
            if (focused is TextBoxBase) {
                if (e.KeyboardDevice.Modifiers == ModifierKeys.None) {
                    return UIInputManager.GetCanProcessTextBoxKeyStroke(focused);
                }
                else {
                    return UIInputManager.GetCanProcessTextBoxKeyStrokeWithModifiers(focused);
                }
            }
            else {
                return true;
            }
        }

        public void BeginInputProcessing(DependencyObject obj) {
            this.CurrentSource = obj;
        }

        private void EndInputProcessing() {
            this.lazyCurrentContextData = null;
            this.CurrentSource = null;
        }

        public void OnInputSourceMouseButton(Window root, DependencyObject focused, MouseButtonEventArgs e, bool isRelease) {
            try {
                this.isProcessingMouse = true;
                this.BeginInputProcessing(focused);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, isRelease, e.ClickCount);
                if (this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.EndInputProcessing();
            }
        }

        public void OnInputSourceMouseWheel(Window sender, DependencyObject focused, MouseWheelEventArgs e) {
            int button;
            if (e.Delta < 0) {
                button = WPFShortcutManager.BUTTON_WHEEL_DOWN;
            }
            else if (e.Delta > 0) {
                button = WPFShortcutManager.BUTTON_WHEEL_UP;
            }
            else {
                return;
            }

            try {
                this.isProcessingMouse = true;
                this.BeginInputProcessing(focused);
                MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
                if (this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.EndInputProcessing();
            }
        }

        public static bool GetKeyCombo(KeyEventArgs e, out Key key, out ModifierKeys mods) {
            Key input = e.Key == Key.System ? e.SystemKey : e.Key;
            if (input != Key.DeadCharProcessed && input != Key.None) {
                mods = ShortcutUtils.IsModifierKey(input) ? ModifierKeys.None : e.KeyboardDevice.Modifiers;
                key = input;
                return true;
            }

            key = default;
            mods = default;
            return false;
        }

        public void OnInputSourceKeyEvent(Window root, WPFShortcutInputManager processor, DependencyObject focused, KeyEventArgs e, Key key, bool isRelease, bool isPreviewEvent) {
            if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessKeyEvent(focused, e)) {
                return;
            }

            try {
                this.isProcessingKey = true;
                this.BeginInputProcessing(focused);
                ModifierKeys mods = ShortcutUtils.IsModifierKey(key) ? ModifierKeys.None : e.KeyboardDevice.Modifiers;
                KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);
                if (processor.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, e.IsRepeat)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingKey = false;
                this.EndInputProcessing();
            }
        }

        public override IContextData GetCurrentContext() {
            if (this.lazyCurrentContextData == null) {
                if (this.CurrentSource == null)
                    return null;
                this.lazyCurrentContextData = DataManager.GetFullContextData(this.CurrentSource);
            }

            return this.lazyCurrentContextData;
        }
    }
}