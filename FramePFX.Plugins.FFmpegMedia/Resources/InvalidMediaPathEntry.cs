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

using FramePFX.Editing.ResourceManaging.Autoloading;
using PFXToolKitUI.Utils;

namespace FramePFX.Plugins.FFmpegMedia.Resources;

public class InvalidMediaPathEntry : InvalidResourceEntry {
    public new ResourceAVMedia Resource => (ResourceAVMedia) base.Resource;

    private string? filePath;

    public string? FilePath {
        get => this.filePath;
        set {
            if (this.filePath == value)
                return;
            this.filePath = value;
            this.FilePathChanged?.Invoke(this);
        }
    }

    private string? exceptionMessage;

    public string? ExceptionMessage {
        get => this.exceptionMessage;
        set {
            if (this.exceptionMessage == value)
                return;

            this.exceptionMessage = value;
            this.ExceptionMessageChanged?.Invoke(this);
        }
    }

    public event InvalidResourceEntryEventHandler? FilePathChanged;
    public event InvalidResourceEntryEventHandler? ExceptionMessageChanged;

    public InvalidMediaPathEntry(ResourceAVMedia resource, Exception? exception) : this(resource, exception?.GetToString() ?? "<no error available>") {
    }

    public InvalidMediaPathEntry(ResourceAVMedia resource, string errorMessage) : base(resource) {
        this.DisplayName = resource.DisplayName ?? "Invalid Media";
        this.ExceptionMessage = errorMessage;
        this.FilePath = resource.FilePath;
    }
}