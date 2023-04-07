using System.Threading.Tasks;
using MCNBTViewer.Core.Shortcuts.Managing;

namespace MCNBTViewer.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, ManagedShortcut shortcut);
}