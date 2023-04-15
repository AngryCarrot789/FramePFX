using FramePFX.Core.Views.Dialogs;
using FramePFX.Core.Views.Dialogs.FilePicking;
using Microsoft.Win32;

namespace FramePFX.Views.Dialogs.FilePicking {
    public class FilePickDialogService : IFilePickDialogService {
        public DialogResult<string[]> ShowFilePickerDialog(string filter, string defaultPath = null, string titleBar = null, bool multiSelect = false) {
            OpenFileDialog dialog = new OpenFileDialog {
                Filter = filter,
                Multiselect = multiSelect,
                Title = titleBar ?? "Select a file"
            };

            if (defaultPath != null) {
                dialog.InitialDirectory = defaultPath;
            }

            return dialog.ShowDialog() == true ? new DialogResult<string[]>(dialog.FileNames) : new DialogResult<string[]>(false);
        }

        public DialogResult<string> ShowFolderPickerDialog(string defaultPath = null, string titleBar = null) {
            FolderPicker picker = new FolderPicker {
                Title = titleBar ?? "Select a folder"
            };

            if (defaultPath != null) {
                picker.InputPath = defaultPath;
            }

            return picker.ShowDialog() == true ? new DialogResult<string>(picker.ResultPath) : new DialogResult<string>(false);
        }

        public DialogResult<string> ShowSaveFileDialog(string filter, string defaultPath = null, string titleBar = null) {
            SaveFileDialog dialog = new SaveFileDialog {
                Title = titleBar ?? "Save a file",
                Filter = filter ?? "All files|*.*"
            };

            if (defaultPath != null) {
                dialog.FileName = defaultPath;
            }

            return dialog.ShowDialog() == true ? new DialogResult<string>(dialog.FileName) : new DialogResult<string>(false);
        }
    }
}