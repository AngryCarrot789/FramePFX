using System.Windows.Input;

namespace FramePFX.Core.Shortcuts {
    public interface IShortcutToCommand {
        ICommand GetCommandForShortcut(string shortcutId);
    }
}