using System;
using System.Windows.Input;

namespace FramePFX.WPF.Utils {
    public static class KeyUtils {
        public static Key ParseKey(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return Key.None;
            }

            // 'A' == 65 | 'Z' == 90
            // 'a' == 97 | 'z' == 122

            char first = input[0]; // Parse D0-D9
            if (input.Length == 2 && (first == 'D' || first == 'd') && input[1] >= '0' && input[1] <= '9') {
                return (Key) (input[1] - '0') + (int) Key.D0;
            }

            if (input.Length == 1) {
                if (first >= '0' && first <= '9') {
                    // Parse 0-9
                    return (Key) (first - '0') + (int) Key.D0;
                }

                if (first >= 'a' && first <= 'z') {
                    // Parse a-z
                    return (Key) (first - 'a') + (int) Key.A;
                }

                if (first >= 'A' && first <= 'Z') {
                    // Parse A-Z
                    return (Key) (first - 'A') + (int) Key.A;
                }
            }

            switch (input.ToLower()) {
                case "del": return Key.Delete;
                case "leftarrow":
                case "arrowleft":
                    return Key.Left;
                case "rightarrow":
                case "arrowright":
                    return Key.Right;
                case "uparrow":
                case "arrowup":
                    return Key.Up;
                case "downarrow":
                case "arrowdown":
                    return Key.Down;
            }

            // worst case:
            return Enum.TryParse(input, out Key key) ? key : Key.None;
        }

        public static string KeyToString(Key key) {
            if (key >= Key.A && key <= Key.Z) {
                return key.ToString();
            }

            switch (key) {
                case Key.D0: return "0";
                case Key.D1: return "1";
                case Key.D2: return "2";
                case Key.D3: return "3";
                case Key.D4: return "4";
                case Key.D5: return "5";
                case Key.D6: return "6";
                case Key.D7: return "7";
                case Key.D8: return "8";
                case Key.D9: return "9";
            }

            return key.ToString();
        }
    }
}