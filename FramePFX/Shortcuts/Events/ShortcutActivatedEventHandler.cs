using System.Threading.Tasks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Events {
    public delegate Task<bool> ShortcutActivatedEventHandler(ShortcutInputManager inputManager, GroupedShortcut shortcut, IDataContext context);
}