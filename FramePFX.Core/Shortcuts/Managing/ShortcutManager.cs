namespace FramePFX.Core.Shortcuts.Managing {
    public abstract class ShortcutManager {
        public ShortcutGroup Root { get; set; }

        public ShortcutManager() {
            this.Root = ShortcutGroup.CreateRoot();
        }

        public ShortcutGroup FindGroupByPath(string path) {
            return this.Root.GetGroupByPath(path);
        }

        public ManagedShortcut FindShortcutByPath(string path) {
            return this.Root.GetShortcutByPath(path);
        }
    }
}