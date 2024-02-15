// #define PRINT_DEBUG_KEYSTROKES

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.WPF.Bindings;

namespace FramePFX.Shortcuts.WPF {
    public class WPFShortcutInputManager : ShortcutInputManager {
        private readonly Dictionary<string, List<InputStateBinding>> inputBindingStateMap;
        internal bool isProcessingKey;
        internal bool isProcessingMouse;

        public new WPFShortcutManager Manager => (WPFShortcutManager) base.Manager;

        /// <summary>
        /// The dependency object that was focused at the time of the input stroke
        /// </summary>
        public DependencyObject CurrentSource { get; private set; }

        private DataContext lazyCurrentDataContext;

        public WPFShortcutInputManager(WPFShortcutManager manager) : base(manager) {
            this.inputBindingStateMap = new Dictionary<string, List<InputStateBinding>>();
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

        private void ClearCurrentContext() {
            this.lazyCurrentDataContext = null;
            this.CurrentSource = null;
        }

        public void OnInputSourceMouseDown(Window root, DependencyObject focused, MouseButtonEventArgs e) {
            try {
                this.isProcessingMouse = true;
                this.SetupContext(focused);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, false, e.ClickCount);
                if (this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.ClearCurrentContext();
            }
        }

        public void OnInputSourceMouseUp(Window root, DependencyObject focused, MouseButtonEventArgs e) {
            try {
                this.isProcessingMouse = true;
                this.SetupContext(focused);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, false, e.ClickCount);
                if (this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.ClearCurrentContext();
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
                this.SetupContext(focused);
                MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
                if (this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.ClearCurrentContext();
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
                this.SetupContext(focused);
                ModifierKeys mods = ShortcutUtils.IsModifierKey(key) ? ModifierKeys.None : e.KeyboardDevice.Modifiers;
                KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);
                if (processor.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, e.IsRepeat)) {
                    e.Handled = true;
                }
            }
            finally {
                this.ClearCurrentContext();
                this.isProcessingKey = false;
            }
        }

        public void SetupContext(DependencyObject obj) {
            this.CurrentSource = obj;
        }

        public override IDataContext GetCurrentDataContext() {
            if (this.lazyCurrentDataContext == null) {
                if (this.CurrentSource == null)
                    return null;
                this.lazyCurrentDataContext = DataManager.GetDataContext(this.CurrentSource);
            }

            return this.lazyCurrentDataContext;
        }

        protected override void ProcessInputStates() {
            try {
                if (this.CurrentSource != null) {
                    InputStateCollection.GetInputStateBindingHierarchy(this.CurrentSource, this.inputBindingStateMap);
                }

                base.ProcessInputStates();
            }
            finally {
                this.inputBindingStateMap.Clear();
            }
        }

        protected internal override void OnInputStateActivated(GroupedInputState state) {
            base.OnInputStateActivated(state);
            this.ProcessTriggerState(state.FullPath, true);
        }

        protected internal override void OnInputStateDeactivated(GroupedInputState state) {
            base.OnInputStateDeactivated(state);
            this.ProcessTriggerState(state.FullPath, false);
        }

        private Task ProcessTriggerState(string path, bool isActive) {
            if (this.inputBindingStateMap.TryGetValue(path, out List<InputStateBinding> list)) {
                foreach (InputStateBinding binding in list) {
                    binding.IsActive = isActive;
                }
            }

            return Task.CompletedTask;
        }
    }
}