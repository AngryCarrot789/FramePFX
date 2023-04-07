using System;
using System.Text;
using System.Windows.Input;
using MCNBTViewer.Core.Shortcuts.Inputs;
using MCNBTViewer.Core.Shortcuts.Serialization;
using MCNBTViewer.Core.Utils;

namespace MCNBTViewer.Shortcuts {
    public class WPFKeyMapDeserialiser : KeyMapDeserialiser {
        public static WPFKeyMapDeserialiser Instance { get; } = new WPFKeyMapDeserialiser();

        public WPFKeyMapDeserialiser() {
        }

        protected override Keystroke SerialiseKeystroke(in KeyStroke stroke) {
            Keystroke keystroke = new Keystroke();
            Key key = (Key) stroke.KeyCode;
            if (Enum.IsDefined(typeof(Key), key)) {
                keystroke.KeyName = key.ToString();
            }
            else {
                keystroke.KeyCode = stroke.KeyCode.ToString();
            }

            keystroke.Mods = ModsToString((ModifierKeys)stroke.Modifiers);
            if (string.IsNullOrWhiteSpace(keystroke.Mods)) {
                keystroke.Mods = null;
            }

            if (stroke.IsKeyRelease) {
                keystroke.IsRelease = "true";
            }

            return keystroke;
        }

        protected override Mousestroke SerialiseMousestroke(in MouseStroke stroke) {
            Mousestroke mousestroke = new Mousestroke();
            string name;
            switch (stroke.MouseButton) {
                case 0: name = "LMB"; break;
                case 1: name = "WMB"; break;
                case 2: name = "RMB"; break;
                case 3: name = "X1"; break;
                case 4: name = "X2"; break;
                case AppShortcutManager.BUTTON_WHEEL_UP: name = "WHEEL_UP"; break;
                case AppShortcutManager.BUTTON_WHEEL_DOWN: name = "WHEEL_DOWN"; break;
                default: throw new Exception("Invalid mouse button: " + stroke.MouseButton);
            }

            mousestroke.Button = name;
            mousestroke.Mods = ModsToString((ModifierKeys) stroke.Modifiers);
            if (string.IsNullOrWhiteSpace(mousestroke.Mods)) {
                mousestroke.Mods = null;
            }

            if (stroke.ClickCount > 0) {
                mousestroke.ClickCount = stroke.ClickCount.ToString();
            }

            if (stroke.WheelDelta != 0) {
                mousestroke.WheelDelta = stroke.WheelDelta.ToString();
            }

            mousestroke.CustomParamInt = stroke.CustomParam != 0 ? stroke.CustomParam.ToString() : null;
            return mousestroke;
        }

        protected override KeyStroke DeserialiseKeystroke(Keystroke stroke) {
            int keyCode;
            if (Enum.TryParse(stroke.KeyName, out Key key)) {
                keyCode = (int) key;
            }
            else if (!int.TryParse(stroke.KeyCode, out keyCode)) {
                throw new Exception($"Invalid key '{stroke.KeyName}' and keycode '{stroke.KeyCode}'");
            }

            int mods = (int) StringToMods(stroke.Mods);
            bool isRelease = "true".Equals(stroke.IsRelease);
            return new KeyStroke(keyCode, mods, isRelease);
        }

        protected override MouseStroke DeserialiseMousestroke(Mousestroke stroke) {
            if (string.IsNullOrWhiteSpace(stroke.Button)) {
                throw new Exception("Missing mouse button");
            }

            int mouseButton;
            switch (stroke.Button) {
                case "LMB": mouseButton = 0; break;
                case "WMB": mouseButton = 1; break;
                case "RMB": mouseButton = 2; break;
                case "X1": mouseButton = 3; break;
                case "X2": mouseButton = 4; break;
                case "WHEEL_UP": mouseButton = AppShortcutManager.BUTTON_WHEEL_UP; break;
                case "WHEEL_DOWN": mouseButton = AppShortcutManager.BUTTON_WHEEL_DOWN; break;
                default: {
                    if (!int.TryParse(stroke.Button, out mouseButton)) {
                        throw new Exception("Invalid mouse button: " + stroke.Button);
                    }

                    break;
                }
            }

            int clickCout;
            int wheelDelta;
            int param;
            int mods = (int) StringToMods(stroke.Mods);
            if (string.IsNullOrWhiteSpace(stroke.ClickCount) || !int.TryParse(stroke.ClickCount, out clickCout)) {
                clickCout = 1;
            }

            if (string.IsNullOrWhiteSpace(stroke.WheelDelta) || !int.TryParse(stroke.WheelDelta, out wheelDelta)) {
                wheelDelta = 0;
            }

            if (string.IsNullOrWhiteSpace(stroke.CustomParamInt) || !int.TryParse(stroke.CustomParamInt, out param)) {
                param = 0;
            }

            return new MouseStroke(mouseButton, mods, clickCout, wheelDelta, param);
        }

        private static string ModsToString(ModifierKeys keys) {
            StringJoiner joiner = new StringJoiner(new StringBuilder(), "+");
            if ((keys & ModifierKeys.Control) != 0) joiner.Append("ctrl");
            if ((keys & ModifierKeys.Alt) != 0)     joiner.Append("alt");
            if ((keys & ModifierKeys.Shift) != 0)   joiner.Append("shift");
            if ((keys & ModifierKeys.Windows) != 0) joiner.Append("win");
            return joiner.ToString();
        }

        private static ModifierKeys StringToMods(string mods) {
            ModifierKeys keys = ModifierKeys.None;
            if (string.IsNullOrWhiteSpace(mods)) {
                return keys;
            }

            string[] parts = mods.Split('+');
            if (parts.Length <= 0) {
                return keys;
            }

            foreach (string part in parts) {
                ModifierKeys mod;
                switch (part.ToLower()) {
                    case "ctrl":  mod = ModifierKeys.Control; break;
                    case "alt":   mod = ModifierKeys.Alt; break;
                    case "shift": mod = ModifierKeys.Shift; break;
                    case "win":   mod = ModifierKeys.Windows; break;
                    default: continue;
                }

                keys |= mod;
            }

            return keys;
        }
    }
}