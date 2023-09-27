using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Logger;
using FramePFX.Utils;
using Microsoft.Win32;
using FramePFX.Views.Dialogs.FilePicking;

namespace FramePFX.WPF.Views.FilePicking {
    [ServiceImplementation(typeof(IFilePickDialogService))]
    public class FilePickDialogService : IFilePickDialogService {
        public Task<string[]> OpenFiles(string filter, string defaultDirectory = null, string titleBar = null, bool multiSelect = false) {
            Dispatcher dispatcher;
            if ((dispatcher = Application.Current?.Dispatcher) == null)
                throw new Exception("Application main thread is unavailable");
            if (dispatcher.CheckAccess())
                return Task.FromResult(this.OpenFilesInternal(filter, defaultDirectory, titleBar, multiSelect));
            return dispatcher.InvokeAsync(() => this.OpenFilesInternal(filter, defaultDirectory, titleBar, multiSelect)).Task;
        }

        public Task<string> OpenFolder(string initialPath = null, string titleBar = null) {
            Dispatcher dispatcher;
            if ((dispatcher = Application.Current?.Dispatcher) == null)
                throw new Exception("Application main thread is unavailable");
            if (dispatcher.CheckAccess())
                return Task.FromResult(this.OpenFolderInternal(initialPath, titleBar));
            return dispatcher.InvokeAsync(() => this.OpenFolderInternal(initialPath, titleBar)).Task;
        }

        public Task<string> SaveFile(string filter, string initialPath = null, string titleBar = null) {
            Dispatcher dispatcher;
            if ((dispatcher = Application.Current?.Dispatcher) == null)
                throw new Exception("Application main thread is unavailable");
            if (dispatcher.CheckAccess())
                return Task.FromResult(this.SaveFileInternal(filter, initialPath, titleBar));
            return dispatcher.InvokeAsync(() => this.SaveFileInternal(filter, initialPath, titleBar)).Task;
        }

        public string[] OpenFilesInternal(string filter, string defaultDirectory = null, string titleBar = null, bool multiSelect = false) {
            OpenFileDialog dialog = new OpenFileDialog {
                Filter = filter ?? "",
                Multiselect = multiSelect,
                Title = titleBar ?? "Select a file"
            };

            if (defaultDirectory != null) {
                dialog.InitialDirectory = defaultDirectory;
            }

            string[] array;
            if (dialog.ShowDialog() != true || (array = dialog.FileNames).Length < 1) {
                return null;
            }

            if (multiSelect) {
                return (array = array.Where(x => !string.IsNullOrEmpty(x)).ToArray()).Length > 0 ? array : null;
            }
            else if (array.Length == 1 && !string.IsNullOrEmpty(array[0])) {
                return array;
            }
            else {
                return null;
            }
        }

        public string OpenFolderInternal(string defaultPath = null, string titleBar = null) {
            FolderPicker picker = new FolderPicker {
                Title = titleBar ?? "Select a folder"
            };

            if (defaultPath != null) {
                picker.InputPath = defaultPath;
            }

            return picker.ShowDialog() == true && !string.IsNullOrEmpty(picker.ResultPath) ? picker.ResultPath : null;
        }

        public string SaveFileInternal(string filter, string defaultPath = null, string titleBar = null) {
            SaveFileDialog dialog = new SaveFileDialog {
                Title = titleBar ?? "Save a file",
                Filter = filter ?? "All files|*.*"
            };

            if (defaultPath != null) {
                try {
                    dialog.InitialDirectory = Path.GetDirectoryName(defaultPath) ?? "";
                    dialog.FileName = Path.GetFileName(defaultPath);
                }
                catch (Exception e) {
                    AppLogger.WriteLine("An error occurred calculating default path for save file dialog\n" + e.GetToString());
                }
            }

            return dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.FileName) ? dialog.FileName : null;
        }
    }
}