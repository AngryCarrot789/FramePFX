using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelineVideoClipControl : BaseTimelineClipControl {
        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelineVideoClipControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineVideoClipControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(TimelineVideoClipControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineVideoClipControl) d).OnFrameDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty FrameBeginDataOffsetProperty =
            DependencyProperty.Register(
                "FrameBeginDataOffset",
                typeof(int),
                typeof(TimelineVideoClipControl),
                new PropertyMetadata(0));

        /// <summary>
        /// The zero-based frame index where this element begins (relative to the parent timeline layer)
        /// </summary>
        public long FrameBegin {
            get => (long) this.GetValue(FrameBeginProperty);
            set => this.SetValue(FrameBeginProperty, value);
        }

        /// <summary>
        /// This element's duration, in frames
        /// </summary>
        public long FrameDuration {
            get => (long) this.GetValue(FrameDurationProperty);
            set => this.SetValue(FrameDurationProperty, value);
        }

        /// <summary>
        /// A value that indicates this clip's content offset. Initially this is 0. This value increases when the clip's left thumb
        /// is dragged right, and decreases when the thumb is dragged left. Dragging the right thumb does not affect this value
        /// </summary>
        public int FrameBeginDataOffset {
            get => (int) this.GetValue(FrameBeginDataOffsetProperty);
            set => this.SetValue(FrameBeginDataOffsetProperty, value);
        }

        public long FrameEndIndex {
            get => this.FrameBegin + this.FrameDuration;
            set {
                long duration = value - this.FrameBegin;
                if (duration < 0) {
                    throw new ArgumentException($"FrameEndIndex cannot be below FrameBegin ({value} < {this.FrameBegin})");
                }

                this.FrameDuration = duration;
            }
        }

        public ClipSpan Span {
            get => new ClipSpan(this.FrameBegin, this.FrameDuration);
            set {
                this.FrameBegin = value.Begin;
                this.FrameDuration = value.Duration;
            }
        }

        /// <summary>
        /// The rendered X position of this element
        /// </summary>
        public double RealPixelX {
            get => base.Layer?.GetRenderX(this) ?? 0;
            set => base.Layer?.SetRenderX(this, value);
        }

        /// <summary>
        /// The calculated width of this element based on the frame duration and zoom
        /// </summary>
        public double PixelWidth => this.FrameDuration * this.UnitZoom;

        /// <summary>
        /// The calculated render X position of this element based on the start frame, frame offset and zoom
        /// </summary>
        public double PixelStart => this.FrameBegin * this.UnitZoom;

        public new VideoTimelineLayerControl Layer => this.ParentSelector as VideoTimelineLayerControl;

        public VideoClipViewModel ViewModel => this.DataContext as VideoClipViewModel;

        public bool IsMovingControl { get; set; }

        private bool isUpdatingFrameBegin;
        private bool isUpdatingFrameDuration;

        public ClipDragData DragData { get; set; }

        public TimelineVideoClipControl() {
            this.Focusable = true;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is VideoClipViewModel vm) {
                    BaseViewModel.SetInternalData(vm, typeof(IClipHandle), this);
                }
            };

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            // if (this.PART_Presenter != null && VisualTreeHelper.GetChildrenCount(this.PART_Presenter) > 0) {
            //     this.generatedChild = VisualTreeHelper.GetChild(this.PART_Presenter, 0) as BaseClipControl;
            //     if (this.generatedChild is IClipHandle h2) {
            //         this.ClipContent = h2;
            //     }
            // }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
        }

        public double GetMouseDifference(double mouseX) {
            return Math.Abs(this.lastLeftClickPoint.X - mouseX);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.Layer.MakeTopElement(this);
            this.lastLeftClickPoint = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            this.isProcessingMouseAction = true;
            if (!e.Handled) {
                if (this.IsFocused || this.Focus()) {
                    e.Handled = true;
                    this.Layer.OnClipMouseButton(this, e);
                }

                this.isClipDragActivated = true;
            }

            base.OnMouseLeftButtonDown(e);
            this.isProcessingMouseAction = false;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            this.isProcessingMouseAction = true;
            this.isClipDragActivated = false;
            if (this.IsMouseCaptured) {
                this.ReleaseMouseCapture();
            }

            bool hasDrag = false;
            if (this.Timeline.HasActiveDrag()) {
                this.Timeline.DragData.OnCompleted();
                this.Timeline.DragData = null;
                hasDrag = true;
            }

            if (!e.Handled) {
                e.Handled = true;
                this.Layer.OnClipMouseButton(this, e, hasDrag);
            }

            base.OnMouseLeftButtonUp(e);
            this.isProcessingMouseAction = false;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (this.IsMovingControl || this.isProcessingMouseAction || this.isCancellingDragAction) {
                return;
            }

            if (!this.isClipDragActivated) {
                return;
            }

            if (this.PART_ThumbLeft.IsDragging || this.PART_ThumbRight.IsDragging) {
                return;
            }

            Point mousePoint = e.GetPosition(this);
            if (this.Timeline.HasActiveDrag()) {
                if (this.Timeline.DragData.IsBeingDragged(this)) {
                    if (this.IsMouseCaptured || this.CaptureMouse()) {
                        if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                            this.Timeline.DragData.OnEnterCopyMove();
                        }
                        else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                            this.Timeline.DragData.OnEnterMoveMode();
                        }

                        this.isProcessingMouseAction = true;
                        // TODO: somehow implement cross-layer drag drop...
                        double diffX = mousePoint.X - this.lastLeftClickPoint.X;
                        if (Math.Abs(diffX) >= 1.0d) {
                            long offset = (long) (diffX / this.UnitZoom);
                            if (offset != 0) {
                                if ((this.FrameBegin + offset) < 0) {
                                    offset = -this.FrameBegin;
                                }

                                if (offset != 0) {
                                    // causes a re-render
                                    this.Timeline.DragData.OnMouseMove(offset);
                                }
                            }
                        }

                        this.isProcessingMouseAction = false;
                    }
                }
                else if (this.DragData != null) {
                    throw new Exception("????????????????????????????????????");
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed) {
                // handle "drag entry zone"
                if (this.GetMouseDifference(mousePoint.X) > 5d) {
                    this.BeginDragMovement();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.Escape) {
                this.isCancellingDragAction = true;
                this.isClipDragActivated = false;
                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.isCancellingDragAction = false;
                if (this.Timeline.HasActiveDrag() && this.Timeline.DragData.IsBeingDragged(this)) {
                    this.Timeline.DragData.OnCancel();
                    this.Timeline.DragData = null;
                    e.Handled = true;
                }

                base.OnKeyDown(e);
            }
            else if (e.Key == Key.Delete && this.IsSelected) {
                this.Layer.RemoveClip(this);
            }
        }

        protected override void OnDragLeftThumb(object sender, DragDeltaEventArgs e) {
            if (this.isDraggingThumb) {
                return;
            }

            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0) {
                return;
            }

            long begin = this.FrameBegin + change;
            if (begin < 0) {
                return;
            }

            long duration = this.FrameDuration - change;
            if (duration < 1) {
                return;
            }

            try {
                this.isDraggingThumb = true;
                this.FrameBegin = begin;
                this.FrameDuration = duration;
            }
            finally {
                this.isDraggingThumb = false;
            }
        }

        protected override void OnDragRightThumb(object sender, DragDeltaEventArgs e) {
            if (this.isDraggingThumb) {
                return;
            }

            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0) {
                return;
            }

            long duration = this.FrameDuration + change;
            if (duration < 1) {
                return;
            }

            try {
                this.isDraggingThumb = true;
                this.FrameDuration = duration;
            }
            finally {
                this.isDraggingThumb = false;
            }
        }

        private void OnFrameBeginChanged(long oldStart, long newStart) {
            if (this.isUpdatingFrameBegin)
                return;

            TimelineUtils.ValidateNonNegative(newStart);
            if (oldStart != newStart) {
                this.isUpdatingFrameBegin = true;
                this.UpdatePosition();
                this.isUpdatingFrameBegin = false;
            }
        }

        private void OnFrameDurationChanged(long oldDuration, long newDuration) {
            if (this.isUpdatingFrameDuration)
                return;

            TimelineUtils.ValidateNonNegative(newDuration);
            if (oldDuration != newDuration) {
                this.isUpdatingFrameDuration = true;
                this.UpdateSize();
                this.isUpdatingFrameDuration = false;
            }
        }

        public override void UpdatePosition() {
            this.RealPixelX = this.PixelStart;
        }

        public override void UpdateSize() {
            this.Width = this.PixelWidth;
        }

        public override string ToString() {
            return $"TimelineClipControl({this.Span})";
        }
    }
}
