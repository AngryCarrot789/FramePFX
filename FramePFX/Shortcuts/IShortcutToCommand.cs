using System.Windows.Input;

namespace FramePFX.Shortcuts {
    public interface IShortcutToCommand {
        ICommand GetCommandForShortcut(string shortcutId);
    }
}