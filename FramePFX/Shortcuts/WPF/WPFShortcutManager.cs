using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using FramePFX.Editors.Views;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Keymapping;
using FramePFX.Shortcuts.Managing;
using FramePFX.Shortcuts.Usage;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.WPF {
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

        protected internal override void OnSecondShortcutUsagesProgressed(ShortcutInputManager inputManager) {
            base.OnSecondShortcutUsagesProgressed(inputManager);
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in inputManager.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            BroadcastShortcutActivity("Waiting for next input: " + joiner);
        }

        protected internal override void OnShortcutUsagesCreated(ShortcutInputManager inputManager) {
            base.OnShortcutUsagesCreated(inputManager);
            StringJoiner joiner = new StringJoiner(", ");
            foreach (KeyValuePair<IShortcutUsage, GroupedShortcut> pair in inputManager.ActiveUsages) {
                joiner.Append(pair.Key.CurrentStroke.ToString());
            }

            BroadcastShortcutActivity("Waiting for next input: " + joiner);
        }

        protected internal override void OnCancelUsageForNoSuchNextMouseStroke(ShortcutInputManager inputManager, IShortcutUsage usage, GroupedShortcut shortcut, MouseStroke stroke) {
            base.OnCancelUsageForNoSuchNextMouseStroke(inputManager, usage, shortcut, stroke);
            BroadcastShortcutActivity("No such shortcut for next mouse stroke: " + stroke);
        }

        protected internal override void OnCancelUsageForNoSuchNextKeyStroke(ShortcutInputManager inputManager, IShortcutUsage usage, GroupedShortcut shortcut, KeyStroke stroke) {
            base.OnCancelUsageForNoSuchNextKeyStroke(inputManager, usage, shortcut, stroke);
            BroadcastShortcutActivity("No such shortcut for next key stroke: " + stroke);
        }

        protected internal override void OnNoSuchShortcutForMouseStroke(ShortcutInputManager inputManager, string @group, MouseStroke stroke) {
            base.OnNoSuchShortcutForMouseStroke(inputManager,group, stroke);
            BroadcastShortcutActivity("No such shortcut for mouse stroke: " + stroke + " in group: " + group);
        }

        protected internal override void OnNoSuchShortcutForKeyStroke(ShortcutInputManager inputManager, string @group, KeyStroke stroke) {
            base.OnNoSuchShortcutForKeyStroke(inputManager, group, stroke);
            if (stroke.IsKeyDown) {
                BroadcastShortcutActivity("No such shortcut for key stroke: " + stroke + " in group: " + group);
            }
        }

        private static void BroadcastShortcutActivity(string msg) {
            // lazy. This should be done via event handling
            if (Application.Current.MainWindow is EditorWindow window) {
                window.ActivityTextBlock.Text = msg;
            }
        }
    }
}