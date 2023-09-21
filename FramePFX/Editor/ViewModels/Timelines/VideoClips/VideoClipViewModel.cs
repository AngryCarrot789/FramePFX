using System;
using System.Diagnostics;
using FramePFX.Automation.Events;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Commands;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.Utils;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    // TODO: Maybe instead of using inheritance, instead, use composition?
    // Maybe using some sort of trait system, where a clip can have, for example, a
    // transformation trait (pos, scale, origin), video media trait, etc. Or maybe other
    // editors just call them "effects".

    // Premiere pro's "effect controls" contains the "Motion" effect, and it automatically adds
    // an instance that cannot be removed. Maybe that's better than inheritance? And to save/load
    // clips, traits/effects can be serialised and deserialised (using a factory) just like everything else

    // Vegas also does a similar thing but uses "Event Pan/Crop", which is an effect added
    // to clips by default (or at least added when you open the effect window by clicking the button on the clip)

    /// <summary>
    /// Base view model class for video clips that are placed on a video track
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel {
        public new VideoClip Model => (VideoClip) base.Model;

        public double Opacity {
            get => this.Model.Opacity;
            set {
                this.ValidateNotInAutomationChange();
                TimelineViewModel timeline = this.Timeline;
                if (AutomationUtils.GetNewKeyFrameTime(this, VideoClip.OpacityKey, out long frame)) {
                    this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(frame).SetDoubleValue(value);
                }
                else {
                    this.AutomationData[VideoClip.OpacityKey].GetOverride().SetDoubleValue(value);
                    this.AutomationData[VideoClip.OpacityKey].RaiseOverrideValueChanged();
                }
            }
        }

        public RelayCommand ResetOpacityCommand { get; }
        public RelayCommand InsertOpacityKeyFrameCommand { get; }
        public RelayCommand ToggleOpacityActiveCommand { get; }


        // binding helpers

        public AutomationSequenceViewModel OpacityAutomationSequence => this.AutomationData[VideoClip.OpacityKey];

        private readonly ClipRenderInvalidatedEventHandler renderCallback;

        #region Cached refresh event handlers

        private static readonly RefreshAutomationValueEventHandler RefreshOpacityHandler = (s, e) => {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.Opacity));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        #endregion

        public readonly Func<bool> IsPlayHeadFrameInRange;

        protected VideoClipViewModel(VideoClip model) : base(model) {
            this.IsPlayHeadFrameInRange = () => {
                long? frame = this.Timeline?.PlayHeadFrame;
                return frame.HasValue && this.Model.IsTimelineFrameInRange(frame.Value);
            };

            this.ResetOpacityCommand = new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);
            this.InsertOpacityKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetDoubleValue(this.Opacity), this.IsPlayHeadFrameInRange);
            this.ToggleOpacityActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].ToggleOverrideAction());

            this.renderCallback = (x, s) => {
                this.OnInvalidateRender(s); // assert ReferenceEquals(this.Model, x)
            };

            this.Model.RenderInvalidated += this.renderCallback;
            this.AutomationData.AssignRefreshHandler(VideoClip.OpacityKey, RefreshOpacityHandler);
        }

        // TODO: implement "OnPlayHeadEnter", "OnPlayHeadMoved", and "OnPlayHeadLeave" to refresh
        // the key frame insertion commands

        public override void OnUserSeekedFrame(long oldFrame, long newFrame) {
            base.OnUserSeekedFrame(oldFrame, newFrame);
            this.UpdateKeyFrameCommands();
        }

        public override void OnClipMovedToPlayeHeadFrame(long frame) {
            base.OnClipMovedToPlayeHeadFrame(frame);
            this.UpdateKeyFrameCommands();
        }

        public override void OnPlayHeadLeaveClip(bool isCausedByPlayHeadMovement) {
            base.OnPlayHeadLeaveClip(isCausedByPlayHeadMovement);
            this.UpdateKeyFrameCommands();
        }

        private void UpdateKeyFrameCommands() {
            this.InsertOpacityKeyFrameCommand.RaiseCanExecuteChanged();
        }

        public override void OnFrameSpanChanged(FrameSpan oldSpan) {
            base.OnFrameSpanChanged(oldSpan);
            this.Model.InvalidateRender();
        }

        protected override void OnMediaFrameOffsetChanged(long oldFrame, long newFrame) {
            base.OnMediaFrameOffsetChanged(oldFrame, newFrame);
            this.Model.InvalidateRender();
        }

        public virtual void OnInvalidateRender(bool schedule = true) {
            this.Track?.Timeline.DoAutomationTickAndRender(schedule);
        }

        public override void Dispose() {
            this.Model.RenderInvalidated -= this.renderCallback;
            base.Dispose();
        }

        protected void InvalidateRenderForAutomationRefresh(in RefreshAutomationValueEventArgs e) {
            if (!e.IsDuringPlayback && !e.IsPlaybackTick) {
                this.Model.InvalidateRender(true);
            }
        }

        [Conditional("DEBUG")]
        private void ValidateNotInAutomationChange() {
            if (this.IsAutomationRefreshInProgress) {
                Debugger.Break();
                throw new Exception("Cannot modify view-model parameter property while automation refresh is in progress. " +
                                    $"Only the model value should be modified, and {nameof(this.RaisePropertyChanged)} should be called in the view-model");
            }
        }
    }
}