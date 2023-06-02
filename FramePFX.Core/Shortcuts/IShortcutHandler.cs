using System.Threading.Tasks;
using FrameControlEx.Core.Shortcuts.Managing;

namespace FrameControlEx.Core.Shortcuts {
    public interface IShortcutHandler {
        Task<bool> OnShortcutActivated(ShortcutProcessor processor, GroupedShortcut shortcut);
    }
}