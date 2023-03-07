using System;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Media;

namespace FramePFX.Timeline {
    public class TimelineElementControl : ContentControl {
        public static readonly DependencyProperty UnitZoomProperty =
            TimelineLayerControl.UnitZoomProperty.AddOwner(
                typeof(TimelineElementControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((TimelineElementControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnitZoom(v)));

        public static readonly DependencyProperty FrameOffsetProperty =
            TimelineLayerControl.FrameOffsetProperty.AddOwner(
                typeof(TimelineElementControl),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((TimelineElementControl) d).OnFrameOffsetChanged((double) e.OldValue, (double) e.NewValue)));

        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(int),
                typeof(TimelineElementControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((TimelineElementControl) d).OnFrameBeginChanged((int) e.OldValue, (int) e.NewValue),
                    (d, v) => (int) v < 0 ? 0 : v));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(int),
                typeof(TimelineElementControl),
                new FrameworkPropertyMetadata(
                    0,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((TimelineElementControl) d).OnFrameDurationChanged((int) e.OldValue, (int) e.NewValue),
                    (d, v) => (int) v < 0 ? 0 : v));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(TimelineElementControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((TimelineElementControl) d).OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty HeaderBrushProperty = DependencyProperty.Register("HeaderBrush", typeof(Brush), typeof(TimelineElementControl), new PropertyMetadata(null));

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(TimelineElementControl));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(TimelineElementControl));

        public event RoutedEventHandler Selected {
            add => this.AddHandler(SelectedEvent, value);
            remove => this.RemoveHandler(SelectedEvent, value);
        }

        public event RoutedEventHandler Unselected {
            add => this.AddHandler(UnselectedEvent, value);
            remove => this.RemoveHandler(UnselectedEvent, value);
        }

        /// <summary>
        /// The zoom level of this timeline layer
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        /// <summary>
        /// The number of frames this element's render is offset by
        /// <para>
        /// TODO: Remove this because the rendering offset should be done somewhere better! This just introduces more hassle and bugs
        /// </para>
        /// </summary>
        public double FrameOffset {
            get => (double) this.GetValue(FrameOffsetProperty);
            set => this.SetValue(FrameOffsetProperty, value);
        }

        /// <summary>
        /// This element's duration, in frames
        /// </summary>
        public int FrameDuration {
            get => (int) this.GetValue(FrameDurationProperty);
            set => this.SetValue(FrameDurationProperty, value);
        }

        /// <summary>
        /// The zero-based frame index where this element begins (relative to the parent timeline layer)
        /// </summary>
        public int FrameBegin {
            get => (int) this.GetValue(FrameBeginProperty);
            set => this.SetValue(FrameBeginProperty, value);
        }

        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        [Category("Appearance")]
        public Brush HeaderBrush {
            get => (Brush) this.GetValue(HeaderBrushProperty);
            set => this.SetValue(HeaderBrushProperty, value);
        }

        /// <summary>
        /// The rendered X position of this element
        /// </summary>
        public double RealPixelX {
            get => this.TimelineLayer?.GetRenderX(this) ?? 0;
            set => this.TimelineLayer?.SetRenderX(this, value);
        }

        /// <summary>
        /// The rendered Y position of this element (should not be modified generally)
        /// </summary>
        public double RealPixelY {
            get => this.TimelineLayer?.GetRenderY(this) ?? 0;
            set => this.TimelineLayer?.SetRenderY(this, value);
        }

        /// <summary>
        /// The calculated width of this element based on the frame duration and zoom
        /// </summary>
        public double PixelWidth => this.FrameDuration * this.UnitZoom;

        /// <summary>
        /// The calculated render X position of this element based on the start frame, frame offset and zoom
        /// </summary>
        public double PixelStartWithOffset => (this.FrameBegin + this.FrameOffset) * this.UnitZoom;

        /// <summary>
        /// The calculated render X position of this element based on the start frame and zoom
        /// </summary>
        public double PixelStartWithoutOffset => this.FrameBegin * this.UnitZoom;

        public TimelineLayerControl TimelineLayer { get; set; }

        public TimelineControl Timeline => this.TimelineLayer.Timeline;

        private bool isUpdatingUnitZoom;
        private bool isUpdatingFrameOffset;
        private bool isUpdatingFrameBegin;
        private bool isUpdatingFrameDuration;

        public TimelineElementMoveData moveDrag;

        private bool isMovingControl;
        private Point lastLeftClickPoint;

        public TimelineElementControl() {
            this.Focusable = true;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
        }

        public double GetMouseDifference(double mouseX) {
            return Math.Abs(this.lastLeftClickPoint.X - mouseX);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            this.lastLeftClickPoint = e.GetPosition(this);
            if (!e.Handled) {
                e.Handled = true;
                this.HandleMouseButton(e);
            }

            this.CaptureMouse();
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            if (this.IsMouseCaptured) {
                this.FinishCompletedDragMove();
                this.ReleaseMouseCapture();
            }

            if (!e.Handled) {
                e.Handled = true;
                this.HandleMouseButton(e);
            }

            base.OnMouseLeftButtonUp(e);
        }

        private void HandleMouseButton(MouseButtonEventArgs e) {
            if (!this.Focus()) {
                return;
            }

            this.TimelineLayer.HandleMouseClick(this, e);
        }

        private void BeginDragMovement(/* Point mousePosition */) {
            this.moveDrag = new TimelineElementMoveData(this) {
                FrameBegin = this.FrameBegin
                // SelectedItems = this.Timeline.GetSelectedElements()
            };

            // if (this.moveDrag.IsMultiMove) {
            //     foreach (TimelineElementControl element in this.moveDrag.SelectedItems) {
            //         if (element != this) {
            //             element.moveDrag = new TimelineElementMoveData(element) {
            //                 FrameBegin = element.FrameBegin,
            //                 Parent = this.moveDrag
            //             };
            //         };
            //     }
            // }

            // this.lastLeftClickPoint = mousePosition;
            this.CaptureMouse();
            this.Focus();
        }

        private void FinishCompletedDragMove() {
            if (this.moveDrag != null) {
                this.moveDrag.OnDragComplete();
                this.moveDrag = null;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e) {
            // if (this.moveDrag == null || this.moveDrag.IsMultiMove) {
            //     return;
            // }

            switch (e.Key) {
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    this.HandleDrag_CopyClipMode();
                    return;
                case Key.LeftShift:
                case Key.RightShift:
                    this.HandleDrag_MoveClipMode();
                    return;
                case Key.Escape:
                    this.HandleDrag_CancelDrag();
                    return;
            }
        }

        private void HandleDrag_CopyClipMode() {
            if (this.moveDrag.CopiedElement == null) {
                this.moveDrag.CopiedElement = this.TimelineLayer.CreateClonedElement(this);
                bool createCopyAtCursor = Keyboard.IsKeyDown(Key.Space);
                if (createCopyAtCursor) {
                    this.moveDrag.CopiedElement.FrameBegin = this.FrameBegin;
                    this.moveDrag.IsCopyDropAndMoveOriginal = true;
                }
                else {
                    this.moveDrag.CopiedElement.FrameBegin = this.moveDrag.FrameBegin;
                    this.moveDrag.IsCopyDropAndMoveOriginal = false;
                }

                this.moveDrag.IsCopyDropAndLeaveOriginal = true;
            }
        }

        private void HandleDrag_MoveClipMode() {
            this.moveDrag.DestroyCopiedClip();
            this.moveDrag.IsCopyDropAndLeaveOriginal = false;
        }

        private void HandleDrag_CancelDrag() {
            this.moveDrag.OnDragCancelled();
            this.moveDrag = null;
            this.ReleaseMouseCapture();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            if (this.isMovingControl) {
                return;
            }

            Point mousePoint = e.GetPosition(this);
            if (this.IsMouseCaptured && this.moveDrag != null) {
                if (Keyboard.IsKeyDown(Key.Escape)) {
                    this.HandleDrag_CancelDrag();
                    return;
                }

                if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                    this.HandleDrag_CopyClipMode();
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                    this.HandleDrag_MoveClipMode();
                }

                double difference = mousePoint.X - this.lastLeftClickPoint.X;
                if (Math.Abs(difference) >= 1.0d) {
                    this.isMovingControl = true;
                    this.FrameBegin += (int) (difference / this.UnitZoom);
                    this.isMovingControl = false;
                }
            }
            else if (this.moveDrag != null) {
                if (e.LeftButton == MouseButtonState.Released) {
                    return;
                }
                else {
                    this.moveDrag.OnDragCancelled();
                    this.moveDrag = null;
                }
            }
            else if (e.LeftButton == MouseButtonState.Pressed && this.IsSelected) {
                // handle "drag entry zone"
                if (this.GetMouseDifference(mousePoint.X) > 10d) {
                    this.BeginDragMovement();
                }
            }
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom)
                return;

            this.isUpdatingUnitZoom = true;
            if (Math.Abs(oldZoom - newZoom) >= TimelineUtils.MinUnitZoom)
                this.UpdatePositionAndSize();
            this.isUpdatingUnitZoom = false;
        }

        private void OnFrameOffsetChanged(double oldOffset, double newOffset) {
            if (this.isUpdatingFrameOffset)
                return;

            this.isUpdatingFrameOffset = true;
            if (Math.Abs(oldOffset - newOffset) >= TimelineUtils.MinUnitZoom)
                this.UpdatePosition();
            this.isUpdatingFrameOffset = false;
        }

        private void OnFrameBeginChanged(int oldStart, int newStart) {
            if (this.isUpdatingFrameBegin)
                return;
            this.isUpdatingFrameBegin = true;
            if (oldStart != newStart)
                this.UpdatePosition();
            this.isUpdatingFrameBegin = false;

            // this.Content = $"{this.FrameBegin},{this.FrameDuration}";
        }

        private void OnFrameDurationChanged(int oldDuration, int newDuration) {
            if (this.isUpdatingFrameDuration)
                return;
            this.isUpdatingFrameDuration = true;
            if (oldDuration != newDuration)
                this.UpdateSize();
            this.isUpdatingFrameDuration = false;

            // this.Content = $"{this.FrameBegin},{this.FrameDuration}";
        }

        private void OnIsSelectedChanged(bool oldSelected, bool newSelected) {
            if (newSelected) {
                this.OnSelected(new RoutedEventArgs(SelectedEvent, this));
            }
            else {
                this.OnUnselected(new RoutedEventArgs(SelectedEvent, this));
            }
        }

        public virtual void OnSelected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        public virtual void OnUnselected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        public void UpdatePositionAndSize() {
            this.UpdatePosition();
            this.UpdateSize();
        }

        public void UpdatePosition() {
            this.RealPixelX = this.PixelStartWithOffset;
        }

        public void UpdateSize() {
            this.Width = this.PixelWidth;
        }

        /// <summary>
        /// Called when the parent (aka the timeline layer)'s type changes from something to something else
        /// <para>
        /// This can be used to set an invalid state in this element if the new type is incompatible
        /// </para>
        /// </summary>
        /// <param name="oldType">The previous layer type</param>
        /// <param name="newType">The new layer type</param>
        public void OnLayerTypeChanged(string oldType, string newType) {

        }
    }
}
