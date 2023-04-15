using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Render;
using FramePFX.Render.OGL;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Timeline.ViewModels.Clips;

namespace FramePFX.Timeline.Controls {
    public class TimelineClipContainerControl : ContentControl, IClipContainerHandle {
        #region Dependency Properties

        public static readonly DependencyProperty UnitZoomProperty =
            TimelineLayerControl.UnitZoomProperty.AddOwner(
                typeof(TimelineClipContainerControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineClipContainerControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelineClipContainerControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineClipContainerControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(TimelineClipContainerControl),
                new FrameworkPropertyMetadata(
                    0L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineClipContainerControl) d).OnFrameDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty FrameBeginDataOffsetProperty =
            DependencyProperty.Register(
                "FrameBeginDataOffset",
                typeof(int),
                typeof(TimelineClipContainerControl),
                new PropertyMetadata(0));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(TimelineClipContainerControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((TimelineClipContainerControl) d).OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty HeaderBrushProperty =
            DependencyProperty.Register(
                "HeaderBrush",
                typeof(Brush),
                typeof(TimelineClipContainerControl),
                new PropertyMetadata(null));

        #endregion

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(TimelineClipContainerControl));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(TimelineClipContainerControl));

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

        public FrameSpan Span {
            get => new FrameSpan(this.FrameBegin, this.FrameDuration);
            set {
                this.FrameBegin = value.Begin;
                this.FrameDuration = value.Duration;
            }
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
        public double PixelStart => this.FrameBegin * this.UnitZoom;

        public Selector ParentSelector => ItemsControl.ItemsControlFromItemContainer(this) as Selector;
        public TimelineLayerControl TimelineLayer => this.ParentSelector as TimelineLayerControl;

        public TimelineControl Timeline => this.ParentSelector is TimelineLayerControl layer ? layer.Timeline : null;

        public TimelineVideoClip ViewModel => this.DataContext as TimelineVideoClip;

        public bool IsMovingControl { get; set; }

        private bool isUpdatingUnitZoom;
        private bool isUpdatingFrameBegin;
        private bool isUpdatingFrameDuration;
        private Point lastLeftClickPoint;
        private Thumb PART_ThumbLeft;
        private Thumb PART_ThumbRight;
        // private ContentPresenter PART_Presenter;
        private OGLViewportControl PART_ViewPort;
        private bool isDraggingThumb;
        private bool isClipDragActivated;
        private bool isCancellingDragAction;

        private bool isProcessingMouseAction;

        public ClipDragData DragData { get; set; }

        public TimelineClipContainerControl() {
            this.Focusable = true;
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is TimelineVideoClip vm) {
                    vm.Handle = this;
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
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.PART_ThumbLeft = this.GetTemplateElement<Thumb>("PART_ThumbLeft");
            this.PART_ThumbRight = this.GetTemplateElement<Thumb>("PART_ThumbRight");
            this.PART_ThumbLeft.DragDelta += this.OnDragLeftThumb;
            this.PART_ThumbRight.DragDelta += this.OnDragRightThumb;
            // this.PART_Presenter = this.GetTemplateElement<ContentPresenter>("PART_Presenter");
            this.PART_ViewPort = this.GetTemplateElement<OGLViewportControl>("PART_ViewPort");
        }

        public double GetMouseDifference(double mouseX) {
            return Math.Abs(this.lastLeftClickPoint.X - mouseX);
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.TimelineLayer.MakeTopElement(this);
            this.lastLeftClickPoint = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            this.isProcessingMouseAction = true;
            if (!e.Handled) {
                if (this.IsFocused || this.Focus()) {
                    e.Handled = true;
                    this.TimelineLayer.OnClipMouseButton(this, e);
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
                this.TimelineLayer.OnClipMouseButton(this, e, hasDrag);
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
                                    this.Timeline.DragData.OnMouseMove(offset);
                                }
                            }

                            // force re-render view port. Without this code, if the playhead is at 0, then sometimes
                            // the clips won't be rendered if you very quickly drag the clip to the very start
                            IoC.VideoEditor.PlaybackView.RenderTimeline(this.Timeline.ViewModel);
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
                this.TimelineLayer.RemoveClip(this);
            }
        }

        private void OnDragLeftThumb(object sender, DragDeltaEventArgs e) {
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

        private void OnDragRightThumb(object sender, DragDeltaEventArgs e) {
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

        private void BeginDragMovement(/* Point mousePosition */) {
            if (this.Timeline.DragData != null) {
                return; // ?????
            }

            this.Timeline.BeginDragAction();
            // this.CaptureMouse();
            // this.Focus();
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom)
                return;

            TimelineUtils.ValidateNonNegative(newZoom);
            if (Math.Abs(oldZoom - newZoom) >= TimelineUtils.MinUnitZoom) {
                this.isUpdatingUnitZoom = true;
                this.UpdatePositionAndSize();
                this.isUpdatingUnitZoom = false;
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
            this.RealPixelX = this.PixelStart;
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

        private T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }

        public override string ToString() {
            return $"TimelineClipControl({this.Span})";
        }
    }
}
