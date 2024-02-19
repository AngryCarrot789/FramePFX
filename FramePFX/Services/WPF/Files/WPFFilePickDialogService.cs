// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

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