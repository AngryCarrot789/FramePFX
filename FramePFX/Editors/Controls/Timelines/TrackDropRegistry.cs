using System;
using System.Numerics;
using System.Windows;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.ResourceManaging.Resources;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Services.Messages;

namespace FramePFX.Editors.Controls.Timelines {
    public static class TrackDropRegistry {
        public static DragDropRegistry<Track> DropRegistry { get; }

        static TrackDropRegistry() {
            DropRegistry = new DragDropRegistry<Track>();

            DropRegistry.RegisterNative<Track>(NativeDropTypes.FileDrop, (handler, objekt, type, c) => {
                return objekt.GetData(NativeDropTypes.FileDrop) is string[] files && files.Length > 0 ? EnumDropType.Copy : EnumDropType.None;
            }, async (model, objekt, type, c) => {
                string[] files = (string[]) objekt.GetData(NativeDropTypes.FileDrop);
                MessageBox.Show($"Dropping files directly into the timeline is not implemented yet.\nYou dropped: {string.Join(", ", files)}", "STILL TODO");
            });

            DropRegistry.Register<VideoTrack, ResourceItem>((track, resource, dt, ctx) => {
                return resource is ResourceColour || resource is ResourceImage || resource is ResourceTextStyle || resource is ResourceAVMedia
                    ? EnumDropType.Copy
                    : EnumDropType.None;
            }, async (track, resource, dt, ctx) => {
                if (!ctx.TryGetContext(DataKeys.TrackDropFrameKey, out long frame)) {
                    MessageBox.Show("Drag drop error: no track frame location", "Drop err");
                    return;
                }

                if (!resource.IsOnline) {
                    MessageBox.Show("Cannot add an offline resource to the timeline", "Resource Offline");
                    return;
                }

                if (resource.UniqueId == ResourceManager.EmptyId || !resource.IsRegistered()) {
                    MessageBox.Show("This resource is not registered yet. This is a bug", "Invalid resource");
                    return;
                }

                double fps = track.Timeline.Project.Settings.FrameRate;
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
                            RectSize = new Vector2(200, 200)
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
                    default: return;
                }

                theNewClip.AddEffect(new MotionEffect());
                track.AddClip(theNewClip);
                track.InvalidateRender();
            });
        }
    }
}