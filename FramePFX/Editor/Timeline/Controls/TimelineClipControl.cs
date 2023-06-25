using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public abstract class TimelineClipControl : Control {
        private static readonly object LongZeroObject = 0L;

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => {
                        if ((bool) e.NewValue) {
                            ((TimelineClipControl) d).OnSelected(new RoutedEventArgs(SelectedEvent, d));
                        }
                        else {
                            ((TimelineClipControl) d).OnUnselected(new RoutedEventArgs(SelectedEvent, d));
                        }
                    }));

        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    LongZeroObject,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineClipControl) d).OnFrameBeginChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? LongZeroObject : v));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    LongZeroObject,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineClipControl) d).OnFrameDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? LongZeroObject : v));

        public static readonly DependencyProperty IsDroppableTargetOverProperty =
            DependencyProperty.Register(
                "IsDroppableTargetOver",
                typeof(bool),
                typeof(TimelineClipControl),
                new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty HeaderBrushProperty =
            DependencyProperty.Register(
                "HeaderBrush",
                typeof(Brush),
                typeof(TimelineClipControl),
                new PropertyMetadata(null));

        /// <summary>
        /// The zoom level of this timeline track
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom => this.Track?.UnitZoom ?? 1d;

        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// The zero-based frame index where this element begins (relative to the parent timeline track)
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

        public FrameSpan Span {
            get => new FrameSpan(this.FrameBegin, this.FrameDuration);
            set {
                this.FrameBegin = value.Begin;
                this.FrameDuration = value.Duration;
            }
        }

        /// <summary>
        /// The calculated width of this element based on the frame duration and zoom
        /// </summary>
        public double PixelWidth => this.FrameDuration * this.UnitZoom;

        /// <summary>
        /// The calculated render X position of this element based on the start frame, frame offset and zoom
        /// </summary>
        public double PixelStart => this.FrameBegin * this.UnitZoom;

        [Category("Appearance")]
        public Brush HeaderBrush {
            get => (Brush) this.GetValue(HeaderBrushProperty);
            set => this.SetValue(HeaderBrushProperty, value);
        }

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value);
        }

        public event RoutedEventHandler Selected {
            add => this.AddHandler(SelectedEvent, value);
            remove => this.RemoveHandler(SelectedEvent, value);
        }

        public event RoutedEventHandler Unselected {
            add => this.AddHandler(UnselectedEvent, value);
            remove => this.RemoveHandler(UnselectedEvent, value);
        }

        public TimelineTrackControl Track => ItemsControl.ItemsControlFromItemContainer(this) as TimelineTrackControl;

        public TimelineControl Timeline => this.Track?.Timeline;

        public IClipDragHandler DragHandler => this.DataContext as IClipDragHandler;

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(TimelineClipControl));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(TimelineClipControl));

        protected bool isUpdatingUnitZoom;
        protected Point? lastLeftClickPoint;
        protected Thumb PART_ThumbLeft;
        protected Thumb PART_ThumbRight;
        protected bool isProcessingLeftThumbDrag;
        protected bool isProcessingRightThumbDrag;
        protected bool isProcessingMouseMove;
        private bool isUpdatingFrameBegin;
        private bool isUpdatingFrameDuration;
        private bool isProcessingAsyncDrop;
        private bool isClipDragRunning;

        private bool isLoadedWithActiveDrag;

        protected TimelineClipControl() {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.Focusable = true;
            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.Loaded+= this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (this.DragHandler is IClipDragHandler handler) {
                if (handler.IsDraggingClip) {
                    this.isLoadedWithActiveDrag = true;
                    this.Focus();
                    this.CaptureMouse();
                }
            }
        }

        static TimelineClipControl() {
            EventManager.RegisterClassHandler(typeof(TimelineClipControl), Mouse.LostMouseCaptureEvent, new MouseEventHandler((sender, e) => {
                TimelineClipControl thumb = (TimelineClipControl) sender;
                if (ReferenceEquals(Mouse.Captured, thumb))
                    return;
                //thumb.CancelDrag();
            }));
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.PART_ThumbLeft = this.GetTemplateElement<Thumb>("PART_ThumbLeft");
            this.PART_ThumbRight = this.GetTemplateElement<Thumb>("PART_ThumbRight");
            this.PART_ThumbLeft.DragStarted += this.OnLeftThumbDragStart;
            this.PART_ThumbLeft.DragDelta += this.OnLeftThumbDragDelta;
            this.PART_ThumbLeft.DragCompleted += this.OnLeftThumbDragComplete;
            this.PART_ThumbRight.DragStarted += this.OnRightThumbDragStart;
            this.PART_ThumbRight.DragDelta += this.OnRightThumbDragDelta;
            this.PART_ThumbRight.DragCompleted += this.OnRightThumbDragComplete;
        }

        protected override Size MeasureOverride(Size constraint) {
            Size size = new Size(this.FrameDuration * this.UnitZoom, constraint.Height);
            if (this.VisualChildrenCount > 0) {
                UIElement visualChild = (UIElement) this.GetVisualChild(0);
                visualChild?.Measure(size);
            }

            return size;
        }

        private void OnLeftThumbDragStart(object sender, DragStartedEventArgs e) {
            if (this.DragHandler is IClipDragHandler handler && !handler.IsDraggingLeftThumb) {
                handler.OnLeftThumbDragStart();
            }
        }

        private void OnLeftThumbDragDelta(object sender, DragDeltaEventArgs e) {
            if (this.isProcessingLeftThumbDrag)
                return;
            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0 || !(this.DragHandler is IClipDragHandler handler))
                return;
            if (!handler.IsDraggingLeftThumb)
                return;
            try {
                this.isProcessingLeftThumbDrag = true;
                handler.OnLeftThumbDelta(change);
            }
            finally {
                this.isProcessingLeftThumbDrag = false;
            }
        }

        private void OnLeftThumbDragComplete(object sender, DragCompletedEventArgs e) {
            if (this.DragHandler is IClipDragHandler handler && handler.IsDraggingLeftThumb) {
                handler.OnLeftThumbDragStop(e.Canceled);
            }
        }

        private void OnRightThumbDragStart(object sender, DragStartedEventArgs e) {
            if (this.DragHandler is IClipDragHandler handler && !handler.IsDraggingRightThumb) {
                handler.OnRightThumbDragStart();
            }
        }

        private void OnRightThumbDragDelta(object sender, DragDeltaEventArgs e) {
            if (this.isProcessingRightThumbDrag)
                return;
            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0 || !(this.DragHandler is IClipDragHandler handler))
                return;
            if (!handler.IsDraggingRightThumb)
                return;
            try {
                this.isProcessingRightThumbDrag = true;
                handler.OnRightThumbDelta(change);
            }
            finally {
                this.isProcessingRightThumbDrag = false;
            }
        }

        private void OnRightThumbDragComplete(object sender, DragCompletedEventArgs e) {
            if (this.DragHandler is IClipDragHandler handler && handler.IsDraggingRightThumb) {
                handler.OnRightThumbDragStop(e.Canceled);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.Track?.MakeTopElement(this);
            // this.lastLeftClickPoint = e.GetPosition(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (e.Handled) {
                return;
            }

            if (this.DragHandler is IClipDragHandler handler && !handler.IsDraggingClip) {
                if (this.IsFocused || this.Focus()) {
                    if (!this.IsMouseCaptured) {
                        this.CaptureMouse();
                    }

                    this.lastLeftClickPoint = e.GetPosition(this);

                    if (InputUtils.AreModifiersPressed(ModifierKeys.Control)) {
                        this.Track.SetItemSelectedProperty(this, true);
                    }
                    else if (InputUtils.AreModifiersPressed(ModifierKeys.Shift) && this.Track.lastSelectedItem != null && this.Track.SelectedItems.Count > 0) {
                        this.Track.MakeRangedSelection(this.Track.lastSelectedItem, this);
                    }
                    else if (this.Track.Timeline.GetSelectedClipContainers().ToList().Count > 1) {
                        if (!this.IsSelected) {
                            this.Track.Timeline.SetPrimarySelection(this);
                        }
                    }
                    else {
                        this.Track.Timeline.SetPrimarySelection(this);
                    }

                    e.Handled = true;
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            this.lastLeftClickPoint = null;
            this.isClipDragRunning = false;
            bool? wasDragging = null;
            if (this.DragHandler is IClipDragHandler handler && this.IsMouseCaptured && handler.IsDraggingClip) {
                wasDragging = true;
                handler.OnDragStop(false);
                this.ReleaseMouseCapture();
                e.Handled = true;
            }

            if (wasDragging != true && this.IsSelected && !InputUtils.AreModifiersPressed(ModifierKeys.Control) && !InputUtils.AreModifiersPressed(ModifierKeys.Shift) && this.Track.Timeline.GetSelectedClipContainers().ToList().Count > 1) {
                this.Track.Timeline.SetPrimarySelection(this);
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.isProcessingMouseMove || (this.PART_ThumbLeft?.IsDragging ?? true) || (this.PART_ThumbRight?.IsDragging ?? true)) {
                return;
            }

            if (!(this.DragHandler is IClipDragHandler handler)) {
                return;
            }

            bool didJustDragTrack = false;
            if (this.isLoadedWithActiveDrag) {
                this.isLoadedWithActiveDrag = false;
                didJustDragTrack = true;
                if (handler.IsDraggingClip) {
                    this.lastLeftClickPoint = this.Timeline.ClipMousePosForTrackTransition;
                    this.isClipDragRunning = true;
                }
            }

            if (e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
                if (ReferenceEquals(e.MouseDevice.Captured, this))
                    this.ReleaseMouseCapture();
                this.isClipDragRunning = false;
                this.lastLeftClickPoint = null;
                if (handler.IsDraggingClip) {
                    handler.OnDragStop(false);
                }
            }
            else {
                Point mousePoint = e.GetPosition(this);
                if (!(this.lastLeftClickPoint is Point lastClickPoint)) {
                    return;
                }

                if (!this.isClipDragRunning) {
                    const double range = 8d;
                    if (Math.Abs(lastClickPoint.X - mousePoint.X) < range && Math.Abs(lastClickPoint.Y - mousePoint.Y) < range) {
                        return;
                    }

                    if (handler.IsDraggingClip) {
                        throw new Exception("Did not expect handler IsDraggingClip to be true");
                    }
                    else if (handler.IsDraggingRightThumb) {
                        throw new Exception("Did not expect handler IsDraggingRightThumb to be true");
                    }
                    else if (handler.IsDraggingLeftThumb) {
                        throw new Exception("Did not expect handler IsDraggingLeftThumb to be true");
                    }
                    else {
                        this.isClipDragRunning = true;
                        handler.OnDragStart();
                    }
                }

                if (!handler.IsDraggingClip) {
                    return;
                }

                double diffX = mousePoint.X - lastClickPoint.X;
                double diffY = mousePoint.Y - lastClickPoint.Y;

                long negativeFrames = 0;
                if (Math.Abs(diffX) >= 1.0d) {
                    long offset = (long) (diffX / this.UnitZoom);
                    if (offset != 0) {
                        long begin = this.FrameBegin;
                        if ((begin + offset) < 0) {
                            negativeFrames = offset;
                            offset = -begin;
                        }

                        if (offset != 0) {
                            try {
                                this.isProcessingMouseMove = true;
                                handler.OnDragDelta(offset);
                            }
                            finally {
                                this.isProcessingMouseMove = false;
                            }
                        }
                    }
                }

                if (!didJustDragTrack && Math.Abs(diffY) >= 1.0d) {
                    int index = 0;
                    List<TimelineTrackControl> tracks = this.Timeline.GetTrackContainers().ToList();
                    foreach (TimelineTrackControl track in tracks) {
                        // IsMouseOver does not work
                        Point mpos = e.GetPosition(track);
                        if (mpos.Y >= 0 && mpos.Y < track.ActualHeight && track.CanAcceptClip(this))
                            break;
                        index++;
                    }

                    if (index < tracks.Count) {
                        this.Timeline.ClipMousePosForTrackTransition = lastClickPoint;
                        handler.OnDragToTrack(index);
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape) {
                e.Handled = true;
                this.CancelDrag();
            }
        }

        public void CancelDrag() {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.isClipDragRunning = false;
            this.lastLeftClickPoint = null;
            if (this.DragHandler is IClipDragHandler handler && handler.IsDraggingClip) {
                handler.OnDragStop(true);
            }
        }

        public virtual void OnSelected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        public virtual void OnUnselected(RoutedEventArgs e) {
            this.RaiseEvent(e);
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

        #region Size Calculations

        public void OnUnitZoomChanged() {
            if (this.isUpdatingUnitZoom) {
                return;
            }

            try {
                this.isUpdatingUnitZoom = true;
                this.UpdatePositionAndSize();
            }
            finally {
                this.isUpdatingUnitZoom = false;
            }
        }

        public void UpdatePositionAndSize() {
            this.UpdatePosition();
            this.UpdateSize();
        }

        public void UpdatePosition() {
            Canvas.SetLeft(this, this.PixelStart);
            // Thickness margin = this.Margin;
            // margin.Left = this.PixelStart;
            // this.Margin = margin;
            // this.InvalidateArrange();
        }

        public void UpdateSize() {
            // this.Width = this.PixelWidth;
            this.InvalidateMeasure();
        }

        #endregion

        #region Drag Dropping

        protected override void OnDragOver(DragEventArgs e) {
            if (e.Data.GetDataPresent(nameof(BaseResourceObjectViewModel))) {
                object obj = e.Data.GetData(nameof(BaseResourceObjectViewModel));
                if (obj is BaseResourceObjectViewModel resource && this.DataContext is IAcceptResourceDrop drop && drop.CanDropResource(resource)) {
                    this.IsDroppableTargetOver = true;
                    e.Effects = DragDropEffects.Move;
                    e.Handled = true;
                    goto end;
                }
            }

            this.ClearValue(IsDroppableTargetOverProperty);
            e.Effects = DragDropEffects.None;

            end:
            e.Handled = true;
            base.OnDragOver(e);
        }

        protected override void OnDragLeave(DragEventArgs e) {
            base.OnDragLeave(e);
            this.Dispatcher.Invoke(() => {
                this.ClearValue(IsDroppableTargetOverProperty);
            }, DispatcherPriority.Loaded);
        }

        private void OnDrop(object sender, DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop) {
                return;
            }

            this.isProcessingAsyncDrop = true;
            if (this.DataContext is IAcceptResourceDrop drop && e.Data.GetData(nameof(BaseResourceObjectViewModel)) is BaseResourceObjectViewModel resource) {
                if (drop.CanDropResource(resource)) {
                    this.HandleOnDropResource(drop, resource);
                }
            }
        }

        private async void HandleOnDropResource(IAcceptResourceDrop acceptResourceDrop, BaseResourceObjectViewModel resource) {
            await acceptResourceDrop.OnDropResource(resource);
            this.ClearValue(IsDroppableTargetOverProperty);
            this.isProcessingAsyncDrop = false;
        }

        #endregion

        protected T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }
    }
}