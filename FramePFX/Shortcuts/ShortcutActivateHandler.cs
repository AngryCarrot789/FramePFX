using System.Threading.Tasks;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, GroupedShortcut shortcut);
}