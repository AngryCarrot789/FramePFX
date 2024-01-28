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
            if (!(focused is TextBoxBase)) {
                return true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.None) {
                return UIInputManager.GetCanProcessTextBoxKeyStroke(focused);
            }
            else {
                return UIInputManager.GetCanProcessTextBoxKeyStrokeWithModifiers(focused);
            }
        }

        // Using async void here could possibly be dangerous if the awaited processor method (e.g. OnMouseStroke) halts
        // for a while due to a dialog for example. However... the methods should only really be callable when the window
        // is actually focused. But if the "root" event source is not a window then it could possibly be a problem
        // IsProcessingMouse and IsProcessingKey should prevent this issue

        public async void OnInputSourceMouseDown(Window root, DependencyObject focused, MouseButtonEventArgs e) {
            try {
                this.isProcessingMouse = true;
                this.SetupContext(root, focused);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, false, e.ClickCount);
                if (await this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.CurrentDataContext = null;
                this.CurrentSource = null;
            }
        }

        public async void OnInputSourceMouseUp(Window root, DependencyObject focused, MouseButtonEventArgs e) {
            try {
                this.isProcessingMouse = true;
                this.SetupContext(root, focused);
                MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, false, e.ClickCount);
                if (await this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.CurrentDataContext = null;
                this.CurrentSource = null;
            }
        }

        public async void OnInputSourceMouseWheel(Window sender, DependencyObject focused, MouseWheelEventArgs e) {
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
                this.SetupContext(sender, focused);
                MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, false, 0, e.Delta);
                if (await this.OnMouseStroke(UIInputManager.Instance.FocusedPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.isProcessingMouse = false;
                this.CurrentDataContext = null;
                this.CurrentSource = null;
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

        public async void OnInputSourceKeyEvent(Window root, WPFShortcutInputManager processor, DependencyObject focused, KeyEventArgs e, Key key, bool isRelease, bool isPreviewEvent) {
#if PRINT_DEBUG_KEYSTROKES
            if (!isPreviewEvent) {
                System.Diagnostics.Debug.WriteLine($"{(isRelease ? "UP  " : "DOWN")}: {e.Key}");
            }
#endif

            if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessKeyEvent(focused, e)) {
                return;
            }

            ModifierKeys mods = ShortcutUtils.IsModifierKey(key) ? ModifierKeys.None : e.KeyboardDevice.Modifiers;

            try {
                this.isProcessingKey = true;
                this.SetupContext(root, focused);
                KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);
                if (await processor.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, e.IsRepeat)) {
                    e.Handled = true;
                }
            }
            finally {
                this.CurrentDataContext = null;
                this.CurrentSource = null;
                this.isProcessingKey = false;
            }
        }

        public void SetupContext(Window root, DependencyObject obj) {
            this.CurrentDataContext = UIInputManager.GetDataContext(obj);
            this.CurrentSource = obj;
        }

        protected override async Task ProcessInputStates() {
            try {
                if (this.CurrentSource != null) {
                    InputStateCollection.GetInputStateBindingHierarchy(this.CurrentSource, this.inputBindingStateMap);
                }

                await base.ProcessInputStates();
            }
            finally {
                this.inputBindingStateMap.Clear();
            }
        }

        protected internal override async Task OnInputStateActivated(GroupedInputState state) {
            await base.OnInputStateActivated(state);
            await this.ProcessTriggerState(state.FullPath, true);
        }

        protected internal override async Task OnInputStateDeactivated(GroupedInputState state) {
            await base.OnInputStateDeactivated(state);
            await this.ProcessTriggerState(state.FullPath, false);
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