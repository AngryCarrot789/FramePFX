using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FramePFX.Commands;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Keymapping;
using FramePFX.Shortcuts.Managing;
using FramePFX.Utils;
using FramePFX.WPF.Shortcuts.Bindings;

namespace FramePFX.WPF.Shortcuts {
    public class WPFShortcutManager : ShortcutManager {
        public const int BUTTON_WHEEL_UP = 143; // Away from the user
        public const int BUTTON_WHEEL_DOWN = 142; // Towards the user
        public const string DEFAULT_USAGE_ID = "DEF";

        public static WPFShortcutManager WPFInstance => (WPFShortcutManager) Instance ?? throw new Exception("No shortcut manager available");

        static WPFShortcutManager() {
            KeyStroke.KeyCodeToStringProvider = (x) => ((Key) x).ToString();
            KeyStroke.ModifierToStringProvider = (x, s) => {
                StringJoiner joiner = new StringJoiner(s ? " + " : "+");
                ModifierKeys keys = (ModifierKeys) x;
                if ((keys & ModifierKeys.Control) != 0)
                    joiner.Append("Ctrl");
                if ((keys & ModifierKeys.Alt) != 0)
                    joiner.Append("Alt");
                if ((keys & ModifierKeys.Shift) != 0)
                    joiner.Append("Shift");
                if ((keys & ModifierKeys.Windows) != 0)
                    joiner.Append("Win");
                return joiner.ToString();
            };

            MouseStroke.MouseButtonToStringProvider = (x) => {
                switch (x) {
                    case 0: return "LMB";
                    case 1: return "MMB";
                    case 2: return "RMB";
                    case 3: return "X1";
                    case 4: return "X2";
                    case BUTTON_WHEEL_UP: return "WHEEL_UP";
                    case BUTTON_WHEEL_DOWN: return "WHEEL_DOWN";
                    default: return $"Unknown Button ({x})";
                }
            };
        }

        public WPFShortcutManager() {
        }

        public override ShortcutInputManager NewProcessor() => new WPFShortcutInputManager(this);

        public void DeserialiseRoot(Stream stream) {
            this.InvalidateShortcutCache();
            Keymap map = WPFKeyMapSerialiser.Instance.Deserialise(this, stream);
            this.Root = map.Root; // invalidates cache automatically
            try {
                this.EnsureCacheBuilt(); // do keymap check; crash on errors (e.g. duplicate shortcut path)
            }
            catch (Exception e) {
                this.InvalidateShortcutCache();
                this.Root = ShortcutGroup.CreateRoot(this);
                throw new Exception("Failed to process keymap and built caches", e);
            }
        }

        protected override async Task<bool> OnShortcutActivatedInternal(ShortcutInputManager inputManager, GroupedShortcut shortcut) {
            bool result = false;
            List<ShortcutCommandBinding> bindings;
            DependencyObject src = ((WPFShortcutInputManager) inputManager).CurrentSource;
            if (src != null && (bindings = ShortcutCommandCollection.GetCommandBindingHierarchy(src)).Count > 0) {
                foreach (ShortcutCommandBinding binding in bindings) {
                    if (!shortcut.FullPath.Equals(binding.ShortcutPath)) {
                        continue;
                    }

                    ICommand cmd;
                    if (result && !binding.AllowChainExecution || (cmd = binding.Command) == null) {
                        continue;
                    }

                    object param;
                    if (cmd is BaseAsyncRelayCommand asyncCommand) {
                        IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.InProgress", shortcut));
                        if (await asyncCommand.TryExecuteAsync(binding.CommandParameter)) {
                            IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate.Completed", shortcut));
                            result = true;
                        }
                    }
                    else if (cmd.CanExecute(param = binding.CommandParameter)) {
                        IoC.BroadcastShortcutActivity(IoC.Translator.GetString("S.Shortcuts.Activate", shortcut));
                        cmd.Execute(param);
                        result = true;
                    }
                }
            }

            return result || await base.OnShortcutActivatedInternal(inputManager, shortcut);
        }
    }
}