namespace FramePFX.Core.Views.Dialogs.FilePicking {
    public interface IFilePickDialogService {
        DialogResult<string[]> ShowFilePickerDialogAsync(string filter, string defaultPath = null, string titleBar = null, bool multiSelect = false);

        DialogResult<string> ShowFolderPickerDialogAsync(string defaultPath = null, string titleBar = null);
    }
}