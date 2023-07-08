using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Core.Annotations;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    /// <summary>
    /// Base view model class for video clips that are placed on a video track
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel {
        private readonly HistoryBuffer<HistoryClipMediaTransformation> transformationHistory = new HistoryBuffer<HistoryClipMediaTransformation>();
        private readonly HistoryBuffer<HistoryVideoClipOpacity> opacityHistory = new HistoryBuffer<HistoryVideoClipOpacity>();

        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;

        public new VideoClip Model => (VideoClip) base.Model;

        #region Media/Visual properties

        public float MediaPositionX {
            get => this.MediaPosition.X;
            set => this.MediaPosition = new Vector2(value, this.MediaPosition.Y);
        }

        public float MediaPositionY {
            get => this.MediaPosition.Y;
            set => this.MediaPosition = new Vector2(this.MediaPosition.X, value);
        }

        /// <summary>
        /// The x and y coordinates of the video's media
        /// </summary>
        public Vector2 MediaPosition {
            get => this.Model.MediaPosition;
            set {
                this.ValidateNotInAutomationChange();
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaPosition.SetCurrent(value);
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaPositionKey)) {
                    this.AutomationData[VideoClip.MediaPositionKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[VideoClip.MediaPositionKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaPositionKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleX {
            get => this.MediaScale.X;
            set => this.MediaScale = new Vector2(value, this.MediaScale.Y);
        }

        public float MediaScaleY {
            get => this.MediaScale.Y;
            set => this.MediaScale = new Vector2(this.MediaScale.X, value);
        }

        /// <summary>
        /// The x and y scale of the video's media (relative to <see cref="MediaScaleOrigin"/>)
        /// </summary>
        public Vector2 MediaScale {
            get => this.Model.MediaScale;
            set {
                this.ValidateNotInAutomationChange();
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaScale.SetCurrent(value);
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaScaleKey)) {
                    this.AutomationData[VideoClip.MediaScaleKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[VideoClip.MediaScaleKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaScaleKey].RaiseOverrideValueChanged();
                }
            }
        }

        public float MediaScaleOriginX {
            get => this.MediaScaleOrigin.X;
            set => this.MediaScaleOrigin = new Vector2(value, this.MediaScaleOrigin.Y);
        }

        public float MediaScaleOriginY {
            get => this.MediaScaleOrigin.Y;
            set => this.MediaScaleOrigin = new Vector2(this.MediaScaleOrigin.X, value);
        }

        /// <summary>
        /// The scaling origin point of this video's media. Default value is 0.5,0.5 (the center of the frame)
        /// </summary>
        public Vector2 MediaScaleOrigin {
            get => this.Model.MediaScaleOrigin;
            set {
                this.ValidateNotInAutomationChange();
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaScaleOrigin.SetCurrent(value);
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.MediaScaleOriginKey)) {
                    this.AutomationData[VideoClip.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetVector2Value(value);
                }
                else {
                    this.AutomationData[VideoClip.MediaScaleOriginKey].GetOverride().SetVector2Value(value);
                    this.AutomationData[VideoClip.MediaScaleOriginKey].RaiseOverrideValueChanged();
                }
            }
        }

        public double Opacity {
            get => this.Model.Opacity;
            set {
                this.ValidateNotInAutomationChange();
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.opacityHistory.TryGetAction(out HistoryVideoClipOpacity action))
                        this.opacityHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipOpacity(this), "Edit opacity");
                    action.Opacity.SetCurrent(value);
                }

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, VideoClip.OpacityKey)) {
                    this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame - this.FrameBegin).SetDoubleValue(value);
                }
                else {
                    this.AutomationData[VideoClip.OpacityKey].GetOverride().SetDoubleValue(value);
                    this.AutomationData[VideoClip.OpacityKey].RaiseOverrideValueChanged();
                }
            }
        }

        #endregion

        #region WPF NumberDragger helpers

        public float BothPos {
            get => this.bothPos;
            set {
                this.MediaPosition += new Vector2(value - this.bothPos);
                this.RaisePropertyChanged();
                this.bothPos = 0;
            }
        }

        public float BothScale {
            get => this.bothScale;
            set {
                this.MediaScale += new Vector2(value - this.bothScale);
                this.RaisePropertyChanged();
                this.bothScale = 0;
            }
        }

        public float BothScaleOrigin {
            get => this.bothScaleOrigin;
            set {
                this.MediaScaleOrigin += new Vector2(value - this.bothScaleOrigin);
                this.RaisePropertyChanged();
                this.bothScaleOrigin = 0;
            }
        }

        #endregion

        public RelayCommand ResetTransformationCommand { get; }
        public RelayCommand ResetMediaPositionCommand { get; }
        public RelayCommand ResetMediaScaleCommand { get; }
        public RelayCommand ResetMediaScaleOriginCommand { get; }
        public RelayCommand ResetOpacityCommand { get; }

        public RelayCommand InsertMediaPositionKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleKeyFrameCommand { get; }
        public RelayCommand InsertMediaScaleOriginKeyFrameCommand { get; }
        public RelayCommand InsertOpacityKeyFrameCommand { get; }

        public RelayCommand ToggleMediaPositionActiveCommand { get; }
        public RelayCommand ToggleMediaScaleActiveCommand { get; }
        public RelayCommand ToggleMediaScaleOriginActiveCommand { get; }
        public RelayCommand ToggleOpacityActiveCommand { get; }

        // binding helpers
        public AutomationSequenceViewModel MediaPositionAutomationSequence => this.AutomationData[VideoClip.MediaPositionKey];
        public AutomationSequenceViewModel MediaScaleAutomationSequence => this.AutomationData[VideoClip.MediaScaleKey];
        public AutomationSequenceViewModel MediaScaleOriginAutomationSequence => this.AutomationData[VideoClip.MediaScaleOriginKey];
        public AutomationSequenceViewModel OpacityAutomationSequence => this.AutomationData[VideoClip.OpacityKey];

        private readonly ClipRenderInvalidatedEventHandler renderCallback;

        #region Cached refresh event handlers

        private static readonly RefreshAutomationValueEventHandler RefreshMediaPositionHandler = (s, e) => {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaPosition));
            clip.RaisePropertyChanged(nameof(clip.MediaPositionX));
            clip.RaisePropertyChanged(nameof(clip.MediaPositionY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleHandler = (s, e) => {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScale));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshMediaScaleOriginHandler = (s, e) => {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOrigin));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginX));
            clip.RaisePropertyChanged(nameof(clip.MediaScaleOriginY));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        private static readonly RefreshAutomationValueEventHandler RefreshOpacityHandler = (s, e) => {
            VideoClipViewModel clip = (VideoClipViewModel) s.AutomationData.Owner;
            clip.RaisePropertyChanged(nameof(clip.Opacity));
            clip.InvalidateRenderForAutomationRefresh(in e);
        };

        #endregion

        protected VideoClipViewModel(VideoClip model) : base(model) {
            this.ResetTransformationCommand = new RelayCommand(() => {
                this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue;
                this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue;
                this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue;
            });

            this.ResetMediaPositionCommand =    new RelayCommand(() => this.MediaPosition = VideoClip.MediaPositionKey.Descriptor.DefaultValue);
            this.ResetMediaScaleCommand =       new RelayCommand(() => this.MediaScale = VideoClip.MediaScaleKey.Descriptor.DefaultValue);
            this.ResetMediaScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = VideoClip.MediaScaleOriginKey.Descriptor.DefaultValue);
            this.ResetOpacityCommand =          new RelayCommand(() => this.Opacity = VideoClip.OpacityKey.Descriptor.DefaultValue);

            Func<bool> canInsertKeyFrame = () => this.Track != null && this.Model.GetRelativeFrame(this.Timeline.PlayHeadFrame, out long _);
            this.InsertMediaPositionKeyFrameCommand =    new RelayCommand(() => this.AutomationData[VideoClip.MediaPositionKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaPosition), canInsertKeyFrame);
            this.InsertMediaScaleKeyFrameCommand =       new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetVector2Value(this.MediaScale), canInsertKeyFrame);
            this.InsertMediaScaleOriginKeyFrameCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.RelativePlayHead).SetVector2Value(this.MediaScaleOrigin), canInsertKeyFrame);
            this.InsertOpacityKeyFrameCommand =          new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(Math.Max(this.RelativePlayHead, 0)).SetDoubleValue(this.Opacity), canInsertKeyFrame);

            this.ToggleMediaPositionActiveCommand =    new RelayCommand(() => this.AutomationData[VideoClip.MediaPositionKey].ToggleOverrideAction());
            this.ToggleMediaScaleActiveCommand =       new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleKey].ToggleOverrideAction());
            this.ToggleMediaScaleOriginActiveCommand = new RelayCommand(() => this.AutomationData[VideoClip.MediaScaleOriginKey].ToggleOverrideAction());
            this.ToggleOpacityActiveCommand =          new RelayCommand(() => this.AutomationData[VideoClip.OpacityKey].ToggleOverrideAction());

            this.renderCallback = (x, s) => {
                // assert ReferenceEquals(this.Model, x)
                this.OnInvalidateRender(s);
            };

            this.Model.RenderInvalidated += this.renderCallback;
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaPositionKey, RefreshMediaPositionHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleKey, RefreshMediaScaleHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleOriginKey, RefreshMediaScaleOriginHandler);
            this.AutomationData.AssignRefreshHandler(VideoClip.OpacityKey, RefreshOpacityHandler);
        }

        // this is messy asf but it works :DDD

        protected override void OnFrameSpanChanged(FrameSpan oldSpan, FrameSpan newSpan) {
            base.OnFrameSpanChanged(oldSpan, newSpan);
            this.Model.InvalidateRender();
        }

        protected override void OnMediaFrameOffsetChanged(long oldFrame, long newFrame) {
            base.OnMediaFrameOffsetChanged(oldFrame, newFrame);
            this.Model.InvalidateRender();
        }

        public virtual void OnInvalidateRender(bool schedule = true) {
            this.Track?.Timeline.DoRender(schedule);
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            this.Model.RenderInvalidated -= this.renderCallback;
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