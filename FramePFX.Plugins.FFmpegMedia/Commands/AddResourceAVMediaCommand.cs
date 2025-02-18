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

using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Plugins.FFmpegMedia.Resources;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Utils;

namespace FramePFX.Plugins.FFmpegMedia.Commands;

public class AddResourceAVMediaCommand : AddResourceCommand<ResourceAVMedia> {
    protected override async Task OnPostAddToFolder(ResourceFolder folder, ResourceAVMedia resource, IResourceTreeNodeElement? folderUI, IResourceTreeNodeElement? resourceUI, bool success, IContextData ctx) {
        if (!success)
            return;

        string? path = await IFilePickDialogService.Instance.OpenFile("Open a media file for this resource?", Filters.CombinedVideoTypesAndAll);
        if (path != null) {
            resource.FilePath = path;
            resource.DisplayName = Path.GetFileName(path);
            if (string.IsNullOrWhiteSpace(resource.DisplayName))
                resource.DisplayName = "New AVMedia";
            await IResourceLoaderDialogService.Instance.TryLoadResource(resource);
        }
    }
}