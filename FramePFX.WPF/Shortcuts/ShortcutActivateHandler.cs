using System.Threading.Tasks;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.WPF.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, GroupedShortcut shortcut);
}