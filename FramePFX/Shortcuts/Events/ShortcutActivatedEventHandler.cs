using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.Managing;

namespace FramePFX.Shortcuts.Events {
    public delegate bool ShortcutActivatedEventHandler(ShortcutInputManager inputManager, GroupedShortcut shortcut, IDataContext context);
}