using System.Threading.Tasks;
using FramePFX.Actions.Contexts;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Events {
    public delegate Task<bool> ShortcutActivatedEventHandler(ShortcutInputManager inputManager, GroupedShortcut shortcut, IDataContext context);
}