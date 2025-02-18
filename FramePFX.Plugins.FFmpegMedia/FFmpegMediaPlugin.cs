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
using FramePFX.BaseFrontEnd.ResourceManaging;
using FramePFX.BaseFrontEnd.ResourceManaging.Autoloading;
using PFXToolKitUI.Avalonia.PropertyEditing;
using FramePFX.Editing;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.Exporting;
using FramePFX.Editing.Factories;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Plugins.FFmpegMedia.Clips;
using FramePFX.Plugins.FFmpegMedia.Commands;
using FramePFX.Plugins.FFmpegMedia.Exporter;
using FramePFX.Plugins.FFmpegMedia.Resources;
using FramePFX.Plugins.FFmpegMedia.Resources.Controls;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Plugins;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace FramePFX.Plugins.FFmpegMedia;

/// <summary>
/// This plugin implements FFmpeg clips and resources to allow video playback via FFmpeg
/// </summary>
public class FFmpegMediaPlugin : Plugin {
    public override void RegisterCommands(CommandManager manager) {
        // Register our custom commands
        manager.Register("commands.editor.AddAVMediaClip", new AddAVMediaClipCommand());
        manager.Register("commands.resources.AddResourceAVMedia", new AddResourceAVMediaCommand());
    }

    public override void GetXamlResources(List<string> paths) {
        // Tell FramePFX the relative path (to the assembly root, I believe) of our custom control resources.
        paths.Add("FFmpegMediaStyles.axaml");
    }

    public override Task OnApplicationLoaded() {
        // Register our custom media resource and clip types. We have to do this so
        // that FramePFX can deserialise a project and create the appropriate clip,
        // since 'vc_avmedia' will be the clip type when serialising
        ResourceTypeFactory.Instance.RegisterType("r_avmedia", typeof(ResourceAVMedia));
        ClipFactory.Instance.RegisterType("vc_avmedia", typeof(AVMediaVideoClip));

        // Register a handler for when dropping a resource into the timeline
        ResourceDropOnTimelineService.Instance.Register(typeof(ResourceAVMedia), new AvMediaDropHandler());

        // Register a handler for when a resource is dropped ONTO a clip.
        // This "links" the resource to the clip, letting the clip render the media
        ClipDropRegistry.DropRegistry.Register<AVMediaVideoClip, ResourceAVMedia>((clip, h, dt, ctx) => EnumDropType.Link, async (clip, h, dt, c) => {
            await clip.ResourceHelper.SetResourceHelper(AVMediaVideoClip.MediaKey, h);
        });

        // Register our custom but blank control for the resource list item content. At some point we may add a mini view port to it so,
        // we can easily scrub through the video. We could also just select a random frame and draw it. But for now, it contains nothing
        ResourceExplorerListItemContent.Registry.RegisterType<ResourceAVMedia>(() => new RELIC_AVMedia());

        // Register our invalid entry control for the resource loader dialog system, which shows the user
        // the error encountered and also the media's file path, so they can change it if they wanted to
        InvalidResourceEntryControl.Registry.RegisterType<InvalidMediaPathEntry>(() => new InvalidMediaPathEntryControl());

        // Register an enum type with the property editor registry, so that we can use DataParameterAVCodecIDPropertyEditorSlot.
        BasePropertyEditorSlotControl.RegisterEnumControl<AVCodecID, DataParameterAVCodecIDPropertyEditorSlot>();

        // Register FFmpeg exporter
        ExporterRegistry.Instance.RegisterExporter(new ExporterKey("exporter_ffmpeg", "FFmpeg"), new FFmpegExporterInfo());

        // Register context menu entries
        RegisterResourceContextMenus();
        RegisterTrackContextMenus();

        // Register a drop handler when a native file is dropped into the resource manager tree or list.
        // This is a janky implementation using a CLR event but it works :-)
        ResourceDropRegistry.FileDropInFolder += OnHandleResourceNativeFileDrop;

        return Task.CompletedTask;
    }

    private static void RegisterResourceContextMenus() {
        // Some commands can only work on the resource manager's tree, while others only work on the list.
        // This is why there are 2 registries (there's actually a 3rd one for ResourceItems only)
        FixedContextGroup[] list = [
            ResourceContextRegistry.ResourceSurfaceContextRegistry.GetFixedGroup("modify.subcreation"),
            ResourceContextRegistry.ResourceFolderContextRegistry.GetFixedGroup("modify.subcreation")
        ];

        foreach (FixedContextGroup group in list) {
            group.AddEntry(new CommandContextEntry("commands.resources.AddResourceAVMedia", "Add Media", "Create a new media resource"));
        }
    }

    private static void RegisterTrackContextMenus() {
        // Register context menu item for adding a media clip
        FixedContextGroup modAdd = TrackContextRegistry.TimelineTrackContextRegistry.GetFixedGroup("modify.addclips");
        modAdd.AddCommand("commands.editor.AddAVMediaClip", "Add Video Media clip", "Create a new media clip for playing videos or most types of media", SimpleIcons.VideoIcon);
    }

    private static bool OnHandleResourceNativeFileDrop(ResourceFolder folder, List<BaseResource> resources, string path, string extension) {
        if (Filters.CombinedVideoTypes.MatchFilePath(extension) == true) {
            ResourceAVMedia media = new ResourceAVMedia() { FilePath = path, DisplayName = Path.GetFileName(path) };

            resources.Add(media);
            folder.AddItem(media);
            return true;
        }

        // switch (extension) {
        //     case ".gif":
        //     case ".mp3":
        //     case ".wav":
        //     case ".ogg":
        //     case ".mp4":
        //     case ".wmv":
        //     case ".avi":
        //     case ".avchd":
        //     case ".f4v":
        //     case ".swf":
        //     case ".mov":
        //     case ".mkv":
        //     case ".qt":
        //     case ".webm":
        //     case ".flv": {
        //         break;
        //     }
        // }

        return false;
    }

    private class AvMediaDropHandler : IResourceDropHandler {
        public long GetClipDurationForDrop(Track track, ResourceItem resource) {
            // Should never be the case, hopefully. It's a bug if it is null
            if (resource.Manager == null) {
                return -1;
            }

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
}