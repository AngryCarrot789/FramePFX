using System.Numerics;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    /// <summary>
    /// Base view model class for video clips that are placed on a video layer
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel {
        private readonly HistoryBuffer<HistoryClipMediaTransformation> transformationHistory = new HistoryBuffer<HistoryClipMediaTransformation>();
        private readonly HistoryBuffer<HistoryVideoClipOpacity> opacityHistory = new HistoryBuffer<HistoryVideoClipOpacity>();

        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;

        public new VideoClipModel Model => (VideoClipModel) base.Model;

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
                if (!this.IsAutomationChangeInProgress && !this.IsHistoryChanging && this.Layer != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaPosition.SetCurrent(value);
                }

                if (!this.IsAutomationChangeInProgress) {
                    this.AutomationData[VideoClipModel.MediaPositionKey].GetOverride().SetVector2Value(value);
                }

                this.Model.MediaPosition = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaPositionX));
                this.RaisePropertyChanged(nameof(this.MediaPositionY));
                this.Model.InvalidateRender();
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
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaScale.SetCurrent(value);
                }

                if (!this.IsAutomationChangeInProgress) {
                    this.AutomationData[VideoClipModel.MediaScaleKey].GetOverride().SetVector2Value(value);
                }

                this.Model.MediaScale = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleX));
                this.RaisePropertyChanged(nameof(this.MediaScaleY));
                this.Model.InvalidateRender();
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
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaScaleOrigin.SetCurrent(value);
                }

                if (!this.IsAutomationChangeInProgress) {
                    this.AutomationData[VideoClipModel.MediaScaleOriginKey].GetOverride().SetVector2Value(value);
                }

                this.Model.MediaScaleOrigin = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
                this.Model.InvalidateRender();
            }
        }

        public double Opacity {
            get => this.Model.Opacity;
            set {
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.opacityHistory.TryGetAction(out HistoryVideoClipOpacity action))
                        this.opacityHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipOpacity(this), "Edit opacity");
                    action.Opacity.SetCurrent(value);
                }

                if (!this.IsAutomationChangeInProgress) {
                    this.AutomationData[VideoClipModel.OpacityKey].GetOverride().SetDoubleValue(value);
                }

                this.Model.Opacity = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
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

        protected VideoClipViewModel(VideoClipModel model) : base(model) {
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
        }

        public override void RefreshAutomationValues(long frame) {
            base.RefreshAutomationValues(frame);
            this.RaisePropertyChanged(nameof(this.MediaPosition));
            this.RaisePropertyChanged(nameof(this.MediaPositionX));
            this.RaisePropertyChanged(nameof(this.MediaPositionY));
            this.RaisePropertyChanged(nameof(this.MediaScale));
            this.RaisePropertyChanged(nameof(this.MediaScaleX));
            this.RaisePropertyChanged(nameof(this.MediaScaleY));
            this.RaisePropertyChanged(nameof(this.MediaScaleOrigin));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
            this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
            this.RaisePropertyChanged(nameof(this.Opacity));
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
            this.Layer?.Timeline.DoRender(schedule);
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            this.Model.RenderInvalidated -= this.renderCallback;
        }
    }
}