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
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Interactivity;
using FramePFX.PropertyEditing;
using FramePFX.Utils;
using FramePFX.WPF.Editor.Effects;
using FramePFX.WPF.Editor.Resources;
using FramePFX.WPF.Editor.Timeline.Utils;
using FramePFX.WPF.Interactivity;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    public class TimelineClipControl : Control {
        private static readonly object LongZeroObject = 0L;
        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(TimelineClipControl));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(TimelineClipControl));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((TimelineClipControl) d).OnSelectionChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    LongZeroObject,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange,
                    (d, e) => {
                        if ((long) e.OldValue != (long) e.NewValue) {
                            TimelineClipControl control = ((TimelineClipControl) d);
                            Canvas.SetLeft(control, control.PixelStart);
                        }
                    },
                    (d, v) => (long) v < 0 ? LongZeroObject : v));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(TimelineClipControl),
                new FrameworkPropertyMetadata(
                    LongZeroObject,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    null, (d, v) => (long) v < 0 ? LongZeroObject : v));

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

        public TimelineEditorControl Timeline => this.Track?.Timeline;

        private bool isProcessingAsyncDrop;
        private bool isUpdatingUnitZoom;
        private Point? lastLeftClickPoint;

        private Thumb PART_ThumbLeft;
        private Thumb PART_ThumbRight;
        private bool isProcessingLeftThumbDrag;
        private bool isProcessingRightThumbDrag;
        private bool isProcessingMouseMove;
        private bool isClipDragRunning;
        private bool isLoadedWithActiveDrag;

        public TimelineClipControl() {
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.Focusable = true;
            this.AllowDrop = true;
            this.Drop += this.OnDrop;
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (this.DataContext is ClipViewModel handler) {
                this.CoerceValue(IsSelectedProperty);
                if (handler.IsDraggingClip) {
                    this.isLoadedWithActiveDrag = true;
                    this.Focus();
                    this.CaptureMouse();
                    if (!handler.IsSelected)
                        this.IsSelected = true;
                }
            }

            this.Loaded -= this.OnLoaded;
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
                visualChild?.Measure(size); // shouldn't be null due to the VisualChildrenCount logic
            }

            return size;
        }

        private void OnAnyThumbDragStart() {
            this.Focus();
            TimelineEditorControl timeline = this.Timeline;
            if (timeline != null) {
                if (KeyboardUtils.AreModifiersPressed(ModifierKeys.Control)) {
                    this.IsSelected = !this.IsSelected;
                }
                else if (!timeline.GetSelectedClipContainers().HasAtleast(2)) {
                    this.Timeline?.SetPrimarySelection(this, true);
                }
            }
        }

        private void OnLeftThumbDragStart(object sender, DragStartedEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0 && this.Timeline is TimelineEditorControl timeline) {
                e.Handled = true;
                timeline.ScrollToFrame(this.FrameBegin);
            }
            else if (this.DataContext is ClipViewModel handler && !handler.IsDraggingLeftThumb) {
                this.OnAnyThumbDragStart();
                handler.OnLeftThumbDragStart();
            }
        }

        private void OnLeftThumbDragDelta(object sender, DragDeltaEventArgs e) {
            if (this.isProcessingLeftThumbDrag)
                return;
            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0 || !(this.DataContext is ClipViewModel handler))
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
            if (this.DataContext is ClipViewModel handler && handler.IsDraggingLeftThumb) {
                handler.OnLeftThumbDragStop(e.Canceled);
            }
        }

        private void OnRightThumbDragStart(object sender, DragStartedEventArgs e) {
            if ((Keyboard.Modifiers & ModifierKeys.Alt) != 0 && this.Timeline is TimelineEditorControl timeline) {
                e.Handled = true;
                timeline.ScrollToFrame(this.FrameBegin + this.FrameDuration);
            }
            else if (this.DataContext is ClipViewModel handler && !handler.IsDraggingRightThumb) {
                this.OnAnyThumbDragStart();
                handler.OnRightThumbDragStart();
            }
        }

        private void OnRightThumbDragDelta(object sender, DragDeltaEventArgs e) {
            if (this.isProcessingRightThumbDrag)
                return;
            long change = TimelineUtils.PixelToFrame(e.HorizontalChange, this.UnitZoom);
            if (change == 0 || !(this.DataContext is ClipViewModel handler))
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
            if (this.DataContext is ClipViewModel handler && handler.IsDraggingRightThumb) {
                handler.OnRightThumbDragStop(e.Canceled);
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            this.Track?.MakeTopElement(this);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (e.Handled) {
                return;
            }

            if (this.DataContext is ClipViewModel handler && !handler.IsDraggingClip) {
                if (this.IsFocused || this.Focus()) {
                    TimelineTrackControl track = this.Track;
                    TimelineEditorControl timeline = track?.Timeline;
                    if (!this.IsMouseCaptured) {
                        this.CaptureMouse();
                    }

                    this.lastLeftClickPoint = e.GetPosition(this);
                    if (!KeyboardUtils.AreModifiersPressed(ModifierKeys.Control)) {
                        if (KeyboardUtils.AreModifiersPressed(ModifierKeys.Shift)) {
                            // if (track.lastSelectedItem != null && track.SelectedItems.Count > 0) {
                            //     track.MakeRangedSelection(track.lastSelectedItem, this);
                            // }
                            if (timeline?.DataContext is TimelineViewModel vm) {
                                if (track.DataContext is TrackViewModel targetTrack) {
                                    Point mPos = e.GetPosition(timeline);
                                    double posX = mPos.X + timeline.PART_ScrollViewer?.HorizontalOffset ?? 0d;
                                    long frameB = TimelineUtils.PixelToFrame(posX, timeline.UnitZoom);
                                    List<ClipViewModel> clips = new List<ClipViewModel>();
                                    if (MakeRangedSelection(vm, targetTrack, frameB, clips)) {
                                        foreach (ClipViewModel clip in clips) {
                                            if (!clip.IsSelected) {
                                                clip.IsSelected = true;
                                            }
                                        }

                                        timeline.OnSelectionOperationCompleted();
                                    }
                                }
                            }
                        }
                        else if (timeline != null && timeline.GetSelectedClipContainers().HasAtleast(2)) {
                            if (!this.IsSelected) {
                                timeline.SetPrimarySelection(this, true);
                            }
                        }
                        else {
                            timeline?.SetPrimarySelection(this, true);
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        public static bool MakeRangedSelection(TimelineViewModel timeline, TrackViewModel targetTrack, long targetFrame, List<ClipViewModel> clips) {
            int iA, iB;
            if (timeline == null || targetTrack == null)
                return false;

            TrackViewModel trackA = timeline.PreviouslySelectedTrack;
            if (trackA == null || (iA = timeline.Tracks.IndexOf(trackA)) == -1)
                return false;
            if ((iB = timeline.Tracks.IndexOf(targetTrack)) == -1)
                return false;

            if (iA > iB) {
                Maths.Swap(ref iA, ref iB);
            }

            long srcFrame = timeline.PlayHeadFrame;
            if (srcFrame > targetFrame) {
                Maths.Swap(ref srcFrame, ref targetFrame);
            }

            FrameSpan range = FrameSpan.FromIndex(srcFrame, targetFrame + 1);
            for (int i = iA; i <= iB; i++) {
                TrackViewModel track = timeline.Tracks[i];
                foreach (ClipViewModel clip in track.Clips) {
                    if (clip.FrameSpan.Intersects(range)) {
                        clips.Add(clip);
                    }
                }
            }

            return true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            this.lastLeftClickPoint = null;
            this.isClipDragRunning = false;
            bool? wasDragging = null;
            if (this.DataContext is ClipViewModel handler && this.IsMouseCaptured && handler.IsDraggingClip) {
                wasDragging = true;
                handler.OnDragStop(false);
                this.ReleaseMouseCapture();
                e.Handled = true;
            }

            TimelineEditorControl timeline = this.Timeline;
            if (wasDragging != true) {
                if (timeline != null && KeyboardUtils.AreAnyModifiersPressed(ModifierKeys.Alt)) {
                    timeline.ScrollToFrame(this.FrameBegin + (this.FrameDuration / 2));
                    e.Handled = true;
                    return;
                }

                if (KeyboardUtils.AreModifiersPressed(ModifierKeys.Control)) {
                    this.IsSelected = !this.IsSelected;
                    timeline?.OnSelectionOperationCompleted();
                }
                else if (this.IsSelected) {
                    if (!KeyboardUtils.AreModifiersPressed(ModifierKeys.Shift)) {
                        if (this.Track.Timeline.GetSelectedClipContainers().HasAtleast(2)) {
                            timeline?.SetPrimarySelection(this, false);
                        }
                    }

                    if (timeline != null) {
                        Point mPos = e.GetPosition(timeline);
                        double posX = mPos.X + timeline.PART_ScrollViewer?.HorizontalOffset ?? 0d;
                        long frame = TimelineUtils.PixelToFrame(posX, timeline.UnitZoom);
                        if (frame > timeline.MaxDuration) {
                            timeline.MaxDuration = frame + 300;
                        }

                        timeline.PlayHeadFrame = frame;
                    }

                    // track.Timeline.PlayHeadFrame =
                }
            }

            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (!(this.DataContext is ClipViewModel clip) || this.isProcessingMouseMove || (this.PART_ThumbLeft?.IsDragging ?? true) || (this.PART_ThumbRight?.IsDragging ?? true)) {
                return;
            }

            bool didJustDragTrack = false;
            if (this.isLoadedWithActiveDrag) {
                this.isLoadedWithActiveDrag = false;
                didJustDragTrack = true;
                if (clip.IsDraggingClip) {
                    this.lastLeftClickPoint = this.Timeline.ClipMousePosForTrackTransition;
                    this.isClipDragRunning = true;
                }
            }

            if (e.MouseDevice.LeftButton != MouseButtonState.Pressed) {
                if (ReferenceEquals(e.MouseDevice.Captured, this))
                    this.ReleaseMouseCapture();
                this.isClipDragRunning = false;
                this.lastLeftClickPoint = null;
                if (clip.IsDraggingClip) {
                    clip.OnDragStop(false);
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

                    if (clip.IsDraggingClip) {
                        throw new Exception("Did not expect handler IsDraggingClip to be true");
                    }
                    else if (clip.IsDraggingRightThumb) {
                        throw new Exception("Did not expect handler IsDraggingRightThumb to be true");
                    }
                    else if (clip.IsDraggingLeftThumb) {
                        throw new Exception("Did not expect handler IsDraggingLeftThumb to be true");
                    }
                    else {
                        this.isClipDragRunning = true;
                        clip.OnDragStart();
                    }
                }

                if (!clip.IsDraggingClip) {
                    return;
                }

                double diffX = mousePoint.X - lastClickPoint.X;
                double diffY = mousePoint.Y - lastClickPoint.Y;

                if (Math.Abs(diffX) >= 1.0d) {
                    long offset = (long) (diffX / this.UnitZoom);
                    if (offset != 0) {
                        long begin = this.FrameBegin;
                        if ((begin + offset) < 0) {
                            offset = -begin;
                        }

                        if (offset != 0) {
                            try {
                                this.isProcessingMouseMove = true;
                                clip.OnDragDelta(offset);
                            }
                            finally {
                                this.isProcessingMouseMove = false;
                            }
                        }
                    }
                }

                TimelineEditorControl timeline = this.Timeline;
                if (timeline != null && timeline.PART_ScrollViewer != null && timeline.AutoScrollOnClipDrag) {
                    timeline.AutoScrollFrame(this.FrameBegin, this.FrameBegin + this.FrameDuration);
                }

                if (!didJustDragTrack && Math.Abs(diffY) >= 1.0d) {
                    int index = 0;
                    List<TimelineTrackControl> tracks = this.Timeline.GetTrackContainers().ToList();
                    foreach (TimelineTrackControl track in tracks) {
                        if (!(track.DataContext is TrackViewModel vm))
                            continue;

                        // IsMouseOver does not work
                        Point mpos = e.GetPosition(track);
                        if (mpos.Y >= 0 && mpos.Y < track.ActualHeight && vm.IsClipTypeAcceptable(clip))
                            break;
                        index++;
                    }

                    if (index < tracks.Count) {
                        this.Timeline.ClipMousePosForTrackTransition = lastClickPoint;
                        clip.OnDragToTrack(index);
                    }
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape) {
                e.Handled = true;
                if (this.DataContext is ClipViewModel clip && clip.IsDraggingClip) {
                    this.CancelDrag();
                }
                else {
                    TimelineEditorControl timeline = this.Timeline;
                    if (timeline != null) {
                        foreach (TimelineTrackControl trackElement in timeline.GetTrackContainers()) {
                            if (trackElement.SelectedItems.Count > 0) {
                                trackElement.UnselectAll();
                            }
                        }

                        timeline.OnSelectionOperationCompleted();
                    }
                }
            }
        }

        public void CancelDrag() {
            if (this.IsMouseCaptured)
                this.ReleaseMouseCapture();
            this.isClipDragRunning = false;
            this.lastLeftClickPoint = null;
            if (this.DataContext is ClipViewModel handler && handler.IsDraggingClip) {
                handler.OnDragStop(true);
            }
        }

        private void OnSelected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        private void OnUnselected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        #region Size Calculations

        public void OnUnitZoomChanged() {
            if (this.isUpdatingUnitZoom) {
                return;
            }

            try {
                this.isUpdatingUnitZoom = true;
                this.UpdatePosition();
                this.UpdateSize();
            }
            finally {
                this.isUpdatingUnitZoom = false;
            }
        }

        public void UpdatePosition() => Canvas.SetLeft(this, this.PixelStart);

        public void UpdateSize() => this.InvalidateMeasure();

        #endregion

        #region Drag Dropping

        protected override void OnDragEnter(DragEventArgs e) {
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop) {
                e.Effects = DragDropEffects.None;
                return;
            }

            EnumDropType outputEffects = EnumDropType.None;
            EnumDropType inputEffects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (inputEffects != EnumDropType.None && this.DataContext is ClipViewModel target) {
                if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceViewModel> resources) {
                    if (resources.Count == 1 && resources[0] is ResourceItemViewModel) {
                        outputEffects = ClipViewModel.DropRegistry.CanDrop(target, resources[0], inputEffects);
                    }
                }
                else if (e.Data.GetData(EffectProviderTreeViewItem.ProviderDropType) is EffectProviderViewModel provider) {
                    outputEffects = ClipViewModel.DropRegistry.CanDrop(target, provider, inputEffects);
                }
                else {
                    outputEffects = ClipViewModel.DropRegistry.CanDropNative(target, new DataObjectWrapper(e.Data), inputEffects);
                }

                if (outputEffects != EnumDropType.None) {
                    this.OnAcceptDrop();
                    e.Effects = (DragDropEffects) outputEffects;
                }
                else {
                    this.IsDroppableTargetOver = false;
                    e.Effects = DragDropEffects.None;
                }
            }
        }

        private void OnAcceptDrop() {
            if (!this.IsDroppableTargetOver)
                this.IsDroppableTargetOver = true;
        }

        protected override void OnDragLeave(DragEventArgs e) {
            base.OnDragLeave(e);
            this.Dispatcher.Invoke(() => {
                this.ClearValue(IsDroppableTargetOverProperty);
            }, DispatcherPriority.Loaded);
        }

        private async void OnDrop(object sender, DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.DataContext is ClipViewModel drop)) {
                return;
            }

            EnumDropType effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Effects == DragDropEffects.None) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (e.Data.GetData(ResourceListControl.ResourceDropType) is List<BaseResourceViewModel> items && items.Count == 1 && items[0] is ResourceItemViewModel) {
                    await ClipViewModel.DropRegistry.OnDropped(drop, items[0], effects);
                }
                else if (e.Data.GetData(EffectProviderTreeViewItem.ProviderDropType) is EffectProviderViewModel provider) {
                    await ClipViewModel.DropRegistry.OnDropped(drop, provider, effects);
                }
                else {
                    await ClipViewModel.DropRegistry.OnDroppedNative(drop, new DataObjectWrapper(e.Data), effects);
                }
            }
            finally {
                this.isProcessingAsyncDrop = false;
                this.IsDroppableTargetOver = false;
            }
        }

        #endregion

        protected T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }

        private void OnSelectionChanged(bool wasSelected, bool isSelected) {
            ClipViewModel clip = (ClipViewModel) this.DataContext;
            if (isSelected) {
                bool isClipSelected = clip?.IsSelected ?? false;
                this.OnSelected(new RoutedEventArgs(Selector.SelectedEvent, this));
                if (clip != null && !isClipSelected && clip.IsSelected) {
                    if (clip.Timeline != null) {
                        PFXPropertyEditorRegistry.Instance.OnClipSelectionChanged(clip.Timeline.Tracks.SelectMany(x => x.SelectedClips).ToList());
                    }
                }
            }
            else {
                this.OnUnselected(new RoutedEventArgs(Selector.UnselectedEvent, this));
            }
        }
    }
}