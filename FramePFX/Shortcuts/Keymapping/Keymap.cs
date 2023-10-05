using System;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Keymapping {
    /// <summary>
    /// Stores information about a keymap
    /// </summary>
    public class Keymap {
        /// <summary>
        /// The keymap version
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The root shortcut group
        /// </summary>
        public ShortcutGroup Root { get; set; }

        public Keymap() {
        }
    }
}