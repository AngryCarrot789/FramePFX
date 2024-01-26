using System;
using System.Windows.Input;
using System.Xml;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Keymapping;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.WPF {
    public class WPFKeyMapSerialiser : XMLShortcutSerialiser {
        public static readonly WPFKeyMapSerialiser Instance = new WPFKeyMapSerialiser();

        public WPFKeyMapSerialiser() {
        }

        protected override void SerialiseKeystroke(XmlDocument doc, XmlElement elem, in KeyStroke stroke, string childElementName = "KeyStroke") {
            XmlElement element = doc.CreateElement(childElementName);
            if (stroke.Modifiers != 0) {
                element.SetAttribute("Mods", ModsToString((ModifierKeys) stroke.Modifiers));
            }

            Key key = (Key) stroke.KeyCode;
            if (Enum.IsDefined(typeof(Key), key)) {
                element.SetAttribute("Key", KeyUtils.KeyToString(key));
            }
            else {
                element.SetAttribute("KeyCode", stroke.KeyCode.ToString());
            }

            if (stroke.IsRelease) {
                element.SetAttribute("IsRelease", "true");
            }

            elem.AppendChild(element);
        }

        protected override void SerialiseMousestroke(XmlDocument doc, XmlElement elem, in MouseStroke stroke, string childElementName = "MouseStroke") {
            XmlElement element = doc.CreateElement(childElementName);
            if (stroke.Modifiers != 0) {
                element.SetAttribute("Mods", ModsToString((ModifierKeys) stroke.Modifiers));
            }

            string btn;
            switch (stroke.MouseButton) {
                case 0:
                    btn = "Left";
                    break;
                case 1:
                    btn = "Middle";
                    break;
                case 2:
                    btn = "Right";
                    break;
                case 3:
                    btn = "X1";
                    break;
                case 4:
                    btn = "X2";
                    break;
                case WPFShortcutManager.BUTTON_WHEEL_UP:
                    btn = "WHEEL_UP";
                    break;
                case WPFShortcutManager.BUTTON_WHEEL_DOWN:
                    btn = "WHEEL_DOWN";
                    break;
                default: throw new Exception("Invalid mouse button: " + stroke.MouseButton);
            }

            element.SetAttribute("Button", btn);
            if (stroke.ClickCount > 0)
                element.SetAttribute("ClickCount", stroke.ClickCount.ToString());
            if (stroke.WheelDelta != 0)
                element.SetAttribute("WheelDelta", stroke.WheelDelta.ToString());
            if (stroke.IsRelease)
                element.SetAttribute("IsRelease", "true");
            elem.AppendChild(element);
        }

        protected override KeyStroke DeserialiseKeyStroke(XmlElement element) {
            string keyText = GetAttributeNullable(element, "Key");
            string keyCodeText = GetAttributeNullable(element, "KeyCode");
            string modsText = GetAttributeNullable(element, "Mods");
            string isReleaseText = GetAttributeNullable(element, "IsRelease");

            int keyCode;
            Key key = KeyUtils.ParseKey(keyText);
            if (key != Key.None) {
                keyCode = (int) key;
            }
            else if (!int.TryParse(keyCodeText, out keyCode)) {
                if (keyText != null) {
                    throw new Exception($"Unknown key: {keyText}");
                }

                if (keyCodeText != null) {
                    throw new Exception($"Unknown key code point: '{keyCodeText}'");
                }

                throw new Exception($"Missing the Key attribute or KeyCode attribute");
            }

            int mods = (int) StringToMods(modsText);
            bool isRelease = "true".Equals(isReleaseText, StringComparison.OrdinalIgnoreCase);
            return new KeyStroke(keyCode, mods, isRelease);
        }

        protected override MouseStroke DeserialiseMouseStroke(XmlElement element) {
            string modsText = GetAttributeNullable(element, "Mods");
            string buttonText = GetAttributeNullable(element, "Button");
            string isReleaseText = GetAttributeNullable(element, "IsRelease");
            string clickCountText = GetAttributeNullable(element, "ClickCount");
            string wheelDeltaText = GetAttributeNullable(element, "WheelDelta");
            if (string.IsNullOrWhiteSpace(buttonText)) {
                throw new Exception("Missing mouse button");
            }

            int mouseButton;
            switch (buttonText.ToLower()) {
                case "lmb":
                case "left":
                    mouseButton = 0;
                    break;
                case "mmb": // middle mouse button
                case "mwb": // mouse wheel button
                case "middle":
                    mouseButton = 1;
                    break;
                case "rmb":
                case "right":
                    mouseButton = 2;
                    break;
                case "x1":
                    mouseButton = 3;
                    break;
                case "x2":
                    mouseButton = 4;
                    break;
                case "wheel_up":
                case "wheelup":
                    mouseButton = WPFShortcutManager.BUTTON_WHEEL_UP;
                    break;
                case "wheel_down":
                case "wheeldown":
                    mouseButton = WPFShortcutManager.BUTTON_WHEEL_DOWN;
                    break;
                default: {
                    if (!int.TryParse(buttonText, out mouseButton)) {
                        throw new Exception("Invalid mouse button: " + buttonText);
                    }

                    break;
                }
            }

            int mods = (int) StringToMods(modsText);
            if (string.IsNullOrWhiteSpace(clickCountText) || !int.TryParse(clickCountText, out int clickCout)) {
                clickCout = -1;
            }

            if (string.IsNullOrWhiteSpace(wheelDeltaText) || !int.TryParse(wheelDeltaText, out int wheelDelta)) {
                wheelDelta = 0;
            }

            bool isRelease = "true".Equals(isReleaseText, StringComparison.OrdinalIgnoreCase);
            return new MouseStroke(mouseButton, mods, isRelease, clickCout, wheelDelta);
        }

        public static string ModsToString(ModifierKeys keys) {
            StringJoiner joiner = new StringJoiner("+");
            if ((keys & ModifierKeys.Control) != 0)
                joiner.Append("CTRL");
            if ((keys & ModifierKeys.Alt) != 0)
                joiner.Append("ALT");
            if ((keys & ModifierKeys.Shift) != 0)
                joiner.Append("SHIFT");
            if ((keys & ModifierKeys.Windows) != 0)
                joiner.Append("WIN");
            return joiner.ToString();
        }

        public static ModifierKeys StringToMods(string mods) {
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
                switch (part.Trim().ToLower()) {
                    case "ctrl":
                        mod = ModifierKeys.Control;
                        break;
                    case "alt":
                        mod = ModifierKeys.Alt;
                        break;
                    case "shift":
                        mod = ModifierKeys.Shift;
                        break;
                    case "win":
                        mod = ModifierKeys.Windows;
                        break;
                    default: continue;
                }

                keys |= mod;
            }

            return keys;
        }
    }
}