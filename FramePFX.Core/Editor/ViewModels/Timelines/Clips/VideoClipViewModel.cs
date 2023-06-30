using System;
using System.Diagnostics;
using System.Numerics;
using FramePFX.Core.Automation;
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
                    this.AutomationData[VideoClip.MediaPositionKey].GetActiveKeyFrameOrCreateNew(this.Model.GetRelativeFrame(timeline.PlayHeadFrame)).SetVector2Value(value);
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
                    this.AutomationData[VideoClip.MediaScaleKey].GetActiveKeyFrameOrCreateNew(this.Model.GetRelativeFrame(timeline.PlayHeadFrame)).SetVector2Value(value);
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
                    this.AutomationData[VideoClip.MediaScaleOriginKey].GetActiveKeyFrameOrCreateNew(this.Model.GetRelativeFrame(timeline.PlayHeadFrame)).SetVector2Value(value);
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
                    this.AutomationData[VideoClip.OpacityKey].GetActiveKeyFrameOrCreateNew(this.Model.GetRelativeFrame(timeline.PlayHeadFrame)).SetDoubleValue(value);
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
        public RelayCommand ResetPositionCommand { get; }
        public RelayCommand ResetScaleCommand { get; }
        public RelayCommand ResetScaleOriginCommand { get; }

        private readonly ClipRenderInvalidatedEventHandler renderCallback;

        protected VideoClipViewModel(VideoClip model) : base(model) {
            this.ResetTransformationCommand = new RelayCommand(() => {
                this.MediaPosition = new Vector2(0f, 0f);
                this.MediaScale = new Vector2(1f, 1f);
                this.MediaScaleOrigin = new Vector2(0.5f, 0.5f);
            });

            this.ResetPositionCommand = new RelayCommand(() => this.MediaPosition = new Vector2(0f, 0f));
            this.ResetScaleCommand = new RelayCommand(() => this.MediaScale = new Vector2(1f, 1f));
            this.ResetScaleOriginCommand = new RelayCommand(() => this.MediaScaleOrigin = new Vector2(0.5f, 0.5f));

            this.renderCallback = (x, s) => {
                // assert ReferenceEquals(this.Model, x)
                this.OnInvalidateRender(s);
            };

            this.Model.RenderInvalidated += this.renderCallback;
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaPositionKey, (s, e) => {
                this.RaiseAutomationPropertyUpdated(nameof(this.MediaPosition), in e);
                this.RaisePropertyChanged(nameof(this.MediaPositionX));
                this.RaisePropertyChanged(nameof(this.MediaPositionY));
            });
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleKey, (s, e) => {
                this.RaiseAutomationPropertyUpdated(nameof(this.MediaScale), in e);
                this.RaisePropertyChanged(nameof(this.MediaScaleX));
                this.RaisePropertyChanged(nameof(this.MediaScaleY));
            });
            this.AutomationData.AssignRefreshHandler(VideoClip.MediaScaleOriginKey, (s, e) => {
                this.RaiseAutomationPropertyUpdated(nameof(this.MediaScaleOrigin), in e);
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
            });
            this.AutomationData.AssignRefreshHandler(VideoClip.OpacityKey, (s, e) => {
                this.RaiseAutomationPropertyUpdated(nameof(this.Opacity), in e);
            });
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

        protected override void RaiseAutomationPropertyUpdated(string propertyName, in RefreshAutomationValueEventArgs e) {
            base.RaiseAutomationPropertyUpdated(propertyName, in e);
            if (!e.IsDuringPlayback && !e.IsPlaybackTick) {
                this.Model.InvalidateRender(true);
                // this.Timeline.DoRender(true);
            }
        }

        [Conditional("DEBUG")]
        private void ValidateNotInAutomationChange() {
            if (this.IsAutomationRefreshInProgress) {
                Debugger.Break();
                throw new Exception("Cannot modify view-model parameter property while automation refresh is in progress. " +
                                    $"Only the model value should be modified, and {nameof(this.RaiseAutomationPropertyUpdated)} should be called in the view-model");
            }
        }
    }
}