// 
// Copyright (c) 2024-2024 REghZy
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
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using FramePFX.Services.FilePicking;
using Path = System.IO.Path;

namespace FramePFX.Avalonia.Services.Files;

public class FilePickDialogServiceImpl : IFilePickDialogService {
    public async Task<string?> OpenFile(string? message, string? filter = null, string? initialPath = null) {
        if (!App.RZApplicationImpl.TryGetActiveWindow(out Window? window)) {
            return null;
        }
        
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        IReadOnlyList<IStorageFile> list = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = message ?? "Pick a file", AllowMultiple = false, SuggestedFileName = fileName
        });

        return list.Count != 1 ? null : list[0].Path.AbsolutePath;
    }

    public async Task<string[]?> OpenMultipleFiles(string? message, string? filter = null, string? initialPath = null) {
        if (!App.RZApplicationImpl.TryGetActiveWindow(out Window? window)) {
            return null;
        }
        
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        IReadOnlyList<IStorageFile> list = await window.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions() {
            Title = message ?? "Pick some files", AllowMultiple = true, SuggestedFileName = fileName
        });

        return list.Count == 0 ? null : list.Select(x => x.Path.AbsolutePath).ToArray();
    }

    public async Task<string?> SaveFile(string? message, string? filter = null, string? initialPath = null, bool warnOverwrite = true) {
        if (!App.RZApplicationImpl.TryGetActiveWindow(out Window? window)) {
            return null;
        }
        
        string? fileName = initialPath != null ? Path.GetFileName(initialPath) : initialPath;
        string? extension = fileName != null ? Path.GetExtension(fileName) : null;
        IStorageFile? item = await window.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions() {
            Title = message ?? "Save a file", SuggestedFileName = fileName, DefaultExtension = extension, ShowOverwritePrompt = warnOverwrite
        });

        return item?.Path.AbsolutePath;
    }
}