using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Editor.ViewModels.Timeline.Removals;
using FramePFX.Core.History;
using FramePFX.Core.Utils;
using FramePFX.Core.Views.Dialogs.Message;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Tracks {
    public class VideoTrackViewModel : TrackViewModel {
        public const string OpacityHistoryKey = "video-track.Opacity";

        private HistoryTrackOpacity opacityHistory;

        private static readonly MessageDialog SliceCloneTextResourceDialog;

        static VideoTrackViewModel() {
            SliceCloneTextResourceDialog = new MessageDialog("reference") {
                ShowAlwaysUseNextResultOption = true,
                Header = "Reference or copy text resource?",
                Message = "Do you want to reference the same text resource (shared text, font, etc), or clone it (creating a new resource)?"
            };
            SliceCloneTextResourceDialog.AddButton("Reference", "reference", true);
            SliceCloneTextResourceDialog.AddButton("Copy", "copy", true);
            SliceCloneTextResourceDialog.AddButton("Cancel", "cancel", true);
        }

        public new VideoTrackModel Model => (VideoTrackModel) base.Model;

        public double Opacity {
            get => this.Model.Opacity;
            set {
                if (this.IsAutomationRefreshInProgress || this.Model.IsAutomationChangeInProgress) {
                    Debugger.Break();
                    return;
                }

                if (!this.IsHistoryChanging) {
                    if (FrontEndHistoryHelper.ActiveDragId == OpacityHistoryKey) {
                        if (this.opacityHistory == null)
                            this.opacityHistory = new HistoryTrackOpacity(this);
                        this.opacityHistory.Opacity.SetCurrent(value);
                        FrontEndHistoryHelper.OnDragEnd = FrontEndHistoryHelper.OnDragEnd ?? ((s, cancel) => {
                            if (cancel) {
                                this.IsHistoryChanging = true;
                                this.Opacity = this.opacityHistory.Opacity.Original;
                                this.IsHistoryChanging = false;
                            }
                            else {
                                this.HistoryManager.AddAction(this.opacityHistory, "Edit opacity");
                            }

                            this.opacityHistory = null;
                        });
                    }
                    else {
                        HistoryTrackOpacity action = new HistoryTrackOpacity(this);
                        action.Opacity.SetCurrent(value);
                        this.HistoryManager.AddAction(action, "Edit opacity");
                    }
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoTrackModel.OpacityKey)) {
                    this.AutomationData[VideoTrackModel.OpacityKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame).SetDoubleValue(value);
                }
                else {
                    this.AutomationData[VideoTrackModel.OpacityKey].GetOverride().SetDoubleValue(value);
                    this.AutomationData[VideoTrackModel.OpacityKey].RaiseOverrideValueChanged();
                }
            }
        }

        public bool IsVisible {
            get => this.Model.IsVisible;
            set {
                if (this.IsVisible == value) {
                    return;
                }

                if (this.IsAutomationRefreshInProgress || this.Model.IsAutomationChangeInProgress) {
                    Debugger.Break();
                    return;
                }

                if (!this.IsHistoryChanging) {
                    this.HistoryManager.AddAction(new HistoryTrackIsVisible(this, value), "Edit IsVisible");
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoTrackModel.IsVisibleKey)) {
                    this.AutomationData[VideoTrackModel.IsVisibleKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame).SetBooleanValue(value);
                }
                else {
                    this.AutomationData[VideoTrackModel.IsVisibleKey].GetOverride().SetBooleanValue(value);
                    this.AutomationData[VideoTrackModel.IsVisibleKey].RaiseOverrideValueChanged();
                }
            }
        }

        public VideoTrackViewModel(TimelineViewModel timeline, VideoTrackModel model) : base(timeline, model) {
            this.AutomationData.AssignRefreshHandler(VideoTrackModel.OpacityKey, (s, f) => this.OnAutomationPropertyUpdated(nameof(this.Opacity), in f));
            this.AutomationData.AssignRefreshHandler(VideoTrackModel.IsVisibleKey, (s, f) => this.OnAutomationPropertyUpdated(nameof(this.IsVisible), in f));
        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return resource is ResourceMediaViewModel || resource is ResourceColourViewModel || resource is ResourceImageViewModel || resource is ResourceTextViewModel;
        }

        public override async Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin) {
            Validate.Exception(!string.IsNullOrEmpty(resource.UniqueId), "Expected valid resource UniqueId");
            if (!resource.Model.IsRegistered()) {
                await IoC.MessageDialogs.ShowMessageAsync("Invalid resource", "This resource is not registered yet");
                return;
            }

            double fps = this.Timeline.Project.Settings.FrameRate.ActualFPS;
            long defaultDuration = (long) (fps * 5);

            ClipModel newClip = null;
            if (resource.Model is ResourceMedia media) {
                media.OpenMediaFromFile();
                TimeSpan span = media.GetDuration();
                long dur = (long) Math.Floor(span.TotalSeconds * fps);
                if (dur < 2) {
                    // image files are 1
                    dur = defaultDuration;
                }

                if (dur > 0) {
                    long newProjectDuration = frameBegin + dur + 600;
                    if (newProjectDuration > this.Timeline.MaxDuration) {
                        this.Timeline.MaxDuration = newProjectDuration;
                    }

                    MediaClipModel clip = new MediaClipModel() {
                        FrameSpan = new FrameSpan(frameBegin, dur),
                        DisplayName = media.UniqueId
                    };

                    clip.SetTargetResourceId(media.UniqueId);
                    newClip = clip;
                }
                else {
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid media", "This media has a duration of 0 and cannot be added to the timeline");
                    return;
                }
            }
            else {
                if (resource.Model is ResourceColour argb) {
                    ShapeClipModel clip = new ShapeClipModel() {
                        FrameSpan = new FrameSpan(frameBegin, defaultDuration),
                        Width = 200, Height = 200,
                        DisplayName = argb.UniqueId
                    };

                    clip.SetTargetResourceId(argb.UniqueId);
                    newClip = clip;
                }
                else if (resource.Model is ResourceImage img) {
                    ImageClipModel clip = new ImageClipModel() {
                        FrameSpan = new FrameSpan(frameBegin, defaultDuration),
                        DisplayName = img.UniqueId
                    };

                    clip.SetTargetResourceId(img.UniqueId);
                    newClip = clip;
                }
                else if (resource.Model is ResourceText text) {
                    TextClipModel clip = new TextClipModel() {
                        FrameSpan = new FrameSpan(frameBegin, defaultDuration),
                        DisplayName = text.UniqueId
                    };

                    clip.SetTargetResourceId(text.UniqueId);
                    newClip = clip;
                }
                else {
                    return;
                }
            }

            this.CreateClip(newClip);
            if (newClip is VideoClipModel videoClipModel) {
                videoClipModel.InvalidateRender();
            }
        }

        public VideoClipRangeRemoval GetRangeRemoval(long spanBegin, long spanDuration) {
            if (spanDuration < 0)
                throw new ArgumentOutOfRangeException(nameof(spanDuration), "Span duration cannot be negative");
            long spanEnd = spanBegin + spanDuration;
            VideoClipRangeRemoval range = new VideoClipRangeRemoval();
            foreach (ClipViewModel clipViewModel in this.Clips) {
                if (clipViewModel is VideoClipViewModel clip) {
                    long clipBegin = clip.FrameBegin;
                    long clipDuration = clip.FrameDuration;
                    long clipEnd = clipBegin + clipDuration;
                    if (clipEnd <= spanBegin && clipBegin >= spanEnd) {
                        continue; // not intersecting
                    }
                    if (spanBegin <= clipBegin) { // cut the left part away
                        if (spanEnd >= clipEnd) {
                            // remove clip entirely
                            range.AddRemovedClip(clip);
                        }
                        else if (spanEnd <= clipBegin) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, null, FrameSpan.FromIndex(spanEnd, clipEnd));
                        }
                    }
                    else if (spanEnd >= clipEnd) { // cut the right part away
                        if (spanBegin >= clipEnd) { // not intersecting
                            continue;
                        }
                        else {
                            range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), null);
                        }
                    }
                    else { // fully intersecting; double split
                        range.AddSplitClip(clip, FrameSpan.FromIndex(clipBegin, spanBegin), FrameSpan.FromIndex(spanEnd, clipEnd));
                    }
                }
            }
            return range;
        }

        protected override void OnAutomationPropertyUpdated(string propertyName, in RefreshAutomationValueEventArgs e) {
            base.OnAutomationPropertyUpdated(propertyName, e);
            VideoEditorViewModel editor; // slight performance helper
            if (!e.IsDuringPlayback && (editor = this.Editor) != null && !editor.Playback.IsPlaying) {
                this.Timeline.DoRender(true);
            }
        }
    }
}