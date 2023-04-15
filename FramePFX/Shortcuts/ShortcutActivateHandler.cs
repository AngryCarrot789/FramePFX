using System.Threading.Tasks;
using SharpPadV2.Core.Shortcuts.Managing;

namespace SharpPadV2.Shortcuts {
    public delegate Task<bool> ShortcutActivateHandler(ShortcutProcessor processor, GroupedShortcut shortcut);
}