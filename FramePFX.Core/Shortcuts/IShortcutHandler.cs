using System.Threading.Tasks;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts {
    public interface IShortcutHandler {
        Task<bool> OnShortcutActivated(ShortcutProcessor processor, GroupedShortcut shortcut);
    }
}