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

using System;
using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.ResourceHelpers;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Controls.Timelines
{
    public static class TrackDropRegistry
    {
        public static DragDropRegistry<Track> DropRegistry { get; }

        static TrackDropRegistry()
        {
            DropRegistry = new DragDropRegistry<Track>();

            DropRegistry.RegisterNative<Track>(NativeDropTypes.FileDrop, (handler, objekt, type, c) =>
            {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, (model, objekt, type, c) =>
            {
                string[] files = (string[]) objekt.GetData(NativeDropTypes.FileDrop);
                IoC.MessageService.ShowMessage("STILL TODO", $"Dropping files directly into the timeline is not implemented yet.\nYou dropped: {string.Join(", ", files)}");
                return Task.CompletedTask;
            });

            DropRegistry.Register<VideoTrack, ResourceItem>((track, resource, dt, ctx) =>
            {
                return resource is ResourceColour || resource is ResourceImage || resource is ResourceTextStyle || resource is ResourceAVMedia || resource is ResourceComposition
                    ? EnumDropType.Copy
                    : EnumDropType.None;
            }, (track, resource, dt, ctx) =>
            {
                if (!DataKeys.TrackDropFrameKey.TryGetContext(ctx, out long frame))
                {
                    IoC.MessageService.ShowMessage("Drop err", "Drag drop error: no track frame location");
                    return Task.CompletedTask;
                }

                if (!resource.IsOnline)
                {
                    IoC.MessageService.ShowMessage("Resource Offline", "Cannot add an offline resource to the timeline");
                    return Task.CompletedTask;
                }

                if (resource.UniqueId == ResourceManager.EmptyId || !resource.IsRegistered())
                {
                    IoC.MessageService.ShowMessage("Invalid resource", "This resource is not registered yet. This is a bug");
                    return Task.CompletedTask;
                }

                IBaseResourcePathKey autoLoadKey = null;
                double fps = track.Project.Settings.FrameRate.AsDouble;
                FrameSpan defaultSpan = track.GetSpanUntilClipOrLimitedDuration(frame, (long) (fps * 5));
                Clip theNewClip;
                switch (resource)
                {
                    case ResourceAVMedia media:
                    {
                        if (media.HasReachedResourecLimit())
                        {
                            int count = media.ResourceLinkLimit;
                            IoC.MessageService.ShowMessage("Resource Limit", $"This resource cannot be used by more than {count} clip{Lang.S(count)}");
                            return Task.CompletedTask;
                        }

                        if (media.Demuxer == null)
                        {
                            IoC.MessageService.ShowMessage("Resource demuxer offline", "The resource's demuxer is not available");
                            return Task.CompletedTask;
                        }

                        TimeSpan span = media.GetDuration();
                        long dur = (long) Math.Floor(span.TotalSeconds * fps);
                        if (dur < 1)
                        {
                            IoC.MessageService.ShowMessage("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                            return Task.CompletedTask;
                        }

                        // image files are 1
                        if (dur == 1)
                        {
                            dur = defaultSpan.Duration;
                        }

                        long newProjectDuration = frame + dur + 600;
                        if (newProjectDuration > track.Timeline.MaxDuration)
                        {
                            track.Timeline.MaxDuration = newProjectDuration;
                        }

                        AVMediaVideoClip clip = new AVMediaVideoClip()
                        {
                            FrameSpan = new FrameSpan(frame, dur),
                            DisplayName = "Media Clip"
                        };

                        clip.ResourceAVMediaKey.SetTargetResourceId(media.UniqueId);
                        autoLoadKey = clip.ResourceAVMediaKey;
                        theNewClip = clip;

                        break;
                    }
                    case ResourceColour argb:
                    {
                        VideoClipShape clip = new VideoClipShape
                        {
                            FrameSpan = defaultSpan,
                            DisplayName = "Shape Clip",
                            Size = new Vector2(200, 200)
                        };

                        clip.ColourKey.SetTargetResourceId(argb.UniqueId);
                        autoLoadKey = clip.ColourKey;
                        theNewClip = clip;
                        break;
                    }
                    case ResourceImage img:
                    {
                        ImageVideoClip clip = new ImageVideoClip()
                        {
                            FrameSpan = defaultSpan,
                            DisplayName = "Image Clip"
                        };

                        clip.ResourceImageKey.SetTargetResourceId(img.UniqueId);
                        autoLoadKey = clip.ResourceImageKey;
                        theNewClip = clip;
                        break;
                    }
                    case ResourceComposition comp:
                    {
                        CompositionVideoClip clip = new CompositionVideoClip()
                        {
                            FrameSpan = defaultSpan,
                        };
                        clip.ResourceCompositionKey.SetTargetResourceId(comp.UniqueId);
                        autoLoadKey = clip.ResourceCompositionKey;
                        theNewClip = clip;
                        break;
                    }
                    default: return Task.CompletedTask;
                }

                track.AddClip(theNewClip);
                track.InvalidateRender();
                autoLoadKey?.TryLoadLink();
                return Task.CompletedTask;
            });
        }
    }
}