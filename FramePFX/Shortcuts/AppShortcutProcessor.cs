using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Actions;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Shortcuts.Usage;
using FramePFX.Core.Utils;

namespace FramePFX.Shortcuts {
    public class AppShortcutProcessor : ShortcutProcessor {
        public new AppShortcutManager Manager => (AppShortcutManager) base.Manager;

        public string CurrentInputBindingUsageID { get; set; } = AppShortcutManager.DEFAULT_USAGE_ID;

        public AppShortcutProcessor(ShortcutManager manager) : base(manager) {

        }

        private static AppShortcutProcessor GetWindowProcessor(object sender) {
            return sender is Window window ? UIFocusGroup.GetShortcutProcessor(window) : null;
        }

        public async void OnWindowMouseDown(object sender, MouseButtonEventArgs e) {
            if (e.OriginalSource is DependencyObject hit) {
                UIFocusGroup.ProcessFocusGroupChange(hit);

                try {
                    this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(hit) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupDataContext(hit);
                    MouseStroke stroke = new MouseStroke((int) e.ChangedButton, (int) Keyboard.Modifiers, e.ClickCount);
                    if (await this.OnMouseStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                        e.Handled = true;
                    }
                }
                finally {
                    this.CurrentDataContext = null;
                    this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnWindowMouseWheel(object sender, MouseWheelEventArgs e) {
            if (e.OriginalSource is DependencyObject hit) {
                int button;
                if (e.Delta < 0) {
                    button = AppShortcutManager.BUTTON_WHEEL_DOWN;
                }
                else if (e.Delta > 0) {
                    button = AppShortcutManager.BUTTON_WHEEL_UP;
                }
                else {
                    return;
                }

                try {
                    this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(hit) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                    this.SetupDataContext(hit);
                    MouseStroke stroke = new MouseStroke(button, (int) Keyboard.Modifiers, 0, e.Delta);
                    if (await this.OnMouseStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                        e.Handled = true;
                    }
                }
                finally {
                    this.CurrentDataContext = null;
                    this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
                }
            }
        }

        public async void OnKeyEvent(object sender, DependencyObject focused, KeyEventArgs e, bool isRelease) {
            if (e.Handled || e.IsRepeat) {
                return;
            }

            AppShortcutProcessor processor = GetWindowProcessor(sender);
            if (processor == null) {
                return;
            }

            Key key = e.Key == Key.System ? e.SystemKey : e.Key;
            if (ShortcutUtils.IsModifierKey(key) || key == Key.DeadCharProcessed) {
                return;
            }

            try {
                this.CurrentInputBindingUsageID = UIFocusGroup.GetUsageID(focused) ?? AppShortcutManager.DEFAULT_USAGE_ID;
                this.SetupDataContext(focused);
                KeyStroke stroke = new KeyStroke((int) key, (int) Keyboard.Modifiers, isRelease);
                if (await processor.OnKeyStroke(UIFocusGroup.FocusedGroupPath, stroke)) {
                    e.Handled = true;
                }
            }
            finally {
                this.CurrentDataContext = null;
                this.CurrentInputBindingUsageID = AppShortcutManager.DEFAULT_USAGE_ID;
            }
        }

        public void SetupDataContext(DependencyObject obj) {
            if (obj is FrameworkElement element) {
                this.CurrentDataContext = element.DataContext;
                if (this.CurrentDataContext is IHasDataContext iHas) {
                    this.CurrentDataContext = iHas.DataContext;
                }
            }
            else if (obj is IHasDataContext iHas) {
                this.CurrentDataContext = iHas.DataContext;
            }
        }

        public override async Task<bool> OnShortcutActivated(ManagedShortcut shortcut) {
            // ShortcutInputGesture input = ShortcutInputGesture.CurrentInputGesture;
            // if (input?.ShortcutKeyBinding != null && shortcut.Path == input.ShortcutKeyBinding.ShortcutID) {
            //     input.IsCompleted = true;
            // }

            bool finalResult = false;
            if (AppShortcutManager.InputBindingCallbackMap.TryGetValue(shortcut.Path, out Dictionary<string, List<ShortcutActivateHandler>> usageMap)) {
                if ((shortcut.IsGlobal || shortcut.Group.IsGlobal) && usageMap.TryGetValue(AppShortcutManager.DEFAULT_USAGE_ID, out List<ShortcutActivateHandler> callbacks2) && callbacks2.Count > 0) {
                    CoreIoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks2.Count} callbacks...");
                    foreach (ShortcutActivateHandler callback in callbacks2) {
                        finalResult |= await callback(this, shortcut);
                    }
                    CoreIoC.BroadcastShortcutActivity($"Activated global shortcut: {shortcut}. Calling {callbacks2.Count} callbacks... Complete!");
                }
                else if (usageMap.TryGetValue(this.CurrentInputBindingUsageID, out List<ShortcutActivateHandler> callbacks1) && callbacks1.Count > 0) {
                    CoreIoC.BroadcastShortcutActivity($"Activated shortcut: {shortcut}. Calling {callbacks1.Count} callbacks...");
                    foreach (ShortcutActivateHandler callback in callbacks1) {
                        finalResult |= await callback(this, shortcut);
                    }
                    CoreIoC.BroadcastShortcutActivity($"Activated shortcut: {shortcut}. Calling {callbacks1.Count} callbacks... Complete!");
                }
            }

            return finalResult | await base.OnShortcutActivated(shortcut);
        }

        public override bool OnNoSuchShortcutForKeyStroke(string @group, in KeyStroke stroke) {
            if (stroke.IsKeyDown) {
                CoreIoC.BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
            }

            return base.OnNoSuchShortcutForKeyStroke(@group, in stroke);
        }

        public override bool OnNoSuchShortcutForMouseStroke(string @group, in MouseStroke stroke) {
            CoreIoC.BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
            return base.OnNoSuchShortcutForMouseStroke(@group, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextKeyStroke(IShortcutUsage usage, ManagedShortcut shortcut, in KeyStroke stroke) {
            CoreIoC.BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextKeyStroke(usage, shortcut, in stroke);
        }

        public override bool OnCancelUsageForNoSuchNextMouseStroke(IShortcutUsage usage, ManagedShortcut shortcut, in MouseStroke stroke) {
            CoreIoC.BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
            return base.OnCancelUsageForNoSuchNextMouseStroke(usage, shortcut, in stroke);
        }

        public override bool OnShortcutUsagesCreated() {
            StringJoiner joiner = new StringJoiner(new StringBuilder(), ", ");
            foreach (KeyValuePair<IShortcutUsage, ManagedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            CoreIoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnShortcutUsagesCreated();
        }

        public override bool OnSecondShortcutUsagesProgressed() {
            StringJoiner joiner = new StringJoiner(new StringBuilder(), ", ");
            foreach (KeyValuePair<IShortcutUsage, ManagedShortcut> pair in this.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            CoreIoC.BroadcastShortcutActivity("Waiting for next input: " + joiner);
            return base.OnSecondShortcutUsagesProgressed();
        }
    }
}