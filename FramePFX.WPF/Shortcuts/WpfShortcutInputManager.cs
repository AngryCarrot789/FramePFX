// #define PRINT_DEBUG_KEYSTROKES

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Actions.Contexts;
using FramePFX.Commands;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.Usage;
using FramePFX.Utils;
using FramePFX.WPF.Shortcuts.Bindings;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Shortcuts {
    public class WpfShortcutInputManager : ShortcutInputManager {
        private readonly Dictionary<string, List<InputStateBinding>> inputBindingStateMap;
        private bool isProcessingKey;
        private bool isProcessingMouse;

        public new WPFShortcutManager Manager => (WPFShortcutManager) base.Manager;

        /// <summary>
        /// The dependency object source that was focused during the input stroke currently being processed
        /// </summary>
        public DependencyObject CurrentSource { get; private set; }

        public WpfShortcutInputManager(WPFShortcutManager manager) : base(manager) {
            this.inputBindingStateMap = new Dictionary<string, List<InputStateBinding>>();
        }

        public static bool CanProcessEventType(DependencyObject obj, bool isPreviewEvent) {
            return UIInputManager.GetUsePreviewEvents(obj) == isPreviewEvent;
        }

        public static bool CanProcessKeyEvent(DependencyObject focused, KeyEventArgs e) {
            if (!(focused is TextBoxBase)) {
                return true;
            }
            else if (e.KeyboardDevice.Modifiers == 0) {
                return UIInputManager.GetCanProcessTextBoxKeyStroke(focused);
            }
            else {
                return UIInputManager.GetCanProcessTextBoxKeyStrokeWithModifiers(focused);
            }
        }

        public static bool CanProcessMouseEvent(DependencyObject focused, MouseEventArgs e) {
            return !(focused is TextBoxBase) || UIInputManager.GetCanProcessTextBoxMouseStroke(focused);
        }

        // Using async void here could possibly be dangerous if the awaited processor method (e.g. OnMouseStroke) halts
        // for a while due to a dialog for example. However... the methods should only really be callable when the window
        // is actually focused. But if the "root" event source is not a window then it could possibly be a problem
        // IsProcessingMouse and IsProcessingKey should prevent this issue

        public async void OnWindowMouseDown(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (!this.isProcessingMouse && e.OriginalSource is DependencyObject focused) {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e)) {
                    return;
                }

                try {
                    this.isProcessingMouse = true;
                    this.SetupContext(sender, focused);
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
        }

        public async void OnWindowMouseUp(object sender, MouseButtonEventArgs e, bool isPreviewEvent) {
            if (!this.isProcessingMouse && e.OriginalSource is DependencyObject focused) {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e)) {
                    return;
                }

                try {
                    this.isProcessingMouse = true;
                    this.CurrentSource = focused; // no need to generate the data context as it isn't used
                    MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, true, e.ClickCount);
                    await this.ProcessInputStatesForMouseUp(UIInputManager.Instance.FocusedPath, stroke);
                }
                finally {
                    this.isProcessingMouse = false;
                    this.CurrentSource = null;
                }
            }
        }

        public async void OnWindowMouseWheel(object sender, MouseWheelEventArgs e, bool isPreviewEvent) {
            if (!this.isProcessingMouse && e.OriginalSource is DependencyObject focused) {
                if (!CanProcessEventType(focused, isPreviewEvent) || !CanProcessMouseEvent(focused, e)) {
                    return;
                }

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

        public async void OnKeyEvent(object sender, DependencyObject focused, KeyEventArgs e, bool isRelease, bool isPreviewEvent) {
            if (this.isProcessingKey || e.Handled) {
                return;
            }

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (key == Key.DeadCharProcessed || key == Key.None) {
                return;
            }

            WpfShortcutInputManager inputManager = WPFShortcutManager.GetShortcutProcessorForUIObject(sender);
            if (inputManager == null) {
                return;
            }

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
                this.SetupContext(sender, focused);
                KeyStroke stroke = new KeyStroke((int) key, (int) mods, isRelease);
                if (await inputManager.OnKeyStroke(UIInputManager.Instance.FocusedPath, stroke, e.IsRepeat)) {
                    e.Handled = true;
                }
            }
            finally {
                this.CurrentDataContext = null;
                this.CurrentSource = null;
                this.isProcessingKey = false;
            }
        }

        public void SetupContext(object sender, DependencyObject obj) {
            DataContext context = new DataContext();
            if (VisualTreeUtils.GetDataContext(obj, out object dc)) {
                context.AddContext(dc);
            }

            if (ItemsControl.ItemsControlFromItemContainer(obj) is ItemsControl rc && (dc = rc.DataContext) != null) {
                context.AddContext(dc);
            }

            if (sender is DependencyObject && VisualTreeUtils.GetDataContext((DependencyObject) sender, out dc)) {
                context.AddContext(dc);
            }

            this.CurrentDataContext = context;
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

        protected override async Task OnInputStateActivated(GroupedInputState state) {
            await base.OnInputStateActivated(state);
            await this.ProcessTriggerState(state.FullPath, true);
        }

        protected override async Task OnInputStateDeactivated(GroupedInputState state) {
            await base.OnInputStateDeactivated(state);
            await this.ProcessTriggerState(state.FullPath, false);
        }

        private async Task ProcessTriggerState(string path, bool isActive) {
            if (this.inputBindingStateMap.TryGetValue(path, out List<InputStateBinding> list)) {
                for (int i = 0; i < list.Count; i++) {
                    InputStateBinding binding = list[i];
                    binding.IsActive = isActive;
                    ICommand cmd = binding.Command;
                    if (cmd == null) {
                        continue;
                    }

                    object param = isActive.Box();
                    if (cmd is BaseAsyncRelayCommand asyncCommand) {
                        await asyncCommand.TryExecuteAsync(param);
                    }
                    else if (cmd.CanExecute(param)) {
                        cmd.Execute(param);
                    }
                }
            }
        }

        protected override bool OnNoSuchShortcutForKeyStroke(string @group, in KeyStroke stroke) {
            if (stroke.IsKeyDown) {
                IoC.BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
            }

            return base.OnNoSuchShortcutForKeyStroke(@group, in stroke);
        }

        protected override bool OnNoSuchShortcutForMouseStroke(string @group, in MouseStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
            return base.OnNoSuchShortcutForMouseStroke(@group, in stroke);
        }

        protected override bool OnCancelUsageForNoSuchNextKeyStroke(IShortcutUsage usage, GroupedShortcut shortcut, in KeyStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextKeyStroke(usage, shortcut, in stroke);
        }

        protected override bool OnCancelUsageForNoSuchNextMouseStroke(IShortcutUsage usage, GroupedShortcut shortcut, in MouseStroke stroke) {
            IoC.BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextMouseStroke(usage, shortcut, in stroke);
        }

        protected override bool OnShortcutUsagesCreated() {
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnShortcutUsagesCreated();
        }

        protected override bool OnSecondShortcutUsagesProgressed() {
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            IoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnSecondShortcutUsagesProgressed();
        }
    }
}