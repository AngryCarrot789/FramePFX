using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.EffectProviding;
using FramePFX.Editors.Controls.Resources;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.EffectSource;
using FramePFX.Editors.Factories;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;
using Timeline = FramePFX.Editors.Timelines.Timeline;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Clips {
    /// <summary>
    /// The control used to represent a clip in a UI
    /// </summary>
    public sealed class TimelineClipControl : ContentControl {
        private static readonly FontFamily SegoeUI = new FontFamily("Segoe UI");
        public static readonly DependencyProperty DisplayNameProperty = DependencyProperty.Register("DisplayName", typeof(string), typeof(TimelineClipControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimelineClipControl), new PropertyMetadata(BoolBox.False, OnIsSelectedChanged));
        private static readonly Pen WhitePen = new Pen(Brushes.White, 2.0d);

        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(TimelineClipControl), new PropertyMetadata(BoolBox.False));

        public string DisplayName {
            get => (string) this.GetValue(DisplayNameProperty);
            set => this.SetValue(DisplayNameProperty, value);
        }

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value.Box());
        }

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value.Box());
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

        public TimelineControl TimelineControl => this.Track?.TimelineControl;

        public Clip Model { get; private set; }

        public AutomationSequenceEditor AutomationEditor { get; private set; }

        public double TimelineZoom => this.Model.Track?.Timeline?.Zoom ?? 1d;

        public double PixelBegin => this.frameBegin * this.TimelineZoom;

        public double PixelWidth => this.frameDuration * this.TimelineZoom;

        private const double EdgeGripSize = 8d;
        public const double HeaderSize = Editors.Timelines.Tracks.Track.MinimumHeight;

        private long frameBegin;
        private long frameDuration;

        private DragState dragState;
        private Track trackAtDragBegin;
        private FrameSpan spanAtDragBegin;

        private Point clickPos;
        private Point clickPosAbs;

        private bool hasMadeExceptionalSelectionInMouseDown;
        private bool isMovingBetweenTracks;
        private bool isProcessingAsyncDrop;

        private GlyphRun glyphRun;
        private readonly RectangleGeometry renderSizeRectGeometry;

        // The clip control that created this current instance (we are the phantom clip)
        private TimelineClipControl dragCopyOwnerControl;

        private readonly AutoPropertyUpdateBinder<Clip> displayNameBinder = new AutoPropertyUpdateBinder<Clip>(DisplayNameProperty, nameof(VideoClip.DisplayNameChanged), b => {
            TimelineClipControl control = (TimelineClipControl) b.Control;
            control.glyphRun = null;
            control.DisplayName = b.Model.DisplayName;
        }, b => b.Model.DisplayName = ((TimelineClipControl) b.Control).DisplayName);

        private readonly AutoPropertyUpdateBinder<Clip> frameSpanBinder = new AutoPropertyUpdateBinder<Clip>(nameof(VideoClip.FrameSpanChanged), obj => ((TimelineClipControl) obj.Control).SetSizeFromSpan(obj.Model.FrameSpan), null);
        private readonly GetSetAutoPropertyBinder<Clip> isSelectedBinder = new GetSetAutoPropertyBinder<Clip>(IsSelectedProperty, nameof(VideoClip.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);

        public TimelineClipControl() {
            this.AllowDrop = true;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.GotFocus += this.OnGotFocus;
            this.LostFocus += this.OnLostFocus;
            this.renderSizeRectGeometry = new RectangleGeometry();
            AdvancedContextMenu.SetContextGenerator(this, ClipContextRegistry.Instance);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            if (!(this.GetTemplateChild("PART_AutomationSequence") is AutomationSequenceEditor sequenceEditor))
                throw new Exception("Missing PART_AutomationSequence");
            this.AutomationEditor = sequenceEditor;
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if ((bool) e.NewValue) {
                TimelineClipControl clip = (TimelineClipControl) d;
                clip.Focus();
                // if (clip.Track?.OwnerTimeline?.TimelineContentGrid is TimelineScrollableContentGrid grid) {
                //     bool oldValue = grid.HandleBringIntoView;
                //     grid.HandleBringIntoView = false;
                //     try {
                //         clip.Focus();
                //         clip.BringIntoView();
                //     }
                //     finally {
                //         grid.HandleBringIntoView = oldValue;
                //     }
                // }
                // else {
                //     clip.Focus();
                // }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            this.isSelectedBinder?.OnPropertyChanged(e);
        }

        static TimelineClipControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineClipControl), new FrameworkPropertyMetadata(typeof(TimelineClipControl)));
            if (WhitePen.CanFreeze)
                WhitePen.Freeze();
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

        private void OnVisibilityParameterChanged(DataParameter parameter, ITransferableData owner) {
            this.UpdateClipVisibleState();
        }

        private void UpdateClipVisibleState() {
            bool newValue = !(this.Model is VideoClip clip) || VideoClip.IsVisibleParameter.GetValue(clip);
            if (newValue != this.IsClipVisible) {
                this.IsClipVisible = newValue;
                this.InvalidateVisual();
            }
        }

        public void OnAdding(TimelineTrackControl trackList, Clip clip) {
            this.Track = trackList;
            this.Model = clip;
            this.Content = trackList.TimelineControl.GetClipContentObject(clip.GetType());
            if (clip is VideoClip) {
                clip.TransferableData.AddValueChangedHandler(VideoClip.IsVisibleParameter, this.OnVisibilityParameterChanged);
                this.UpdateClipVisibleState();
            }

            DataManager.SetContextData(this, new DataContext().Set(DataKeys.ClipKey, clip));
        }

        public void OnAdded() {
            TimelineClipContent content = (TimelineClipContent) this.Content;
            if (content != null) {
                content.ApplyTemplate();
                content.Connect(this);
            }

            this.displayNameBinder.Attach(this, this.Model);
            this.frameSpanBinder.Attach(this, this.Model);
            this.isSelectedBinder.Attach(this, this.Model);
            this.Model.ActiveSequenceChanged += this.ClipActiveSequenceChanged;
            if (this.AutomationEditor is AutomationSequenceEditor editor) {
                editor.FrameDuration = this.frameDuration;
                editor.Sequence = this.Model is VideoClip ? this.Model.AutomationData[VideoClip.OpacityParameter] : null;
            }

            this.Model.AutomationData.ActiveParameter = VideoClip.OpacityParameter.Key;
        }

        public void OnRemoving() {
            this.displayNameBinder.Detatch();
            this.frameSpanBinder.Detatch();
            this.isSelectedBinder.Detatch();
            this.AutomationEditor.Sequence = null;
            this.Model.ActiveSequenceChanged -= this.ClipActiveSequenceChanged;
            if (this.Model is VideoClip)
                this.Model.TransferableData.RemoveValueChangedHandler(VideoClip.IsVisibleParameter, this.OnVisibilityParameterChanged);

            TimelineClipContent content = (TimelineClipContent) this.Content;
            if (content != null) {
                content.Disconnect();
                this.Content = null;
                this.Track.TimelineControl.ReleaseContentObject(this.Track.GetType(), content);
            }

            DataManager.ClearContextData(this);
        }

        public void OnRemoved() {
            this.Track = null;
            this.Model = null;
        }

        private void ClipActiveSequenceChanged(Clip clip, AutomationSequence oldsequence, AutomationSequence newsequence) {
            if (this.AutomationEditor is AutomationSequenceEditor editor) {
                editor.Sequence = newsequence;
            }
        }

        #endregion

        #region Drag/Move implementation

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
                    this.SetCursorForDragState(DragState.DragLeftGrip, true);
                    break;
                case ClipPart.RightGrip:
                    this.SetCursorForDragState(DragState.DragRightGrip, true);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (e.ChangedButton != MouseButton.Left && this.dragState != DragState.None) {
                e.Handled = true;
                this.Focus();
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e) {
            base.OnMouseDown(e);
            if (this.Model == null || e.ChangedButton != MouseButton.Left) {
                return;
            }

            this.spanAtDragBegin = this.Model.FrameSpan;
            this.trackAtDragBegin = this.Model.Track;

            e.Handled = true;
            this.Focus();
            this.clickPos = e.GetPosition(this);
            this.clickPosAbs = this.PointToScreen(this.clickPos);
            this.SetDragState(DragState.Initiated);
            if (!this.IsMouseCaptured) {
                this.CaptureMouse();
            }

            Timeline timeline;
            TimelineControl timelineControl = this.Track?.TimelineControl;
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

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (!e.Handled && this.dragState > DragState.Initiated) {
                if (e.Key == Key.Escape) {
                    this.SetDragState(DragState.None);
                    this.SetCursorForMousePoint(Mouse.GetPosition(this));
                    this.ReleaseMouseCapture();

                    // it better not be null!!!
                    if (this.trackAtDragBegin != null) {
                        this.Model.FrameSpan = this.spanAtDragBegin;
                        if (!ReferenceEquals(this.Model.Track, this.trackAtDragBegin)) {
                            this.Model.MoveToTrack(this.trackAtDragBegin);
                        }

                        this.trackAtDragBegin = null;
                    }
                }
                else if (e.Key == Key.LeftCtrl && this.dragState == DragState.DragBody && this.dragCopyOwnerControl == null) {
                    this.BeginDragCopyWithPhantomClip();
                }
            }
        }

        protected override void OnKeyUp(KeyEventArgs e) {
            base.OnKeyUp(e);
            if (!e.Handled && e.Key == Key.LeftCtrl && this.dragCopyOwnerControl != null) {
                this.CancelDragCopyWithPhantomClip();
            }
        }

        protected override void OnContextMenuOpening(ContextMenuEventArgs e) {
            base.OnContextMenuOpening(e);
            if (this.dragState != DragState.None) {
                e.Handled = true;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e) {
            base.OnMouseUp(e);
            if (this.Model == null || e.ChangedButton != MouseButton.Left) {
                return;
            }

            e.Handled = true;
            if (this.dragCopyOwnerControl != null) {
                this.PlaceThisPhantomClipIntoTrackForReal();
                return;
            }

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
                TimelineControl timelineControl = this.Track?.TimelineControl;
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

        private void SetClipSpanForDrag(FrameSpan newSpan) {
            this.Model.FrameSpan = newSpan;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.Model == null) {
                return;
            }

            if (this.isMovingBetweenTracks) {
                this.isMovingBetweenTracks = false;
                return;
            }

            Point mPos = e.GetPosition(this);

            {
                // This is used to prevent "drag jumping" which occurs when a screen pixel is
                // somewhere in between a frame in a sweet spot that results in the control
                // jumping back and forth. To test, do CTRL+MouseWheelUp once to zoom in a bit,
                // and then drag a clip 1 frame at a time and you might see it with the code below removed.
                // This code is pretty much the exact same as what Thumb uses
                Point mPosAbs = this.PointToScreen(mPos);
                // don't care about the Y pos :P
                if (DoubleUtils.AreClose(mPosAbs.X, this.clickPosAbs.X))
                    return;
                this.clickPosAbs = mPosAbs;
            }

            if (e.LeftButton != MouseButtonState.Pressed) {
                this.SetDragState(DragState.None);
                this.SetCursorForMousePoint(mPos);
                this.ReleaseMouseCapture();
                return;
            }

            this.SetCursorForMousePoint(mPos);
            TrackStoragePanel ctrl;
            if (this.Track == null || (ctrl = this.Track.OwnerPanel) == null) {
                return;
            }

            if (this.dragState == DragState.Initiated) {
                double minDragX = SystemParameters.MinimumHorizontalDragDistance;
                double minDragY = SystemParameters.MinimumVerticalDragDistance;
                if (Math.Abs(mPos.X - this.clickPos.X) < minDragX && Math.Abs(mPos.Y - this.clickPos.Y) < minDragY) {
                    return;
                }

                if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && this.dragCopyOwnerControl == null) {
                    this.BeginDragCopyWithPhantomClip();
                    return;
                }

                ClipPart part = this.GetPartForPoint(this.clickPos);
                switch (part) {
                    case ClipPart.Header:
                        this.SetDragState(DragState.DragBody);
                        break;
                    case ClipPart.LeftGrip:
                        this.SetDragState(DragState.DragLeftGrip);
                        break;
                    case ClipPart.RightGrip:
                        this.SetDragState(DragState.DragRightGrip);
                        break;
                }
            }
            else if (this.dragState == DragState.None) {
                return;
            }

            double zoom = this.Model.Track?.Timeline?.Zoom ?? 1.0;
            Vector mPosDifRel = mPos - this.clickPos;

            // Vector mPosDiff = absMpos - this.screenClickPos;
            FrameSpan oldSpan = this.Model.FrameSpan;
            if (this.dragState == DragState.DragBody) {
                double frameOffsetDouble = (mPosDifRel.X / zoom);
                long offset = (long) Math.Round(frameOffsetDouble);
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
                            ctrl.Timeline.TryExpandForFrame(newEndIndex);
                        }

                        this.SetClipSpanForDrag(newSpan);
                    }
                }

                if (Math.Abs(mPosDifRel.Y) >= 1.0d && ctrl.Timeline is Timeline timeline) {
                    int trackIndex = timeline.Tracks.IndexOf(this.Model.Track);
                    const double area = 0;
                    if (mPos.Y < Math.Min(area, this.clickPos.Y)) {
                        if (trackIndex < 1) {
                            return;
                        }

                        this.isMovingBetweenTracks = true;
                        this.Model.MoveToTrack(timeline.Tracks[trackIndex - 1]);
                    }
                    else if (mPos.Y > (this.ActualHeight - area)) {
                        if (trackIndex >= (timeline.Tracks.Count - 1)) {
                            return;
                        }

                        this.isMovingBetweenTracks = true;
                        this.Model.MoveToTrack(timeline.Tracks[trackIndex + 1]);
                    }
                }
            }
            else if (this.dragState == DragState.DragLeftGrip || this.dragState == DragState.DragRightGrip) {
                if (Math.Abs(mPosDifRel.X) >= 1.0d) {
                    long offset = (long) Math.Round(mPosDifRel.X / zoom);
                    if (offset == 0) {
                        return;
                    }

                    if (this.dragState == DragState.DragLeftGrip) {
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
                            ctrl.Timeline.TryExpandForFrame(newSpan.EndIndex);
                            this.SetClipSpanForDrag(newSpan);
                            this.Model.MediaFrameOffset += (oldSpan.Begin - newSpan.Begin);
                        }
                    }
                    else {
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
                            ctrl.Timeline.TryExpandForFrame(newEndIndex);
                            this.SetClipSpanForDrag(newSpan);

                            // account for there being no "grip" control aligned to the right side;
                            // since the clip is resized, the origin point will not work correctly and
                            // results in an exponential endIndex increase unless the below code is used.
                            // This code is not needed for the left grip because it just naturally isn't
                            this.clickPos.X += (newSpan.EndIndex - oldSpan.EndIndex) * zoom;
                        }
                    }
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
                case DragState.None:
                    this.ClearValue(CursorProperty);
                    break;
                case DragState.Initiated: break;
                case DragState.DragBody:
                    this.Cursor = Cursors.SizeAll;
                    break;
                case DragState.DragLeftGrip:
                    this.Cursor = Cursors.SizeWE;
                    break;
                case DragState.DragRightGrip:
                    this.Cursor = Cursors.SizeWE;
                    break;
                default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }

        #endregion

        #region Phantom Clip / Drag Copy Clip

        private void BeginDragCopyWithPhantomClip() {
            FrameSpan phantomSpan = this.Model.FrameSpan;
            if (this.trackAtDragBegin != null) {
                this.Model.FrameSpan = this.spanAtDragBegin;
                if (!ReferenceEquals(this.Model.Track, this.trackAtDragBegin)) {
                    this.Model.MoveToTrack(this.trackAtDragBegin);
                }

                this.trackAtDragBegin = null;
            }

            this.Model.Track.ClearClipSelection();

            // Create a bare type clone, not a full deep clone. This is because the
            // user can cancel the drag, so it would just be wasting CPU cycles cloning
            // the whole thing only for the user to press esc or release CTRL and cancel it
            Clip clone = ClipFactory.Instance.NewClip(this.Model.FactoryId);
            clone.FrameSpan = phantomSpan;
            clone.DisplayName = this.Model.DisplayName + " (phantom)";
            this.Model.Track.AddClip(clone);

            TimelineClipControl cloneControl = this.Track.GetClipAt(clone.Track.Clips.Count - 1);
            cloneControl.dragCopyOwnerControl = this;
            this.SetDragState(DragState.None);
            this.SetCursorForMousePoint(Mouse.GetPosition(this));
            this.ReleaseMouseCapture();

            // IsSelected works both ways, could also set clone.IsSelected too
            cloneControl.IsSelected = true;
            cloneControl.Focus();
            cloneControl.clickPos = this.clickPos;
            cloneControl.clickPosAbs = this.clickPosAbs;
            cloneControl.SetDragState(DragState.DragBody);
            if (!this.IsMouseCaptured) {
                cloneControl.CaptureMouse();
            }
        }

        private void CancelDragCopyWithPhantomClip() {
            TimelineClipControl owner = this.dragCopyOwnerControl;
            this.dragCopyOwnerControl = null;
            this.SetDragState(DragState.None);
            this.SetCursorForMousePoint(Mouse.GetPosition(this));
            this.ReleaseMouseCapture();

            this.Model.Track.RemoveClip(this.Model);

            owner.IsSelected = true;
            owner.Focus();
            owner.clickPos = this.clickPos;
            owner.clickPosAbs = this.clickPosAbs;
            owner.SetDragState(DragState.DragBody);
            if (!this.IsMouseCaptured) {
                owner.CaptureMouse();
            }
        }

        private void PlaceThisPhantomClipIntoTrackForReal() {
            Clip phantomModel = this.Model;
            Track phantomTrack = phantomModel.Track;
            TimelineTrackControl phantomTrackControl = this.Track;
            TimelineClipControl owner = this.dragCopyOwnerControl;

            FrameSpan targetSpan = phantomModel.FrameSpan;

            this.SetDragState(DragState.None);
            this.SetCursorForMousePoint(Mouse.GetPosition(this));
            this.ReleaseMouseCapture();

            this.dragCopyOwnerControl = null;

            int index = phantomTrack.Clips.IndexOf(phantomModel);
            if (index == -1) {
                throw new Exception("WTF");
            }

            Clip realClone = owner.Model.Clone();
            realClone.FrameSpan = targetSpan;

            phantomTrack.RemoveClipAt(index);
            phantomTrack.InsertClip(index, realClone);

            TimelineClipControl newSelf = phantomTrackControl.GetClipAt(index);
            newSelf.IsSelected = true;
            newSelf.Focus();
        }

        #endregion

        #region Measure, Arrange and Render

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

            if (!this.IsClipVisible) {
                dc.DrawRectangle(Brushes.Gray, null, rect);
            }
            else if (this.Background is Brush background) {
                dc.DrawRectangle(background, null, rect);
            }

            Rect headerRect = new Rect(0, 0, rect.Width, Math.Min(rect.Height, HeaderSize));
            if (!this.IsClipVisible) {
                dc.DrawRectangle(Brushes.DarkRed, null, headerRect);
            }
            else if (this.Track?.TrackColourBrush is Brush headerBrush) {
                dc.DrawRectangle(headerBrush, null, headerRect);
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

            if (this.Model.MediaFrameOffset > 0) {
                double pixelX = TimelineUtils.FrameToPixel(this.Model.MediaFrameOffset, this.TimelineZoom);
                dc.DrawRectangle(Brushes.White, null, new Rect(pixelX, 0, 1, 5));
            }

            dc.Pop();

            // Pen pen = new Pen(Brushes.Black, 1d);
            // dc.DrawLine(pen, new Point(0d, 0d), new Point(0d, rect.Height));
            // dc.DrawLine(pen, new Point(rect.Width - 1d, 0d), new Point(rect.Width - 1d, rect.Height));
        }

        public bool IsClipVisible { get; private set; }

        #endregion

        #region Drag dropping items into this clip

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
            if (inputEffects != EnumDropType.None && this.Model is Clip target) {
                if (e.Data.GetData(ResourceDropRegistry.ResourceDropType) is List<BaseResource> resources) {
                    if (resources.Count == 1 && resources[0] is ResourceItem) {
                        outputEffects = ClipDropRegistry.DropRegistry.CanDrop(target, resources[0], inputEffects);
                    }
                }
                else if (e.Data.GetData(EffectProviderListBox.EffectProviderDropType) is EffectProviderEntry provider) {
                    outputEffects = ClipDropRegistry.DropRegistry.CanDrop(target, provider, inputEffects);
                }
                else {
                    outputEffects = ClipDropRegistry.DropRegistry.CanDropNative(target, new DataObjectWrapper(e.Data), inputEffects);
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

        protected override async void OnDrop(DragEventArgs e) {
            base.OnDrop(e);
            e.Handled = true;
            if (this.isProcessingAsyncDrop || !(this.Model is Clip clip)) {
                return;
            }

            EnumDropType effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Effects == DragDropEffects.None) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (e.Data.GetData(ResourceDropRegistry.ResourceDropType) is List<BaseResource> items && items.Count == 1 && items[0] is ResourceItem) {
                    await ClipDropRegistry.DropRegistry.OnDropped(clip, items[0], effects);
                }
                else if (e.Data.GetData(EffectProviderListBox.EffectProviderDropType) is EffectProviderEntry provider) {
                    await ClipDropRegistry.DropRegistry.OnDropped(clip, provider, effects);
                }
                else {
                    await ClipDropRegistry.DropRegistry.OnDroppedNative(clip, new DataObjectWrapper(e.Data), effects);
                }
            }
            finally {
                this.isProcessingAsyncDrop = false;
                this.IsDroppableTargetOver = false;
            }
        }

        #endregion

        public void OnZoomChanged(double newZoom) {
            // this.InvalidateMeasure();
            // if (this.Content is TimelineClipContent content) {
            //     content.InvalidateMeasure();
            // }

            this.AutomationEditor.UnitZoom = newZoom;
        }

        private enum DragState {
            None,
            Initiated,
            DragBody,
            DragLeftGrip,
            DragRightGrip
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