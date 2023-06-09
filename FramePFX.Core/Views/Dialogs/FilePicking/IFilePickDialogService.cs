namespace FramePFX.Core.Views.Dialogs.FilePicking {
    public interface IFilePickDialogService {
        DialogResult<string[]> OpenFiles(string filter, string defaultPath = null, string titleBar = null, bool multiSelect = false);
        DialogResult<string> OpenFolder(string defaultPath = null, string titleBar = null);
        DialogResult<string> SaveFile(string filter, string defaultPath = null, string titleBar = null);
    }
}