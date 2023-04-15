namespace FramePFX.Core.Views.Dialogs.FilePicking {
    public interface IFilePickDialogService {
        DialogResult<string[]> ShowFilePickerDialog(string filter, string defaultPath = null, string titleBar = null, bool multiSelect = false);

        DialogResult<string> ShowFolderPickerDialog(string defaultPath = null, string titleBar = null);
        DialogResult<string> ShowSaveFileDialog(string filter, string defaultPath = null, string titleBar = null);
    }
}