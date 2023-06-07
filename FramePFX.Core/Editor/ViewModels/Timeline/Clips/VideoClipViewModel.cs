using System.Numerics;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    /// <summary>
    /// Base view model class for video clips that are placed on a video layer
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel {
        private readonly DelayedEnqueuement<HistoryClipMediaTransformation> transformationHistory = new DelayedEnqueuement<HistoryClipMediaTransformation>();
        private readonly DelayedEnqueuement<HistoryVideoClipPosition> clipSpanHistory = new DelayedEnqueuement<HistoryVideoClipPosition>();

        private float bothPos;
        private float bothScale;
        private float bothScaleOrigin;

        public new VideoClipModel Model => (VideoClipModel) base.Model;

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
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit media transformation");
                    action.MediaPosition.SetCurrent(value);
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
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit media transformation");
                    action.MediaScale.SetCurrent(value);
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
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit media transformation");
                    action.MediaScaleOrigin.SetCurrent(value);
                }

                this.Model.MediaScaleOrigin = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
                this.Model.InvalidateRender();
            }
        }

        /// <summary>
        /// The number of frames that are skipped relative to <see cref="ClipStart"/>. This will be positive if the
        /// left grip of the clip is dragged to the right, and will be 0 when dragged to the left
        /// <para>
        /// Alternative name: MediaBegin
        /// </para>
        /// </summary>
        public long MediaFrameOffset {
            get => this.Model.MediaFrameOffset;
            set {
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.clipSpanHistory.TryGetAction(out HistoryVideoClipPosition action))
                        this.clipSpanHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.MediaFrameOffset.SetCurrent(value);
                }

                this.Model.MediaFrameOffset = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public long FrameBegin {
            get => this.Span.Begin;
            set => this.Span = this.Span.SetBegin(value);
        }

        public long FrameDuration {
            get => this.Span.Duration;
            set => this.Span = this.Span.SetDuration(value);
        }

        public long FrameEndIndex {
            get => this.Span.EndIndex;
            set => this.Span = this.Span.SetEndIndex(value);
        }

        public ClipSpan Span {
            get => this.Model.FrameSpan;
            set {
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.clipSpanHistory.TryGetAction(out HistoryVideoClipPosition action))
                        this.clipSpanHistory.PushAction(this.HistoryManager, action = new HistoryVideoClipPosition(this), "Edit media pos/duration");
                    action.Span.SetCurrent(value);
                }

                this.Model.FrameSpan = value;
                this.RaisePropertyChanged(nameof(this.FrameBegin));
                this.RaisePropertyChanged(nameof(this.FrameDuration));
                this.RaisePropertyChanged(nameof(this.FrameEndIndex));
                this.Model.InvalidateRender();
            }
        }

        public float BothPos {
            get => this.bothPos;
            set {
                float actualValue = value - this.bothPos;
                this.MediaPosition += new Vector2(actualValue);
                this.RaisePropertyChanged();
                this.bothPos = value;
            }
        }

        public float BothScale {
            get => this.bothScale;
            set {
                float actualValue = value - this.bothScale;
                this.MediaScale += new Vector2(actualValue);
                this.RaisePropertyChanged();
                this.bothScale = value;
            }
        }

        public float BothScaleOrigin {
            get => this.bothScaleOrigin;
            set {
                float actualValue = value - this.bothScaleOrigin;
                this.MediaScaleOrigin += new Vector2(actualValue);
                this.RaisePropertyChanged();
                this.bothScaleOrigin = value;
            }
        }

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

        public virtual void OnInvalidateRender(bool schedule = true) {
            this.Layer?.Timeline.DoRender(schedule);
        }

        protected override void DisposeCore(ExceptionStack stack) {
            base.DisposeCore(stack);
            this.Model.RenderInvalidated -= this.renderCallback;
        }
    }
}