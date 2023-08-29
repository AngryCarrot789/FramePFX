using System.Threading.Tasks;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts {
    public interface IShortcutHandler {
        Task<bool> OnShortcutActivated(ShortcutProcessor processor, GroupedShortcut shortcut);
    }
}