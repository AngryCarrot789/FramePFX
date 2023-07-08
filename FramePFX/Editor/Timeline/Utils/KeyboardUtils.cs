using System.Windows.Input;

namespace FramePFX.Editor.Timeline.Utils {
    public static class KeyboardUtils {
        public static bool AreModifiersPressed(ModifierKeys key1) {
            return (Keyboard.Modifiers & key1) == key1;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2) {
            return AreModifiersPressed(key1 | key2);
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3) {
            return AreModifiersPressed(key1 | key2 | key3);
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3, ModifierKeys key4) {
            return AreModifiersPressed(key1 | key2 | key3 | key4);
        }

        public static bool AreModifiersPressed(params ModifierKeys[] keys) {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (ModifierKeys modifier in keys)
                modifiers |= modifier;
            return AreModifiersPressed(modifiers);
        }
    }
}