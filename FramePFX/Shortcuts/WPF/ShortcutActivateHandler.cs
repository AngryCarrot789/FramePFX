using System.Threading.Tasks;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.WPF {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutInputManager inputManager, GroupedShortcut shortcut);
}