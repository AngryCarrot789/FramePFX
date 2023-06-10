using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.History;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    /// <summary>
    /// Base view model class for video clips that are placed on a video layer
    /// </summary>
    public abstract class VideoClipViewModel : ClipViewModel {
        private readonly HistoryBuffer<HistoryClipMediaTransformation> transformationHistory = new HistoryBuffer<HistoryClipMediaTransformation>();
        private HistoryVideoClipPosition lastDragHistoryAction;

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
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.transformationHistory.TryGetAction(out HistoryClipMediaTransformation action))
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
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
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
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
                        this.transformationHistory.PushAction(this.HistoryManager, action = new HistoryClipMediaTransformation(this), "Edit transformation");
                    action.MediaScaleOrigin.SetCurrent(value);
                }

                this.Model.MediaScaleOrigin = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginX));
                this.RaisePropertyChanged(nameof(this.MediaScaleOriginY));
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

        public override void OnLeftThumbDragStart() {
            base.OnLeftThumbDragStart();
            this.CreateDragHistoryAction();
        }

        public override void OnLeftThumbDragStop(bool cancelled) {
            base.OnLeftThumbDragStop(cancelled);
            this.PushDragHistoryAction(cancelled);
        }

        public override void OnRightThumbDragStart() {
            base.OnRightThumbDragStart();
            this.CreateDragHistoryAction();
        }

        public override void OnRightThumbDragStop(bool cancelled) {
            base.OnRightThumbDragStop(cancelled);
            this.PushDragHistoryAction(cancelled);
        }

        public override void OnDragStart() {
            base.OnDragStart();
            this.CreateDragHistoryAction();
            if (this.Timeline is TimelineViewModel timeline) {
                if (timeline.IsGloballyDragging) {
                    return;
                }

                List<ClipViewModel> selected = timeline.GetSelectedClips().ToList();
                if (selected.Count > 1) {
                    timeline.IsGloballyDragging = true;
                    timeline.DraggingClips = selected;
                    timeline.ProcessingDragEventClip = this;
                    foreach (ClipViewModel clip in selected.Where(clip => clip != this)) {
                        clip.OnDragStart();
                    }
                    timeline.ProcessingDragEventClip = null;
                }
            }
        }

        public override void OnDragStop(bool cancelled) {
            base.OnDragStop(cancelled);
            if (this.Timeline is TimelineViewModel timeline && timeline.IsGloballyDragging) {
                if (timeline.ProcessingDragEventClip == null) {
                    timeline.DragStopHistoryList = new List<HistoryVideoClipPosition>();
                }

                if (cancelled) {
                    this.lastDragHistoryAction.Undo();
                }
                else {
                    timeline.DragStopHistoryList.Add(this.lastDragHistoryAction);
                    this.lastDragHistoryAction = null;
                }

                if (timeline.ProcessingDragEventClip != null) {
                    return;
                }

                timeline.ProcessingDragEventClip = this;
                foreach (ClipViewModel clip in timeline.DraggingClips.Where(clip => this != clip)) {
                    clip.OnDragStop(cancelled);
                }
                timeline.IsGloballyDragging = false;
                timeline.ProcessingDragEventClip = null;
                timeline.DraggingClips = null;
                timeline.Project.Editor.HistoryManager.AddAction(new MultiHistoryAction(new List<IHistoryAction>(timeline.DragStopHistoryList)));
                timeline.DragStopHistoryList = null;
            }
            else {
                this.PushDragHistoryAction(cancelled);
            }
        }

        private long addedOffset;

        public override void OnLeftThumbDelta(long offset) {
            base.OnLeftThumbDelta(offset);
            if (!(this.Timeline is TimelineViewModel timeline)) {
                return;
            }

            long begin = this.FrameBegin + offset;
            if (begin < 0) {
                offset += -begin;
                begin = 0;
            }

            long duration = this.FrameDuration - offset;
            if (duration < 1) {
                begin += (duration - 1);
                duration = 1;
                if (begin < 0) {
                    return;
                }
            }

            this.FrameSpan = new FrameSpan(begin, duration);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public override void OnRightThumbDelta(long offset) {
            base.OnRightThumbDelta(offset);
            if (this.Layer == null) {
                return;
            }

            FrameSpan span = this.FrameSpan;
            long newEndIndex = Math.Max(span.EndIndex + offset, span.Begin + 1);
            if (newEndIndex > this.Timeline.MaxDuration) {
                this.Timeline.MaxDuration = newEndIndex + 300;
            }

            this.FrameSpan = span.SetEndIndex(newEndIndex);
            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        public override void OnDragDelta(long offset) {
            base.OnDragDelta(offset);
            if (!(this.Timeline is TimelineViewModel timeline)) {
                return;
            }

            FrameSpan span = this.FrameSpan;
            long begin = (span.Begin + offset) - this.addedOffset;
            this.addedOffset = 0L;
            if (begin < 0) {
                this.addedOffset = -begin;
                begin = 0;
            }

            long endIndex = begin + span.Duration;
            if (endIndex > timeline.MaxDuration) {
                timeline.MaxDuration = endIndex + 300;
            }

            this.FrameSpan = new FrameSpan(begin, span.Duration);

            if (timeline.IsGloballyDragging) {
                if (timeline.ProcessingDragEventClip == null) {
                    timeline.ProcessingDragEventClip = this;
                    foreach (ClipViewModel clip in timeline.DraggingClips.Where(clip => this != clip)) {
                        clip.OnDragDelta(offset);
                    }
                    timeline.ProcessingDragEventClip = null;
                }
            }

            this.lastDragHistoryAction.Span.SetCurrent(this.FrameSpan);
        }

        private void CreateDragHistoryAction() {
            if (this.lastDragHistoryAction != null) {
                throw new Exception("Drag history was non-null, which means a drag was started before another drag was completed");
            }

            this.lastDragHistoryAction = new HistoryVideoClipPosition(this);
        }

        private void PushDragHistoryAction(bool cancelled) {
            // throws if this.lastDragHistoryAction is null. It should not be null if there's no bugs in the drag start/end calls
            if (cancelled) {
                this.lastDragHistoryAction.Undo();
            }
            else {
                this.HistoryManager?.AddAction(this.lastDragHistoryAction, "Drag clip");
            }

            this.lastDragHistoryAction = null;
        }
    }
}