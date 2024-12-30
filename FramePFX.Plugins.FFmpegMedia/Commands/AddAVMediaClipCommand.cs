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
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Plugins.FFmpegMedia.Clips;
using FramePFX.Plugins.FFmpegMedia.Resources;
using FramePFX.Services.Messaging;

namespace FramePFX.Plugins.FFmpegMedia.Commands;

public class AddAVMediaClipCommand : AddClipCommand<AVMediaVideoClip> {
    protected override async Task OnPostAddToTrack(Track track, AVMediaVideoClip clip, bool success, IContextData ctx) {
        if (DataKeys.VideoEditorUIKey.TryGetContext(ctx, out IVideoEditorWindow? videoEditor)) {
            ISelectionManager<BaseResource> selection = videoEditor.ResourceManager.List.Selection;
            if (selection.Count == 1 && selection.SelectedItems.First() is ResourceAVMedia media && media.IsRegistered()) {
                if (await IMessageDialogService.Instance.ShowMessage("Link resource", $"Link '{media.DisplayName ?? "Selected Media Resource"}' to this clip?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    await clip.ResourceHelper.SetResourceHelper(AVMediaVideoClip.MediaKey, media);
                }
            }
        }
    }
}