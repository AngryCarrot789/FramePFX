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

using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Resources;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Core;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.FilePicking;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.Timelines.Commands;

public abstract class AddClipCommand<T> : Command where T : Clip {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return DataKeys.TrackKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track? track)) {
            return;
        }

        FrameSpan span = new FrameSpan(0, 300);
        if (DataKeys.TrackContextMouseFrameKey.TryGetContext(e.ContextData, out long frame)) {
            span = span.WithBegin(frame);
        }

        T clip = this.NewInstance();
        clip.FrameSpan = span;
        await this.OnPreAddToTrack(track, clip, e.ContextData);

        // Is this way too over secure???

        using ErrorList list = new ErrorList("Exception adding clip to track");

        bool success = false;
        try {
            track.AddClip(clip);
            success = true;
        }
        catch (Exception ex) {
            list.Add(ex);
        }
        finally {
            try {
                await this.OnPostAddToTrack(track, clip, success, e.ContextData);
            }
            catch (Exception ex) {
                list.Add(ex);
            }
        }
    }

    protected virtual bool IsAllowedInTrack(Track track, T clip) {
        return track.IsClipTypeAccepted(clip.GetType());
    }

    protected virtual T NewInstance() {
        return (T) ClipFactory.Instance.NewClip(ClipFactory.Instance.GetId(typeof(T)));
    }

    protected virtual Task OnPreAddToTrack(Track track, T clip, IContextData ctx) {
        return Task.CompletedTask;
    }

    protected virtual Task OnPostAddToTrack(Track track, T clip, bool success, IContextData ctx) {
        return Task.CompletedTask;
    }
}

public class AddTextClipCommand : AddClipCommand<TextVideoClip>;

public class AddTimecodeClipCommand : AddClipCommand<TimecodeClip>;

public class AddVideoClipShapeCommand : AddClipCommand<VideoClipShape> {
    protected override async Task OnPostAddToTrack(Track track, VideoClipShape clip, bool success, IContextData ctx) {
        if (DataKeys.VideoEditorUIKey.TryGetContext(ctx, out IVideoEditorWindow? videoEditor)) {
            ISelectionManager<BaseResource> selection = videoEditor.ResourceManager.List.Selection;
            if (selection.Count == 1 && selection.SelectedItems.First() is ResourceColour colour && colour.IsRegistered()) {
                if (await IMessageDialogService.Instance.ShowMessage("Link resource", $"Link '{colour.DisplayName ?? "Selected Media Resource"}' to this clip?", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    clip.ResourceHelper.SetResource(VideoClipShape.ColourKey, colour);
                }
            }
        }
    }
}

public class AddImageVideoClipCommand : AddClipCommand<ImageVideoClip> {
    protected override async Task OnPreAddToTrack(Track track, ImageVideoClip clip, IContextData ctx) {
        ResourceManager? resMan;
        if (!DataKeys.VideoEditorUIKey.TryGetContext(ctx, out IVideoEditorWindow? videoEditor))
            return;

        if ((resMan = videoEditor.ResourceManager.ResourceManager) == null)
            return;

        if (MessageBoxResult.Yes == await IMessageDialogService.Instance.ShowMessage("Open image", "Do you want to open up an image file for this new clip?", MessageBoxButton.YesNo)) {
            string? path = await IFilePickDialogService.Instance.OpenFile("Open an image file for this image?", Filters.CombinedImageTypesAndAll);
            if (path != null) {
                ResourceImage resourceImage = new ResourceImage();
                ResourceFolder targetFolder;
                if (videoEditor.ResourceManager.List.CurrentFolderTreeNode?.Resource is ResourceFolder folder) {
                    (targetFolder = folder).AddItem(resourceImage);
                }
                else {
                    (targetFolder = resMan.RootContainer).AddItem(resourceImage);
                }

                resourceImage.FilePath = path;
                if (await IResourceLoaderDialogService.Instance.TryLoadResource(resourceImage)) {
                    clip.ResourceHelper.SetResource(ImageVideoClip.ResourceImageKey, resourceImage);
                }
                else {
                    targetFolder.RemoveItem(resourceImage, destroy: true);
                }
            }
        }
    }
}

public class AddCompositionVideoClipCommand : AddClipCommand<CompositionVideoClip>;