using System.Windows.Input;

namespace FramePFX.Editor.Timeline.Utils {
    public static class InputUtils {
        public static bool AreModifiersPressed(ModifierKeys key1) {
            return (Keyboard.Modifiers & key1) != 0;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2) {
            return (Keyboard.Modifiers & (key1 | key2)) != 0;
        }

        public static bool AreModifiersPressed(ModifierKeys key1, ModifierKeys key2, ModifierKeys key3) {
            return (Keyboard.Modifiers & (key1 | key2 | key3)) != 0;
        }

        public static bool AreModifiersPressed(params ModifierKeys[] keys) {
            ModifierKeys modifiers = ModifierKeys.None;
            foreach (ModifierKeys modifier in keys)
                modifiers |= modifier;
            return AreModifiersPressed(modifiers);
        }
    }
}