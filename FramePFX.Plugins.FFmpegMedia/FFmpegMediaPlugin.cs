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

using FFmpeg.AutoGen;
using FramePFX.BaseFrontEnd.PropertyEditing;
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;
using FramePFX.CommandSystem;
using FramePFX.Editing;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Commands;
using FramePFX.Editing.ResourceManaging.UI;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Commands;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Plugins.FFmpegMedia.Clips;
using FramePFX.Plugins.FFmpegMedia.Exporter;
using FramePFX.Plugins.FFmpegMedia.Resources;
using FramePFX.Plugins.FFmpegMedia.Resources.Controls;
using FramePFX.Services.FilePicking;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.Plugins.FFmpegMedia;

/// <summary>
/// This plugin implements FFmpeg clips and resources to allow video playback via FFmpeg
/// </summary>
public class FFmpegMediaPlugin : Plugin {
    public override void RegisterCommands(CommandManager manager) {
        manager.Register("commands.editor.AddAVMediaClip", new AddAVMediaClipCommand());
        manager.Register("commands.resources.AddResourceAVMedia", new AddResourceAVMediaCommand());
    }

    public override Task OnApplicationLoaded() {
        ResourceTypeFactory.Instance.RegisterType("r_avmedia", typeof(ResourceAVMedia));
        ClipFactory.Instance.RegisterType("vc_avmedia", typeof(AVMediaVideoClip));

        ResourceDropRegistry.FileDropInFolder += OnHandleResourceNativeFileDrop;

        ClipDropRegistry.DropRegistry.Register<AVMediaVideoClip, ResourceAVMedia>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) => {
            await clip.ResourceHelper.SetResourceHelper(AVMediaVideoClip.MediaKey, h);
        });

        ResourceDropOnTimelineService.Instance.Register(typeof(ResourceAVMedia), new AvMediaDropHandler());

        ResourceExplorerListItemContent.Registry.RegisterType<ResourceAVMedia>(() => new RELIC_AVMedia());
        InvalidResourceEntryControl.Registry.RegisterType<InvalidMediaPathEntry>(() => new InvalidMediaPathEntryControl());
        
        // Exporter
        BasePropertyEditorSlotControl.RegisterEnumProperty<AVCodecID, DataParameterAVCodecIDPropertyEditorSlot>();

        ExporterRegistry.Instance.RegisterExporter(new ExporterKey("exporter_ffmpeg", "FFmpeg"), new FFmpegExporterInfo());
        
        return Task.CompletedTask;
    }

    public override void GetXamlResources(List<string> paths) {
        paths.Add("FFmpegMediaStyles.axaml");
    }

    private static bool OnHandleResourceNativeFileDrop(ResourceFolder folder, List<BaseResource> resources, string path, string extension) {
        switch (extension) {
            case ".gif":
            case ".mp3":
            case ".wav":
            case ".ogg":
            case ".mp4":
            case ".wmv":
            case ".avi":
            case ".avchd":
            case ".f4v":
            case ".swf":
            case ".mov":
            case ".mkv":
            case ".qt":
            case ".webm":
            case ".flv": {
                ResourceAVMedia media = new ResourceAVMedia() { FilePath = path, DisplayName = Path.GetFileName(path) };

                resources.Add(media);
                folder.AddItem(media);
                return true;
            }
        }

        return false;
    }
    
    private class AvMediaDropHandler : IResourceDropHandler {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) {
            if (resource.Manager == null)
                return -1;

            TimeSpan duration = ((ResourceAVMedia) resource).GetDuration();
            double fps = resource.Manager.Project.Settings.FrameRateDouble;

            return (long) (duration.TotalSeconds * fps);
        }

        public async Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span) {
            if (resource.HasReachedResourceLimit()) {
                int count = resource.ResourceLinkLimit;
                await IMessageDialogService.Instance.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
                return;
            }

            ResourceAVMedia media = (ResourceAVMedia) resource;
            AVMediaVideoClip clip = new AVMediaVideoClip();
            clip.FrameSpan = span;
            await clip.ResourceHelper.SetResourceHelper(AVMediaVideoClip.MediaKey, media);

            track.AddClip(clip);
        }
    }

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
}