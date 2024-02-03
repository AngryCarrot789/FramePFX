using System.Collections.Generic;
using System.IO;
using FramePFX.Services.Files;
using FramePFX.Views;
using Microsoft.Win32;

namespace FramePFX.Services.WPF.Files {
    public class WPFFilePickDialogService : IFilePickDialogService {
        public string OpenFile(string message, string filter = null, string initialDirectory = null) {
            OpenFileDialog dialog = new OpenFileDialog() {
                InitialDirectory = initialDirectory ?? "",
                Title = message ?? "Open a file",
                Filter = filter ?? string.Empty,
                Multiselect = false
            };

            if (dialog.ShowDialog(WindowEx.GetCurrentActiveWindow()) == true) {
                string path = dialog.FileName;
                if (!string.IsNullOrWhiteSpace(path)) {
                    return path;
                }
            }

            return null;
        }

        public string[] OpenMultipleFiles(string message, string filter = null, string initialDirectory = null) {
            OpenFileDialog dialog = new OpenFileDialog() {
                InitialDirectory = initialDirectory ?? "",
                Title = message ?? "Open files",
                Filter = filter ?? string.Empty,
                Multiselect = true
            };

            if (dialog.ShowDialog(WindowEx.GetCurrentActiveWindow()) == true) {
                List<string> fileNames = new List<string>();
                foreach (string filePath in dialog.FileNames) {
                    if (!string.IsNullOrWhiteSpace(filePath))
                        fileNames.Add(filePath);
                }

                return fileNames.Count > 0 ? fileNames.ToArray() : null;
            }

            return null;
        }

        public string SaveFile(string message, string filter = null, string initialFilePath = null) {
            SaveFileDialog dialog = new SaveFileDialog() {
                Title = message ?? "Open files",
                Filter = filter ?? string.Empty
            };

            if (initialFilePath != null) {
                dialog.InitialDirectory = Path.GetDirectoryName(initialFilePath) ?? string.Empty;
                dialog.FileName = Path.GetFileName(initialFilePath);
            }

            if (dialog.ShowDialog(WindowEx.GetCurrentActiveWindow()) == true) {
                string path = dialog.FileName;
                if (!string.IsNullOrWhiteSpace(path)) {
                    return path;
                }
            }

            return null;
        }
    }
}