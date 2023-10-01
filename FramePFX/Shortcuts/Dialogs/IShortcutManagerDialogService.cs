namespace FramePFX.Shortcuts.Dialogs
{
    public interface IShortcutManagerDialogService
    {
        bool IsOpen { get; }

        void ShowEditorDialog();
    }
}