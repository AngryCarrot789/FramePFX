using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;
using FramePFX.Utils;
using Timeline = FramePFX.Editors.Timelines.Timeline;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips {
    /// <summary>
    /// The control used to represent a clip in a UI
    /// </summary>
    public sealed class TimelineClipControl : Control {
        private static readonly FontFamily SegoeUI = new FontFamily("Segoe UI");
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(TimelineClipControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimelineClipControl), new PropertyMetadata(BoolBox.False));

        public string DisplayName {
            get => (string) this.GetValue(DisplayNameProperty);
            set => this.SetValue(DisplayNameProperty, value);
        }

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        public long FrameBegin {
            get => this.frameBegin;
            private set {
                this.frameBegin = value;
                this.InvalidateMeasure();
                this.Track.OnClipSpanChanged();
            }
        }

        public long FrameDuration {
            get => this.frameDuration;
            private set {
                this.frameDuration = value;
                this.InvalidateMeasure();
                this.Track.OnClipSpanChanged();
                if (this.AutomationEditor is AutomationSequenceEditor editor) {
                    editor.FrameDuration = value;
                }
            }
        }

        public TimelineTrackControl Track { get; private set; }

        public Clip Model { get; private set; }

        public AutomationSequenceEditor AutomationEditor { get; private set; }

        public double TimelineZoom => this.Model.Track?.Timeline?.Zoom ?? 1d;
        
        public double PixelBegin => this.frameBegin * this.TimelineZoom;

        public double PixelWidth => this.frameDuration * this.TimelineZoom;

        private const double MinDragInitPx = 5d;
        private const double EdgeGripSize = 8d;
        public const double HeaderSize = Editors.Timelines.Tracks.Track.MinimumHeight;

        private long frameBegin;
        private long frameDuration;

        private DragState dragState;
        private Point clickPoint;
        private bool isUpdatingFrameSpanFromDrag;
        private bool hasMadeExceptionalSelectionInMouseDown;
        private bool isMovingBetweenTracks;

        private GlyphRun glyphRun;
        private readonly RectangleGeometry renderSizeRectGeometry;

        private readonly AutoPropertyUpdateBinder<Clip> displayNameBinder = new AutoPropertyUpdateBinder<Clip>(DisplayNameProperty, nameof(VideoClip.DisplayNameChanged), b => {
            TimelineClipControl control = (TimelineClipControl) b.Control;
            control.glyphRun = null;
            control.DisplayName = b.Model.DisplayName;
        }, b => b.Model.DisplayName = ((TimelineClipControl) b.Control).DisplayName);

        private readonly AutoPropertyUpdateBinder<Clip> frameSpanBinder = new AutoPropertyUpdateBinder<Clip>(nameof(VideoClip.FrameSpanChanged), obj => ((TimelineClipControl) obj.Control).SetSizeFromSpan(obj.Model.FrameSpan), null);
        private readonly GetSetAutoPropertyBinder<Clip> isSelectedBinder = new GetSetAutoPropertyBinder<Clip>(IsSelectedProperty, nameof(VideoClip.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        public TimelineClipControl() {
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.GotFocus += this.OnGotFocus;
            this.LostFocus += this.OnLostFocus;
            this.renderSizeRectGeometry = new RectangleGeometry();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            if (!(this.GetTemplateChild("PART_AutomationSequence") is AutomationSequenceEditor sequenceEditor))
                throw new Exception("Missing PART_AutomationSequence");
            this.AutomationEditor = sequenceEditor;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.isSelectedBinder?.OnPropertyChanged(e);
        }

        static TimelineClipControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineClipControl), new FrameworkPropertyMetadata(typeof(TimelineClipControl)));
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo) {
            base.OnRenderSizeChanged(sizeInfo);
            this.renderSizeRectGeometry.Rect = new Rect(sizeInfo.NewSize);
        }

        private void OnGotFocus(object sender, RoutedEventArgs e) => Panel.SetZIndex(this, 2);

        private void OnLostFocus(object sender, RoutedEventArgs e) => Panel.SetZIndex(this, 0);

        #region Model Binding

        private void SetSizeFromSpan(FrameSpan span) {
            this.FrameBegin = span.Begin;
            this.FrameDuration = span.Duration;
        }

        public void OnAdding(TimelineTrackControl trackList, Clip clip) {
            this.Track = trackList;
            this.Model = clip;
        }

        public void OnAdded() {
            this.displayNameBinder.Attach(this, this.Model);
            this.frameSpanBinder.Attach(this, this.Model);
            this.isSelectedBinder.Attach(this, this.Model);
            this.Model.ActiveSequenceChanged += this.ClipActiveSequenceChanged;
            if (this.AutomationEditor is AutomationSequenceEditor editor) {
                editor.FrameDuration = this.frameDuration;
                editor.Sequence = this.Model.AutomationData[VideoClip.OpacityParameter];
            }

            this.Model.AutomationData.ActiveParameter = VideoClip.OpacityParameter.Key;
        }

        public void OnRemoving() {
            this.displayNameBinder.Detatch();
            this.frameSpanBinder.Detatch();
            this.isSelectedBinder.Detatch();
            this.AutomationEditor.Sequence = null;
            this.Model.ActiveSequenceChanged -= this.ClipActiveSequenceChanged;
        }

        private void ClipActiveSequenceChanged(Clip clip, AutomationSequence oldsequence, AutomationSequence newsequence) {
            if (this.AutomationEditor is AutomationSequenceEditor editor) {
                editor.Sequence = newsequence;
            }
        }

        public void OnRemoved() {
            this.Track = null;
            this.Model = null;
        }

        #endregion

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (this.Model == null) {
                return;
            }

            e.Handled = true;
            this.Focus();
            this.clickPoint = e.GetPosition(this);
            this.SetDragState(DragState.Initiated);
            if (!this.IsMouseCaptured) {
                this.CaptureMouse();
            }

            Timeline timeline;
            TimelineControl timelineControl = this.Track?.OwnerTimeline;
            if (timelineControl == null || (timeline = timelineControl.Timeline) == null) {
                return;
            }

            long mouseFrame = TLCUtils.GetCursorFrame(this);
            if (timeline.HasAnySelectedClips) {
                if ((Keyboard.Modifiers & ModifierKeys.Shift) != 0) {
                    TrackPoint anchor = timeline.RangedSelectionAnchor;
                    if (anchor.TrackIndex != -1) {
                        int idxA = anchor.TrackIndex;
                        int idxB = this.Model.Track.IndexInTimeline;
                        if (idxA > idxB) {
                            Maths.Swap(ref idxA, ref idxB);
                        }

                        long frameA = anchor.Frame;
                        if (frameA > mouseFrame) {
                            Maths.Swap(ref frameA, ref mouseFrame);
                        }

                        timeline.MakeFrameRangeSelection(FrameSpan.FromIndex(frameA, mouseFrame), idxA, idxB + 1);
                    }
                    else {
                        long frameA = timeline.PlayHeadPosition;
                        if (frameA > mouseFrame) {
                            Maths.Swap(ref frameA, ref mouseFrame);
                        }

                        timeline.MakeFrameRangeSelection(FrameSpan.FromIndex(frameA, mouseFrame));
                    }

                    this.hasMadeExceptionalSelectionInMouseDown = true;
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 && !this.Model.IsSelected) {
                    timeline.MakeSingleSelection(this.Model);
                    timeline.RangedSelectionAnchor = new TrackPoint(this.Model, mouseFrame);
                }
                else {
                    return;
                }

                timelineControl.UpdatePropertyEditorClipSelection();
            }
            else {
                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                    this.Model.IsSelected = !this.Model.IsSelected;
                    this.hasMadeExceptionalSelectionInMouseDown = true;
                }
                else {
                    timeline.MakeSingleSelection(this.Model);
                    timeline.RangedSelectionAnchor = new TrackPoint(this.Model, mouseFrame);
                }

                timelineControl.UpdatePropertyEditorClipSelection();
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            if (this.Model == null) {
                return;
            }

            e.Handled = true;
            DragState lastDragState = this.dragState;
            if (this.dragState == DragState.Initiated && !this.hasMadeExceptionalSelectionInMouseDown) {
                this.Track.OwnerPanel.SetPlayHeadToMouseCursor(e.MouseDevice);
            }

            this.SetDragState(DragState.None);
            this.SetCursorForMousePoint(e.GetPosition(this));
            this.ReleaseMouseCapture();

            if (this.hasMadeExceptionalSelectionInMouseDown) {
                this.hasMadeExceptionalSelectionInMouseDown = false;
            }
            else {
                Timeline timeline;
                TimelineControl timelineControl = this.Track?.OwnerTimeline;
                if (timelineControl == null || (timeline = timelineControl.Timeline) == null) {
                    return;
                }

                if ((lastDragState == DragState.None || lastDragState == DragState.Initiated) && timeline.HasAnySelectedClips) {
                    if ((Keyboard.Modifiers & ModifierKeys.Control) != 0) {
                        this.Model.IsSelected = !this.Model.IsSelected;
                    }
                    else if (this.Model.IsSelected && (Keyboard.Modifiers & ModifierKeys.Shift) == 0) {
                        timeline.MakeSingleSelection(this.Model);
                        timeline.RangedSelectionAnchor = new TrackPoint(this.Model, TLCUtils.GetCursorFrame(this));
                    }

                    timelineControl.UpdatePropertyEditorClipSelection();
                }
            }
        }

        private void SetDragState(DragState state) {
            if (this.dragState != state) {
                this.dragState = state;
                this.SetCursorForDragState(state, false);
            }
        }

        private void SetCursorForDragState(DragState state, bool isPreview) {
            if (isPreview && this.dragState != DragState.None) {
                return;
            }

            switch (state) {
                case DragState.None:          this.ClearValue(CursorProperty); break;
                case DragState.Initiated:     break;
                case DragState.DragBody:      this.Cursor = Cursors.SizeAll; break;
                case DragState.DragLeftEdge:  this.Cursor = Cursors.SizeWE; break;
                case DragState.DragRightEdge: this.Cursor = Cursors.SizeWE; break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.Model == null) {
                return;
            }

            if (this.isUpdatingFrameSpanFromDrag) {
                // prevent possible stack overflow exceptions, at the cost of the UI possibly glitching a bit.
                // In my testing, this case is never reached, so it would require something very weird to happen
                return;
            }

            if (this.isMovingBetweenTracks) {
                this.isMovingBetweenTracks = false;
                return;
            }

            Point mpos = e.GetPosition(this);

            if (e.LeftButton != MouseButtonState.Pressed) {
                this.SetDragState(DragState.None);
                this.SetCursorForMousePoint(mpos);
                this.ReleaseMouseCapture();
                return;
            }

            this.SetCursorForMousePoint(mpos);
            TrackStoragePanel ctrl;
            if (this.Track == null || (ctrl = this.Track.OwnerPanel) == null) {
                return;
            }

            if (this.dragState == DragState.Initiated) {
                if (Math.Abs(mpos.X - this.clickPoint.X) < MinDragInitPx && Math.Abs(mpos.Y - this.clickPoint.Y) < MinDragInitPx) {
                    return;
                }

                ClipPart part = this.GetPartForPoint(this.clickPoint);
                switch (part) {
                    case ClipPart.Header:
                        this.SetDragState(DragState.DragBody);
                        break;
                    case ClipPart.LeftGrip:
                        this.SetDragState(DragState.DragLeftEdge);
                        break;
                    case ClipPart.RightGrip:
                        this.SetDragState(DragState.DragRightEdge);
                        break;
                }
            }
            else if (this.dragState == DragState.None) {
                return;
            }

            double zoom = this.Model.Track?.Timeline?.Zoom ?? 1.0;
            Vector mdif = mpos - this.clickPoint;
            FrameSpan oldSpan = this.Model.FrameSpan;
            if (this.dragState == DragState.DragBody) {
                if (Math.Abs(mdif.X) >= 1.0d) {
                    long offset = (long) Math.Round(mdif.X / zoom);
                    if (offset != 0) {
                        // If begin is 2 and offset is -5, this sets offset to -2
                        // and since newBegin = begin+offset (2 + -2)
                        // this ensures begin never drops below 0
                        if ((oldSpan.Begin + offset) < 0) {
                            offset = -oldSpan.Begin;
                        }

                        if (offset != 0) {
                            FrameSpan newSpan = new FrameSpan(oldSpan.Begin + offset, oldSpan.Duration);
                            long newEndIndex = newSpan.EndIndex;
                            if (newEndIndex > ctrl.Timeline.MaxDuration) {
                                ctrl.Timeline.MaxDuration = newEndIndex + 300;
                            }

                            this.isUpdatingFrameSpanFromDrag = true;
                            this.Model.FrameSpan = newSpan;
                            this.isUpdatingFrameSpanFromDrag = false;
                        }
                    }
                }

                if (Math.Abs(mdif.Y) >= 1.0d && ctrl.Timeline is Timeline timeline) {
                    int trackIndex = timeline.Tracks.IndexOf(this.Model.Track);
                    const double area = 0;
                    if (mpos.Y < Math.Min(area, this.clickPoint.Y)) {
                        if (trackIndex < 1) {
                            return;
                        }

                        this.isMovingBetweenTracks = true;
                        this.Model.MoveToTrack(timeline.Tracks[trackIndex - 1]);
                    }
                    else if (mpos.Y > (this.ActualHeight - area)) {
                        if (trackIndex >= (timeline.Tracks.Count - 1)) {
                            return;
                        }

                        this.isMovingBetweenTracks = true;
                        this.Model.MoveToTrack(timeline.Tracks[trackIndex + 1]);
                    }
                }
            }
            else if (this.dragState == DragState.DragLeftEdge || this.dragState == DragState.DragRightEdge) {
                if (Math.Abs(mdif.X) >= 1.0d) {
                    long offset = (long) Math.Round(mdif.X / zoom);
                    if (offset == 0) {
                        return;
                    }

                    if (this.dragState == DragState.DragRightEdge) {
                        // Clamps the offset to ensure we don't end up with a negative duration
                        if ((oldSpan.EndIndex + offset) <= oldSpan.Begin) {
                            // add 1 to ensure clip is always 1 frame long, just because ;)
                            offset = -oldSpan.Duration + 1;
                        }

                        if (offset != 0) {
                            long newEndIndex = oldSpan.EndIndex + offset;
                            // Clamp new frame span to 1 frame, in case user resizes too much to the right
                            // if (newEndIndex >= oldSpan.EndIndex) {
                            //     this.dragAccumulator -= (newEndIndex - oldSpan.EndIndex);
                            //     newEndIndex = oldSpan.EndIndex - 1;
                            // }

                            FrameSpan newSpan = FrameSpan.FromIndex(oldSpan.Begin, newEndIndex);
                            if (newEndIndex > ctrl.Timeline.MaxDuration) {
                                ctrl.Timeline.MaxDuration = newEndIndex + 300;
                            }

                            this.isUpdatingFrameSpanFromDrag = true;
                            this.Model.FrameSpan = newSpan;
                            this.isUpdatingFrameSpanFromDrag = false;

                            // account for there being no "grip" control aligned to the right side;
                            // since the clip is resized, the origin point will not work correctly and
                            // results in an exponential endIndex increase unless the below code is used.
                            // This code is not needed for the left grip because it just naturally isn't
                            this.clickPoint.X += (newSpan.EndIndex - oldSpan.EndIndex) * zoom;
                        }
                    }
                    else {
                        if ((oldSpan.Begin + offset) < 0) {
                            offset = -oldSpan.Begin;
                        }

                        if (offset != 0) {
                            long newBegin = oldSpan.Begin + offset;
                            // Clamps the offset to ensure we don't end up with a negative duration
                            if (newBegin >= oldSpan.EndIndex) {
                                // subtract 1 to ensure clip is always 1 frame long
                                newBegin = oldSpan.EndIndex - 1;
                            }

                            FrameSpan newSpan = FrameSpan.FromIndex(newBegin, oldSpan.EndIndex);
                            long newEndIndex = newSpan.EndIndex;
                            if (newEndIndex > ctrl.Timeline.MaxDuration) {
                                ctrl.Timeline.MaxDuration = newEndIndex + 300;
                            }

                            this.isUpdatingFrameSpanFromDrag = true;
                            this.Model.FrameSpan = newSpan;
                            this.isUpdatingFrameSpanFromDrag = false;
                        }
                    }
                }
            }
        }

        protected override Size MeasureOverride(Size availableSize) {
            Size size = new Size(this.PixelWidth, HeaderSize);
            base.MeasureOverride(size);
            return size;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            Size size = new Size(this.PixelWidth, finalSize.Height);
            base.ArrangeOverride(size);
            return size;
        }

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            Rect rect = new Rect(new Point(), this.RenderSize);
            if (this.renderSizeRectGeometry.Rect != rect) {
                this.renderSizeRectGeometry.Rect = rect;
            }

            dc.PushClip(this.renderSizeRectGeometry);

            if (this.Background is Brush background) {
                dc.DrawRectangle(background, null, rect);
            }

            if (this.Track?.TrackColourBrush is Brush headerBrush) {
                dc.DrawRectangle(headerBrush, null, new Rect(0, 0, rect.Width, Math.Min(rect.Height, HeaderSize)));
            }

            // glyph run is way faster than using formatted text
            if (this.glyphRun == null && this.DisplayName is string str && !string.IsNullOrWhiteSpace(str)) {
                Typeface typeface = new Typeface(SegoeUI, this.FontStyle, FontWeights.SemiBold, this.FontStretch);
                Point origin = new Point(3, 14); // hard coded offset for Segoe UI and header size of 20 px
                this.glyphRun = GlyphGenerator.CreateText(str, 12d, typeface, origin);
            }

            if (this.glyphRun != null) {
                dc.DrawGlyphRun(Brushes.White, this.glyphRun);
            }

            dc.Pop();

            // Pen pen = new Pen(Brushes.Black, 1d);
            // dc.DrawLine(pen, new Point(0d, 0d), new Point(0d, rect.Height));
            // dc.DrawLine(pen, new Point(rect.Width - 1d, 0d), new Point(rect.Width - 1d, rect.Height));
        }

        public void OnZoomChanged(double newZoom) {
            // this.InvalidateMeasure();
            this.AutomationEditor.UnitZoom = newZoom;
        }

        private ClipPart GetPartForPoint(Point mpos) {
            if (mpos.Y <= HeaderSize) {
                if (mpos.X <= EdgeGripSize) {
                    return ClipPart.LeftGrip;
                }
                else if (mpos.X >= (this.ActualWidth - EdgeGripSize)) {
                    return ClipPart.RightGrip;
                }
                else {
                    return ClipPart.Header;
                }
            }
            else {
                Size size = this.RenderSize;
                if (mpos.X < 0 || mpos.Y < 0 || mpos.X > size.Width || mpos.Y > size.Height)
                    return ClipPart.None;
                return ClipPart.Body;
            }
        }

        private void SetCursorForMousePoint(Point mpos) {
            ClipPart part = this.GetPartForPoint(mpos);
            switch (part) {
                case ClipPart.None:
                case ClipPart.Body:
                    this.SetCursorForDragState(DragState.None, true);
                    break;
                case ClipPart.Header:
                    this.SetCursorForDragState(DragState.DragBody, true);
                    break;
                case ClipPart.LeftGrip:
                    this.SetCursorForDragState(DragState.DragLeftEdge, true);
                    break;
                case ClipPart.RightGrip:
                    this.SetCursorForDragState(DragState.DragRightEdge, true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private enum DragState {
            None,
            Initiated,
            DragBody,
            DragLeftEdge,
            DragRightEdge
        }

        private enum ClipPart {
            None,
            Body,
            Header,
            LeftGrip,
            RightGrip
        }
    }
}
