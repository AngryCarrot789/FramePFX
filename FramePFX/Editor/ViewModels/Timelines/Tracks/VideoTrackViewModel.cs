using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.History;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Editor.Timelines;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Editor.Timelines.Tracks;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Editor.ViewModels.Timelines.Removals;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.History;
using FramePFX.History.ViewModels;
using FramePFX.Utils;
using FramePFX.Views.Dialogs.Message;

namespace FramePFX.Editor.ViewModels.Timelines.Tracks
{
    public class VideoTrackViewModel : TrackViewModel
    {
        public const string OpacityHistoryKey = "video-track.Opacity";

        private HistoryTrackOpacity opacityHistory;

        private static readonly MessageDialog SliceCloneTextResourceDialog;

        static VideoTrackViewModel()
        {
            SliceCloneTextResourceDialog = new MessageDialog("reference")
            {
                ShowAlwaysUseNextResultOption = true,
                Header = "Reference or copy text resource?",
                Message = "Do you want to reference the same text resource (shared text, font, etc), or clone it (creating a new resource)?"
            };
            SliceCloneTextResourceDialog.AddButton("Reference", "reference", true);
            SliceCloneTextResourceDialog.AddButton("Copy", "copy", true);
            SliceCloneTextResourceDialog.AddButton("Cancel", "cancel", true);
        }

        public new VideoTrack Model => (VideoTrack) base.Model;

        public double Opacity
        {
            get => this.Model.Opacity;
            set
            {
                if (this.IsAutomationRefreshInProgress || this.Model.IsAutomationChangeInProgress)
                {
                    Debugger.Break();
                    return;
                }

                if (!this.IsHistoryChanging)
                {
                    if (FrontEndHistoryHelper.ActiveDragId == OpacityHistoryKey)
                    {
                        if (this.opacityHistory == null)
                            this.opacityHistory = new HistoryTrackOpacity(this);
                        this.opacityHistory.Opacity.SetCurrent(value);
                        FrontEndHistoryHelper.OnDragEnd = FrontEndHistoryHelper.OnDragEnd ?? ((s, cancel) =>
                        {
                            if (cancel)
                            {
                                this.IsHistoryChanging = true;
                                this.Opacity = this.opacityHistory.Opacity.Original;
                                this.IsHistoryChanging = false;
                            }
                            else
                            {
                                HistoryManagerViewModel.Instance.AddAction(this.opacityHistory, "Edit opacity");
                            }

                            this.opacityHistory = null;
                        });
                    }
                    else
                    {
                        HistoryTrackOpacity action = new HistoryTrackOpacity(this);
                        action.Opacity.SetCurrent(value);
                        HistoryManagerViewModel.Instance.AddAction(action, "Edit opacity");
                    }
                }

                if (AutomationUtils.GetNewKeyFrameTime(this, VideoTrack.OpacityKey, out long frame))
                {
                    this.AutomationData[VideoTrack.OpacityKey].GetActiveKeyFrameOrCreateNew(frame).SetDoubleValue(value);
                }
                else
                {
                    this.AutomationData[VideoTrack.OpacityKey].GetOverride().SetDoubleValue(value);
                }
            }
        }

        public bool IsVisible
        {
            get => this.Model.IsVisible;
            set
            {
                if (this.IsVisible == value)
                {
                    return;
                }

                if (this.IsAutomationRefreshInProgress || this.Model.IsAutomationChangeInProgress)
                {
                    Debugger.Break();
                    return;
                }

                if (!this.IsHistoryChanging)
                {
                    HistoryManagerViewModel.Instance.AddAction(new HistoryTrackIsVisible(this, value), "Edit IsVisible");
                }

                if (AutomationUtils.GetNewKeyFrameTime(this, VideoTrack.IsVisibleKey, out long frame))
                {
                    this.AutomationData[VideoTrack.IsVisibleKey].GetActiveKeyFrameOrCreateNew(frame).SetBooleanValue(value);
                }
                else
                {
                    this.AutomationData[VideoTrack.IsVisibleKey].GetOverride().SetBooleanValue(value);
                }
            }
        }

        private static readonly RefreshAutomationValueEventHandler RefreshOpacityHandler = (s, e) =>
        {
            VideoTrackViewModel track = (VideoTrackViewModel) s.AutomationData.Owner;
            track.RaisePropertyChanged(nameof(track.Opacity));
            track.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshIsVisibleHandler = (s, e) =>
        {
            VideoTrackViewModel track = (VideoTrackViewModel) s.AutomationData.Owner;
            track.RaisePropertyChanged(nameof(track.IsVisible));
            track.InvalidateRenderForAutomationRefresh(in e);
        };

        public AutomationSequenceViewModel OpacityAutomationSequence => this.AutomationData[VideoTrack.OpacityKey];

        public RelayCommand ResetOpacityCommand { get; }
        public RelayCommand InsertOpacityKeyFrameCommand { get; }
        public RelayCommand ToggleOpacityActiveCommand { get; }

        public VideoTrackViewModel(VideoTrack model) : base(model)
        {
            this.AutomationData.AssignRefreshHandler(VideoTrack.OpacityKey, RefreshOpacityHandler);
            this.AutomationData.AssignRefreshHandler(VideoTrack.IsVisibleKey, RefreshIsVisibleHandler);

            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoTrack.OpacityKey.Descriptor.DefaultValue);
            this.InsertOpacityKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoTrack.OpacityKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.Timeline.PlayHeadFrame, 0)).SetDoubleValue(this.Opacity), () => this.Timeline != null);
            this.ToggleOpacityActiveCommand = new RelayCommand(() => this.AutomationData[VideoTrack.OpacityKey].ToggleOverrideAction());
        }

        public override bool CanDropResource(ResourceItemViewModel resource)
        {
            return resource is ResourceAVMediaViewModel ||
                   resource is ResourceColourViewModel ||
                   resource is ResourceImageViewModel ||
                   resource is ResourceTextStyleViewModel ||
                   resource is ResourceMpegMediaViewModel ||
                   resource is ResourceCompositionViewModel;
        }

        public override async Task OnResourceDropped(ResourceItemViewModel resource, long frame)
        {
            if (!resource.Model.IsOnline)
            {
                await Services.DialogService.ShowMessageAsync("Resource Offline", "Cannot add an offline resource to the timeline");
                return;
            }

            if (resource.UniqueId == ResourceManager.EmptyId || !resource.Model.IsRegistered())
            {
                await Services.DialogService.ShowMessageAsync("Invalid resource", "This resource is not registered yet. This is a bug");
                return;
            }

            double fps = this.Timeline.Project.Settings.FrameRate.ToDouble;
            long defaultDuration = (long) (fps * 5);

            Clip newClip;
            switch (resource.Model)
            {
                case ResourceAVMedia media when media.IsValidMediaFile:
                {
                    TimeSpan span = media.GetDuration();
                    long dur = (long) Math.Floor(span.TotalSeconds * fps);
                    if (dur < 2)
                    {
                        // image files are 1
                        dur = defaultDuration;
                    }

                    if (dur > 0)
                    {
                        long newProjectDuration = frame + dur + 600;
                        if (newProjectDuration > this.Timeline.MaxDuration)
                        {
                            this.Timeline.MaxDuration = newProjectDuration;
                        }

                        AVMediaVideoClip clip = new AVMediaVideoClip()
                        {
                            FrameSpan = new FrameSpan(frame, dur),
                            DisplayName = "Media Clip"
                        };

                        clip.ResourceAVMediaKey.SetTargetResourceId(media.UniqueId);
                        newClip = clip;
                    }
                    else
                    {
                        await Services.DialogService.ShowMessageAsync("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                        return;
                    }

                    break;
                }
                case ResourceAVMedia media:
                    await Services.DialogService.ShowMessageAsync("Invalid media", "?????????? Demuxer is closed");
                    return;
                case ResourceColour argb:
                {
                    ShapeSquareVideoClip clip = new ShapeSquareVideoClip()
                    {
                        FrameSpan = new FrameSpan(frame, defaultDuration),
                        DisplayName = "Shape Clip"
                    };

                    clip.GetDefaultKeyFrame(ShapeSquareVideoClip.WidthKey).SetFloatValue(200);
                    clip.GetDefaultKeyFrame(ShapeSquareVideoClip.HeightKey).SetFloatValue(200);
                    clip.ColourKey.SetTargetResourceId(argb.UniqueId);
                    newClip = clip;
                    break;
                }
                case ResourceImage img:
                {
                    ImageVideoClip clip = new ImageVideoClip()
                    {
                        FrameSpan = new FrameSpan(frame, defaultDuration),
                        DisplayName = "Image Clip"
                    };

                    clip.ImageKey.SetTargetResourceId(img.UniqueId);
                    newClip = clip;
                    break;
                }
                case ResourceTextStyle text:
                {
                    TextVideoClip clip = new TextVideoClip()
                    {
                        FrameSpan = new FrameSpan(frame, defaultDuration),
                        DisplayName = "Text Clip"
                    };

                    clip.TextStyleKey.SetTargetResourceId(text.UniqueId);
                    newClip = clip;
                    break;
                }
                case ResourceComposition comp:
                {
                    CompositionVideoClip clip = new CompositionVideoClip()
                    {
                        FrameSpan = new FrameSpan(frame, defaultDuration),
                        DisplayName = "Composition clip"
                    };

                    clip.ResourceCompositionKey.SetTargetResourceId(comp.UniqueId);
                    newClip = clip;
                    break;
                }
                default: return;
            }

            newClip.AddEffect(new MotionEffect());
            this.CreateClip(newClip);
            if (newClip is VideoClip videoClipModel)
            {
                videoClipModel.InvalidateRender();
            }
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration)
        {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;
            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (ClipViewModel clipViewModel in this.Clips)
            {
                if (clipViewModel is VideoClipViewModel clip)
                {
                    long clipBegin = clip.FrameBegin;
                    long clipDuration = clip.FrameDuration;
                    long clipEnd = clipBegin + clipDuration;
                    if (clipEnd <= spanBegin && clipBegin >= spanEnd)
                    {
                        continue; // not intersecting
                    }

                    if (spanBegin <= clipBegin)
                    {
                        // cut the left part away
                        if (spanEnd >= clipEnd)
                        {
                            // remove clip entirely
                            range.AddRemovedClip(clip);
                        }
                        else if (spanEnd <= clipBegin)
                        {
                            // not intersecting
                            continue;
                        }
                        else
                        {
                            range.AddSplitClip(clip, null, FrameSpan.FromIndex(spanEnd, clipEnd));
                        }
                    }
                    else if (spanEnd >= clipEnd)
                    {
                        // cut the right part away
                        if (spanBegin >= clipEnd)
                        {
                            // not intersecting
                            continue;
                        }
                        else
                        {
                            range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), null);
                        }
                    }
                    else
                    {
                        // fully intersecting; double split
                        range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), FrameSpan.FromIndex(spanEnd, clipEnd));
                    }
                }
            }

            return range;
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e)
        {
            VideoEditorViewModel editor; // slight performance helper
            if (!e.IsDuringPlayback && (editor = this.Editor) != null && !editor.Playback.IsPlaying)
            {
                this.Timeline.DoAutomationTickAndRenderToPlayback(true);
            }
        }

        public bool GetSpanUntilClip(long frame, out FrameSpan span, long unlimitedDuration = 300)
        {
            long minimum = long.MaxValue;
            if (this.Clips.Count > 0)
            {
                foreach (ClipViewModel clip in this.Clips)
                {
                    if (clip.FrameBegin > frame)
                    {
                        if (clip.IntersectsFrameAt(frame))
                        {
                            span = default;
                            return false;
                        }
                        else
                        {
                            minimum = Math.Min(clip.FrameBegin, minimum);
                            if (minimum <= frame)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            if (minimum > frame && minimum != long.MaxValue)
            {
                span = FrameSpan.FromIndex(frame, minimum);
            }
            else
            {
                span = new FrameSpan(frame, unlimitedDuration);
            }

            return true;
        }
    }
}