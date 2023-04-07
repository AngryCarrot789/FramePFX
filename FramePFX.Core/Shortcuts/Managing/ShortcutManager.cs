using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MCNBTViewer.Core.Shortcuts.Inputs;
using MCNBTViewer.Core.Shortcuts.Serialization;
using MCNBTViewer.Core.Shortcuts.Usage;

namespace MCNBTViewer.Core.Shortcuts.Managing {
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

        public abstract ShortcutProcessor NewProcessor();
    }
}