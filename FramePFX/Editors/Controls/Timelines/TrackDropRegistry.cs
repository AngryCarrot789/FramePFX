using System;
using System.Numerics;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Clips.Core;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Controls.Timelines {
    public static class TrackDropRegistry {
        public static DragDropRegistry<Track> DropRegistry { get; }

        static TrackDropRegistry() {
            DropRegistry = new DragDropRegistry<Track>();

            DropRegistry.RegisterNative<Track>(NativeDropTypes.FileDrop, (handler, objekt, type, c) => {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, async (model, objekt, type, c) => {
                string[] files = (string[]) objekt.GetData(NativeDropTypes.FileDrop);
                IoC.MessageService.ShowMessage("STILL TODO", $"Dropping files directly into the timeline is not implemented yet.\nYou dropped: {string.Join(", ", files)}");
            });

            DropRegistry.Register<VideoTrack, ResourceItem>((track, resource, dt, ctx) => {
                return resource is ResourceColour || resource is ResourceImage || resource is ResourceTextStyle || resource is ResourceAVMedia || resource is ResourceComposition
                    ? EnumDropType.Copy
                    : EnumDropType.None;
            }, async (track, resource, dt, ctx) => {
                if (!DataKeys.TrackDropFrameKey.TryGetContext(ctx, out long frame)) {
                    IoC.MessageService.ShowMessage("Drop err", "Drag drop error: no track frame location");
                    return;
                }

                if (!resource.IsOnline) {
                    IoC.MessageService.ShowMessage("Resource Offline", "Cannot add an offline resource to the timeline");
                    return;
                }

                if (resource.UniqueId == ResourceManager.EmptyId || !resource.IsRegistered()) {
                    IoC.MessageService.ShowMessage("Invalid resource", "This resource is not registered yet. This is a bug");
                    return;
                }

                double fps = track.Project.Settings.FrameRate.AsDouble;
                FrameSpan defaultSpan = track.GetSpanUntilClipOrLimitedDuration(frame, (long) (fps * 5));
                Clip theNewClip;
                switch (resource) {
                    case ResourceAVMedia media: {
                        if (media.Demuxer == null) {
                            IoC.MessageService.ShowMessage("Resource demuxer offline", "The resource's demuxer is not available");
                            return;
                        }

                        TimeSpan span = media.GetDuration();
                        long dur = (long) Math.Floor(span.TotalSeconds * fps);
                        if (dur < 1) {
                            IoC.MessageService.ShowMessage("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                            return;
                        }

                        // image files are 1
                        if (dur == 1) {
                            dur = defaultSpan.Duration;
                        }

                        long newProjectDuration = frame + dur + 600;
                        if (newProjectDuration > track.Timeline.MaxDuration) {
                            track.Timeline.MaxDuration = newProjectDuration;
                        }

                        AVMediaVideoClip clip = new AVMediaVideoClip() {
                            FrameSpan = new FrameSpan(frame, dur),
                            DisplayName = "Media Clip"
                        };

                        clip.ResourceAVMediaKey.SetTargetResourceId(media.UniqueId);
                        theNewClip = clip;

                        break;
                    }
                    case ResourceColour argb: {
                        VideoClipShape clip = new VideoClipShape {
                            FrameSpan = defaultSpan,
                            DisplayName = "Shape Clip",
                            Size = new Vector2(200, 200)
                        };

                        clip.ColourKey.SetTargetResourceId(argb.UniqueId);
                        theNewClip = clip;
                        break;
                    }
                    case ResourceImage img: {
                        ImageVideoClip clip = new ImageVideoClip() {
                            FrameSpan = defaultSpan,
                            DisplayName = "Image Clip"
                        };

                        clip.ResourceImageKey.SetTargetResourceId(img.UniqueId);
                        theNewClip = clip;
                        break;
                    }
                    case ResourceComposition comp: {
                        CompositionVideoClip clip = new CompositionVideoClip() {
                            FrameSpan = defaultSpan,
                        };
                        clip.ResourceCompositionKey.SetTargetResourceId(comp.UniqueId);
                        theNewClip = clip;
                        break;
                    }
                    default: return;
                }

                track.AddClip(theNewClip);
                track.InvalidateRender();
            });
        }
    }
}