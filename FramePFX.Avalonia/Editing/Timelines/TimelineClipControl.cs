// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Reactive;
using Avalonia.Threading;
using FramePFX.Avalonia.Editing.Automation;
using FramePFX.Avalonia.Editing.Timelines.Selection;
using FramePFX.BaseFrontEnd.AdvancedMenuService;
using FramePFX.BaseFrontEnd.Bindings;
using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.ContextRegistries;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines;

public class TimelineClipControl : ContentControl, IClipElement {
    public static readonly DirectProperty<TimelineClipControl, Clip?> ClipModelProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, Clip?>(nameof(ClipModel), o => o.ClipModel);
    public static readonly DirectProperty<TimelineClipControl, FrameSpan> FrameSpanProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, FrameSpan>(nameof(FrameSpan), o => o.FrameSpan);
    public static readonly DirectProperty<TimelineClipControl, long> FrameBeginProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, long>(nameof(FrameBegin), o => o.FrameBegin);
    public static readonly DirectProperty<TimelineClipControl, long> FrameDurationProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, long>(nameof(FrameDuration), o => o.FrameDuration);
    public static readonly DirectProperty<TimelineClipControl, string?> DisplayNameProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, string?>(nameof(DisplayName), o => o.DisplayName);
    public static readonly DirectProperty<TimelineClipControl, bool> IsSelectedProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, bool>(nameof(IsSelected), o => o.IsSelected);
    public static readonly DirectProperty<TimelineClipControl, bool> IsDroppableTargetOverProperty = AvaloniaProperty.RegisterDirect<TimelineClipControl, bool>(nameof(IsDroppableTargetOver), o => o.IsDroppableTargetOver);
    public static readonly StyledProperty<AutomationSequence?> ActiveSequenceProperty = AvaloniaProperty.Register<TimelineClipControl, AutomationSequence?>(nameof(ActiveSequence));

    public Clip? ClipModel {
        get => this.myClip;
        private set => this.SetAndRaise(ClipModelProperty, ref this.myClip, value);
    }

    public string? DisplayName {
        get => this.myDisplayName;
        private set => this.SetAndRaise(DisplayNameProperty, ref this.myDisplayName, value);
    }

    public bool IsSelected => this.isSelected;

    public bool IsDroppableTargetOver {
        get => this.isDroppableTargetOver;
        private set => this.SetAndRaise(IsDroppableTargetOverProperty, ref this.isDroppableTargetOver, value);
    }

    public AutomationSequence? ActiveSequence {
        get => this.GetValue(ActiveSequenceProperty);
        set => this.SetValue(ActiveSequenceProperty, value);
    }

    public ClipStoragePanel? StoragePanel { get; private set; }

    public TimelineTrackControl? Track => this.StoragePanel?.TrackControl;

    public bool IsConnected { get; private set; }

    public FrameSpan FrameSpan {
        get => this.myFrameSpan;
        private set {
            FrameSpan oldSpan = this.myFrameSpan;
            if (oldSpan == value)
                return;

            this.myFrameSpan = value;

            this.RaisePropertyChanged(FrameSpanProperty, oldSpan, value);
            if (oldSpan.Begin != value.Begin)
                this.RaisePropertyChanged(FrameBeginProperty, oldSpan.Begin, value.Begin);

            if (oldSpan.Duration != value.Duration) {
                this.RaisePropertyChanged(FrameDurationProperty, oldSpan.Duration, value.Duration);
                if (this.PART_AutomationEditor != null)
                    this.PART_AutomationEditor.FrameDuration = value.Duration;
            }

            // TODO: InvalidateMeasure for moving a clip is laggy. It's not horribly slow, but there's noticeable lag
            this.InvalidateMeasure();
            
            if (this.IsConnected && this.StoragePanel!.IsConnected)
                this.StoragePanel.TrackControl!.OnClipSpanChanged();
        }
    }

    public long FrameBegin => this.myFrameSpan.Begin;

    public long FrameDuration => this.myFrameSpan.Duration;

    public double TimelineZoom => this.Track?.TimelineControl?.Zoom ?? 1.0;

    public double PixelBegin => this.FrameBegin * this.TimelineZoom;

    public double PixelWidth => this.FrameDuration * this.TimelineZoom;

    public bool IsClipEnabled { get; private set; }

    private Clip? myClip;
    private FrameSpan myFrameSpan;
    private string? myDisplayName;
    private bool isSelected;
    private bool isDroppableTargetOver;

    private readonly IBinder<Clip> frameSpanBinder = new AutoUpdateAndEventPropertyBinder<Clip>(nameof(VideoClip.FrameSpanChanged), obj => ((TimelineClipControl) obj.Control).FrameSpan = obj.Model.FrameSpan, null);
    private readonly IBinder<Clip> displayNameBinder = new AutoUpdateAndEventPropertyBinder<Clip>(DisplayNameProperty, nameof(VideoClip.DisplayNameChanged), obj => ((TimelineClipControl) obj.Control).DisplayName = obj.Model.DisplayName, null);
    private readonly IBinder<Clip> activeAutoSequenceBinder = new AutoUpdateAndEventPropertyBinder<Clip>(ActiveSequenceProperty, nameof(VideoClip.ActiveSequenceChanged), obj => ((TimelineClipControl) obj.Control).ActiveSequence = obj.Model.ActiveSequence, obj => obj.Model.ActiveSequence = ((TimelineClipControl) obj.Control).ActiveSequence);
    private readonly PropertyBinder<AutomationSequence?> autoSequenceBinder;
    private readonly RectangleGeometry renderSizeRectGeometry;
    private const double EdgeGripSize = 8d;
    public const double HeaderSize = FramePFX.Editing.Timelines.Tracks.Track.MinimumHeight;

    internal IPointer? initiatedDragPointer;
    private DragState dragState;
    private ClipPart clickedPart;
    private Track? trackAtDragBegin;
    private FrameSpan spanAtDragBegin;
    private Point leftClickPos;
    private PixelPoint lastMovePosAbs;
    private bool hasMadeRangeSelectionInMousePress;
    private bool shouldUpdatePlayHeadOnMouseUp;
    private bool isMovingBetweenTracks;
    private bool wasSelectedOnPress;
    private bool isProcessingAsyncDrop;
    private FormattedText? myDisplayNameFormattedText;
    private AutomationSequenceEditorControl? PART_AutomationEditor;
    private Border? PART_ClipBorderRoot;

    public TimelineClipControl() {
        Binders.AttachControls(this, this.frameSpanBinder, this.displayNameBinder, this.activeAutoSequenceBinder);
        this.renderSizeRectGeometry = new RectangleGeometry();
        DataManager.GetContextData(this).Set(DataKeys.ClipUIKey, this);
        DragDrop.SetAllowDrop(this, true);
        this.autoSequenceBinder = new PropertyBinder<AutomationSequence?>(this, ActiveSequenceProperty, AutomationSequenceEditorControl.AutomationSequenceProperty);

        // Compositor.TryGetDefaultCompositor().create
        // Testing render offsets
        // this.Content = new Rectangle() {
        //     Fill = Brushes.OrangeRed,
        //     Stroke = Brushes.DeepSkyBlue,
        //     StrokeThickness = 1
        // };
    }

    static TimelineClipControl() {
        AffectsRender<TimelineClipControl>(BackgroundProperty, DisplayNameProperty);
        DisplayNameProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<string?>>(OnDisplayNameChanged));
        DragDrop.DragEnterEvent.AddClassHandler<TimelineClipControl>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<TimelineClipControl>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<TimelineClipControl>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<TimelineClipControl>((o, e) => o.OnDrop(e));
    }

    private static void OnDisplayNameChanged(AvaloniaPropertyChangedEventArgs<string?> obj) {
        ((TimelineClipControl) obj.Sender).myDisplayNameFormattedText = null;
    }

    private void UpdateIsClipVisibleState() {
        bool isEnabled = !(this.ClipModel is VideoClip clip) || VideoClip.IsEnabledParameter.GetCurrentValue(clip);
        if (isEnabled != this.IsClipEnabled) {
            this.IsClipEnabled = isEnabled;
            this.InvalidateVisual();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_AutomationEditor = e.NameScope.GetTemplateChild<AutomationSequenceEditorControl>("PART_AutomationEditor");
        this.PART_AutomationEditor.HorizontalZoom = this.TimelineZoom;
        this.PART_AutomationEditor.FrameDuration = this.FrameDuration;
        this.PART_ClipBorderRoot = e.NameScope.GetTemplateChild<Border>("PART_ClipBorderRoot");
        this.autoSequenceBinder.SetTargetControl(this.PART_AutomationEditor);
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, ClipContextRegistry.Registry);
        Dispatcher.UIThread.InvokeAsync(() => this.isMovingBetweenTracks = false, DispatcherPriority.Send);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        this.renderSizeRectGeometry.Rect = new Rect(e.NewSize);
        this.InvalidateVisual();
    }

    public void OnConnecting(ClipStoragePanel storagePanel, Clip clip) {
        this.StoragePanel = storagePanel;
        this.ClipModel = clip;
        DataManager.GetContextData(this).Set(DataKeys.ClipKey, clip);
    }

    public void OnConnected() {
        this.IsConnected = true;
        Binders.AttachModels(this.ClipModel!, this.frameSpanBinder, this.displayNameBinder, this.activeAutoSequenceBinder);
        if (this.ClipModel is VideoClip videoClip) {
            videoClip.AutomationData.AddParameterChangedHandler(VideoClip.IsEnabledParameter, this.OnIsVisibleChanged);
        }

        this.UpdateIsClipVisibleState();
    }

    public void OnDisconnecting() {
        Binders.DetachModels(this.frameSpanBinder, this.displayNameBinder, this.activeAutoSequenceBinder);
        if (this.ClipModel is VideoClip videoClip) {
            videoClip.AutomationData.RemoveParameterChangedHandler(VideoClip.IsEnabledParameter, this.OnIsVisibleChanged);
        }
    }

    public void OnDisconnected() {
        this.IsConnected = false;
        DataManager.GetContextData(this).Set(DataKeys.ClipKey, null);
    }

    private void OnIsVisibleChanged(AutomationSequence sequence) {
        this.UpdateIsClipVisibleState();
    }

    #region Drag Move

    private enum DragState {
        None,
        Initiated,
        DragHeader,
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

    private ClipPart GetPartForPoint(Point mPos) {
        if (mPos.Y <= HeaderSize) {
            if (mPos.X <= EdgeGripSize) {
                return ClipPart.LeftGrip;
            }
            else if (mPos.X >= (this.Bounds.Width - EdgeGripSize)) {
                return ClipPart.RightGrip;
            }
            else {
                return ClipPart.Header;
            }
        }
        else {
            Size size = this.Bounds.Size;
            if (mPos.X < 0 || mPos.Y < 0 || mPos.X > size.Width || mPos.Y > size.Height)
                return ClipPart.None;
            return ClipPart.Body;
        }
    }

    private void SetCursorForMousePoint(Point mPos) {
        ClipPart part = this.GetPartForPoint(mPos);
        switch (part) {
            case ClipPart.None:
            case ClipPart.Body:
                this.SetCursorForDragState(DragState.None, true);
            break;
            case ClipPart.Header:    this.SetCursorForDragState(DragState.DragHeader, true); break;
            case ClipPart.LeftGrip:  this.SetCursorForDragState(DragState.DragLeftGrip, true); break;
            case ClipPart.RightGrip: this.SetCursorForDragState(DragState.DragRightGrip, true); break;
            default:                 throw new ArgumentOutOfRangeException();
        }
    }

    private void SetDragState(DragState state) {
        if (this.dragState != state) {
            if (state < DragState.Initiated) {
                this.initiatedDragPointer = null;
                this.clickedPart = ClipPart.None;
            }

            this.dragState = state;
            this.SetCursorForDragState(state, false);
        }
    }

    private void SetCursorForDragState(DragState state, bool isPreview) {
        if (isPreview && this.dragState != DragState.None) {
            return;
        }

        switch (state) {
            case DragState.None:       this.ClearValue(CursorProperty); break;
            case DragState.Initiated:  break;
            case DragState.DragHeader: this.Cursor = new Cursor(StandardCursorType.SizeAll); break;
            case DragState.DragLeftGrip:
            case DragState.DragRightGrip:
                this.Cursor = new Cursor(StandardCursorType.SizeWestEast);
            break;
            default: throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    public static long GetCursorFrame(TimelineClipControl clip, PointerEventArgs e, bool useRounding = true) {
        TrackStoragePanel? timeline = clip.Track?.TrackStoragePanel;
        if (timeline == null) {
            throw new Exception("Clip does not have a timeline sequence associated with it");
        }

        return GetCursorFrame(timeline, e, useRounding);
    }

    public static long GetCursorFrame(TrackStoragePanel trackStoragePanel, PointerEventArgs e, bool useRounding = true) {
        double cursor = e.GetPosition(trackStoragePanel).X;
        return TimelineUtils.PixelToFrame(cursor, trackStoragePanel.TimelineControl?.Zoom ?? 1.0, useRounding);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (this.ClipModel == null)
            return;

        bool isRightClick = point.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed;
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed && !isRightClick)
            return;

        Timeline? timeline;
        TimelineControl? timelineControl = this.Track?.TimelineControl;
        if (timelineControl == null || (timeline = timelineControl.Timeline) == null)
            return;

        if (isRightClick) {
            e.Handled = true;
            this.Focus();
            this.lastMovePosAbs = this.PointToScreen(this.leftClickPos);
            if (!ReferenceEquals(e.Pointer.Captured, this)) {
                e.Pointer.Capture(this);
            }

            if (!this.IsSelected) {
                timelineControl.ClipSelectionManager!.SetSelection(this);
            }
        }
        else {
            this.spanAtDragBegin = this.ClipModel.FrameSpan;
            this.trackAtDragBegin = this.ClipModel.Track;

            e.Handled = true;
            this.Focus();
            this.leftClickPos = e.GetPosition(this);
            this.lastMovePosAbs = this.PointToScreen(this.leftClickPos);
            this.clickedPart = this.GetPartForPoint(this.leftClickPos);
            if (this.clickedPart > ClipPart.Body) {
                this.initiatedDragPointer = e.Pointer;
                this.SetDragState(DragState.Initiated);
            }

            if (!ReferenceEquals(e.Pointer.Captured, this)) {
                e.Pointer.Capture(this);
            }

            long mouseFrame = GetCursorFrame(this, e);
            bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
            if ((e.KeyModifiers & KeyModifiers.Shift) != 0) {
                this.hasMadeRangeSelectionInMousePress = true;
                TrackPoint anchor = timeline.RangedSelectionAnchor;
                if (anchor.TrackIndex != -1) {
                    int idxA = anchor.TrackIndex;
                    int idxB = this.ClipModel.Track!.IndexInTimeline;
                    if (idxA > idxB) {
                        Maths.Swap(ref idxA, ref idxB);
                    }

                    long frameA = anchor.Frame;
                    if (frameA > mouseFrame) {
                        Maths.Swap(ref frameA, ref mouseFrame);
                    }

                    timelineControl.MakeFrameRangeSelection(FrameSpan.FromIndex(frameA, mouseFrame), idxA, idxB + 1);
                }
                else {
                    long frameA = timeline.PlayHeadPosition;
                    if (frameA > mouseFrame) {
                        Maths.Swap(ref frameA, ref mouseFrame);
                    }

                    timelineControl.MakeFrameRangeSelection(FrameSpan.FromIndex(frameA, mouseFrame));
                }
            }
            else {
                this.wasSelectedOnPress = this.IsSelected;
                if (isToggle) {
                    if (this.wasSelectedOnPress) {
                        // do nothing; toggle selection in mouse release
                    }
                    else {
                        timelineControl.ClipSelectionManager!.Select(this);
                    }
                }
                else if (!this.wasSelectedOnPress || timelineControl.ClipSelectionManager!.Count <= 1) {
                    // Check we don't update selection since we are already the only selected item. Pointless re-selection
                    if (!this.wasSelectedOnPress)
                        timelineControl.ClipSelectionManager!.SetSelection(this);
                    this.shouldUpdatePlayHeadOnMouseUp = this.clickedPart != ClipPart.Header;
                }

                timeline.RangedSelectionAnchor = new TrackPoint(this.ClipModel, mouseFrame);
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        if (this.ClipModel == null || e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased) {
            return;
        }

        e.Handled = true;
        DragState lastDragState = this.dragState;
        bool updatePlayHeadOnMouseUp = this.shouldUpdatePlayHeadOnMouseUp;
        this.shouldUpdatePlayHeadOnMouseUp = false;
        if (this.dragState <= DragState.Initiated && updatePlayHeadOnMouseUp) {
            this.Track!.TrackStoragePanel!.SetPlayHeadToMouseCursor(e);
        }

        this.SetDragState(DragState.None);
        this.SetCursorForMousePoint(e.GetPosition(this));
        if (ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(null);

        // If we made a range selection then don't do anything with the current selection
        if (this.hasMadeRangeSelectionInMousePress) {
            this.hasMadeRangeSelectionInMousePress = false;
            return;
        }

        Timeline? timeline;
        TimelineControl? timelineControl = this.Track?.TimelineControl;
        if (timelineControl == null || (timeline = timelineControl.Timeline) == null) {
            return;
        }

        TimelineClipSelectionManager selector = timelineControl.ClipSelectionManager!;
        if (lastDragState == DragState.None || lastDragState == DragState.Initiated) {
            bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
            int selCount = selector.Count;
            if ((e.KeyModifiers & KeyModifiers.Shift) == 0 && (!isToggle || (selCount == 1 && selector.IsSelected(this)))) {
                timeline.RangedSelectionAnchor = new TrackPoint(this.ClipModel, GetCursorFrame(this, e));
            }

            if (selCount == 0) {
                // very rare scenario, shouldn't really occur
                selector.SetSelection(this);
            }
            else if (isToggle && this.wasSelectedOnPress) {
                // Check we want to toggle, check we were selected on click and we probably are still selected,
                // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                selector.Unselect(this);
            }
            else if (selCount > 1 && !isToggle) {
                selector.SetSelection(this);
            }
        }
    }

    private void SetClipSpanForDrag(FrameSpan newSpan) {
        RenderManager? manager = this.ClipModel!.Timeline?.RenderManager;
        if (manager != null) {
            // Signal to use slow render dispatch because of how
            // often mouse movements are that will lag the UI
            using (manager.UseSlowRenderDispatch())
                this.ClipModel!.FrameSpan = newSpan;
        }
        else {
            this.ClipModel!.FrameSpan = newSpan;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (this.ClipModel == null) {
            return;
        }

        TrackStoragePanel? trackList = this.Track?.TrackStoragePanel;
        if (trackList == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        Point mPos = e.GetPosition(this);

        // This is used to prevent "drag jumping" which occurs when a screen pixel is
        // somewhere in between a frame in a sweet spot that results in the control
        // jumping back and forth. To test, do CTRL+MouseWheelUp once to zoom in a bit,
        // and then drag a clip 1 frame at a time and you might see it with the code below removed.
        // This code is pretty much the exact same as what Thumb uses
        PixelPoint mPosAbs = this.PointToScreen(mPos);
        // don't care about the Y pos :P

        bool hasMovedX = !DoubleUtils.AreClose(mPosAbs.X, this.lastMovePosAbs.X);
        bool hasMovedY = !DoubleUtils.AreClose(mPosAbs.Y, this.lastMovePosAbs.Y);
        if (!hasMovedX && !hasMovedY) {
            return;
        }

        this.lastMovePosAbs = mPosAbs;

        if (!point.Properties.IsLeftButtonPressed) {
            this.SetDragState(DragState.None);
            this.SetCursorForMousePoint(mPos);
            e.Pointer.Capture(null);
            return;
        }

        this.SetCursorForMousePoint(mPos);
        if (!this.IsConnected) {
            return;
        }

        const int MinimumHorizontalDragDistance = 2;
        const int MinimumVerticalDragDistance = 2;

        if (hasMovedX && this.dragState == DragState.Initiated) {
            const double minDragX = MinimumHorizontalDragDistance;
            const double minDragY = MinimumVerticalDragDistance;
            if (Math.Abs(mPos.X - this.leftClickPos.X) < minDragX && Math.Abs(mPos.Y - this.leftClickPos.Y) < minDragY) {
                return;
            }

            if ((e.KeyModifiers & KeyModifiers.Control) != 0) {
                // TODO: phantom drag
                return;
            }

            ClipPart part = this.GetPartForPoint(this.leftClickPos);
            switch (part) {
                case ClipPart.Header:    this.SetDragState(DragState.DragHeader); break;
                case ClipPart.LeftGrip:  this.SetDragState(DragState.DragLeftGrip); break;
                case ClipPart.RightGrip: this.SetDragState(DragState.DragRightGrip); break;
            }
        }
        else if (this.dragState == DragState.None) {
            return;
        }

        double zoom = this.TimelineZoom;
        Vector mPosDifRel = mPos - this.leftClickPos;

        // Vector mPosDiff = absMpos - this.screenClickPos;
        FrameSpan oldSpan = this.ClipModel.FrameSpan;
        if (this.dragState == DragState.DragHeader) {
            if (hasMovedX) {
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
                        if (newEndIndex > trackList.Timeline!.MaxDuration) {
                            trackList.Timeline.TryExpandForFrame(newEndIndex);
                        }

                        this.SetClipSpanForDrag(newSpan);
                        // this.SetClipSpanForDrag(new FrameSpan(Maths.Clamp(TimelineUtils.PixelToFrame(mPos.X, this.TimelineZoom), 0, trackList.Timeline.MaxDuration - oldSpan.Duration), oldSpan.Duration));
                    }
                }
            }

            if (hasMovedY && !this.isMovingBetweenTracks && Math.Abs(mPosDifRel.Y) >= 1.0d) {
                double totalHeight = 0.0;
                List<TimelineTrackControl> tracks = new List<TimelineTrackControl>(trackList.GetTracks());
                Point mPosTL = e.GetPosition(trackList);
                for (int i = 0, endIndex = tracks.Count - 1; i <= endIndex; i++) {
                    TimelineTrackControl track = tracks[i];
                    if (DoubleUtils.GreaterThanOrClose(mPosTL.Y, totalHeight) && DoubleUtils.LessThanOrClose(mPosTL.Y, totalHeight + track.Bounds.Height)) {
                        Track? newTrack = track.Track;
                        if (newTrack != null && !ReferenceEquals(this.ClipModel.Track, newTrack) && newTrack.IsClipTypeAccepted(this.ClipModel.GetType())) {
                            this.isMovingBetweenTracks = true;
                            this.ClipModel.MoveToTrack(newTrack);
                            break;
                        }
                    }

                    // 1.0 includes the gap between tracks
                    totalHeight += track.Bounds.Height + 1.0;
                }
            }
        }
        else if (this.dragState == DragState.DragLeftGrip || this.dragState == DragState.DragRightGrip) {
            if (!hasMovedX || !(Math.Abs(mPosDifRel.X) >= 1.0d)) {
                return;
            }

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
                    trackList.Timeline!.TryExpandForFrame(newSpan.EndIndex);
                    if (this.ClipModel.IsMediaFrameSensitive) {
                        this.ClipModel.MediaFrameOffset += (oldSpan.Begin - newSpan.Begin);
                        this.SetClipSpanForDrag(newSpan);
                        // if (this.ClipModel.MediaFrameOffset > 0)
                        // {
                        //     if (this.ClipModel.IsMediaFrameSensitive)
                        //         this.ClipModel.MediaFrameOffset += (oldSpan.Begin - newSpan.Begin);
                        // }
                        // else
                        // {
                        //     long posChange = (newSpan.Begin - oldSpan.Begin);
                        //     long extraMediaFrame = (oldSpan.Begin - newSpan.Begin);
                        //     long subFromSpan = -(this.ClipModel.MediaFrameOffset + extraMediaFrame);
                        //     if (subFromSpan < 0)
                        //     {
                        //         this.SetClipSpanForDrag(new FrameSpan(newSpan.Begin - subFromSpan, oldSpan.Duration));
                        //     }
                        //     else
                        //     {
                        //         this.ClipModel.MediaFrameOffset += extraMediaFrame;
                        //         this.SetClipSpanForDrag(newSpan);
                        //     }
                        // }
                    }
                    else {
                        this.SetClipSpanForDrag(newSpan);
                    }
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
                    trackList.Timeline!.TryExpandForFrame(newEndIndex);
                    this.SetClipSpanForDrag(newSpan);

                    // account for there being no "grip" control aligned to the right side;
                    // since the clip is resized, the origin point will not work correctly and
                    // results in an exponential endIndex increase unless the below code is used.
                    // This code is not needed for the left grip because it just naturally isn't
                    this.leftClickPos = new Point(this.leftClickPos.X + (newSpan.EndIndex - oldSpan.EndIndex) * zoom, this.leftClickPos.Y);
                }
            }
        }
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (!e.Handled && this.dragState > DragState.Initiated) {
            if (e.Key == Key.Escape) {
                this.SetDragState(DragState.None);
                this.SetCursorForMousePoint(this.lastMovePosAbs.ToPoint(1.0));
                this.initiatedDragPointer?.Capture(null);

                // it better not be null!!!
                if (this.trackAtDragBegin != null) {
                    this.ClipModel!.FrameSpan = this.spanAtDragBegin;
                    if (!ReferenceEquals(this.ClipModel.Track, this.trackAtDragBegin)) {
                        this.ClipModel.MoveToTrack(this.trackAtDragBegin);
                    }

                    this.trackAtDragBegin = null;
                }
            }
        }
    }

    #endregion

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

    public override void Render(DrawingContext dc) {
        base.Render(dc);
        Rect finalRect = new Rect(default, this.Bounds.Size);
        if (this.renderSizeRectGeometry.Rect != finalRect) {
            this.renderSizeRectGeometry.Rect = finalRect;
        }

        using (dc.PushGeometryClip(this.renderSizeRectGeometry)) {
            using (dc.PushRenderOptions(new RenderOptions() { EdgeMode = EdgeMode.Aliased, TextRenderingMode = TextRenderingMode.Antialias })) {
                if (!this.IsClipEnabled) {
                    dc.DrawRectangle(Brushes.Gray, null, finalRect);
                }
                else if (this.Background is Brush background) {
                    dc.DrawRectangle(background, null, finalRect);
                }

                Thickness border = this.PART_ClipBorderRoot!.BorderThickness;
                Thickness border2 = new Thickness(border.Left, 0, border.Right, 0);

                Rect headerRect = new Rect(finalRect.X, finalRect.Y, finalRect.Width, Math.Min(finalRect.Height, HeaderSize)).Deflate(border2);
                if (!this.IsClipEnabled) {
                    dc.DrawRectangle(Brushes.DarkRed, null, headerRect);
                }
                else if (this.Track?.ClipHeaderBrush is Brush headerBrush) {
                    dc.DrawRectangle(headerBrush, null, headerRect);
                }

                if (this.DisplayName is string str && !string.IsNullOrWhiteSpace(str)) {
                    IBrush foreground = this.Track?.TrackColourForegroundBrush ?? this.Foreground ?? Brushes.Orange;
                    this.myDisplayNameFormattedText ??= new FormattedText(str, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 12, foreground);

                    // Need to redraw when scrolling. Could do this but we have to draw everything else...
                    // ScrollViewer? scroller = this.Track?.TimelineControl?.TimelineScrollViewer;
                    // Point location;
                    // if (scroller == null) {
                    //     location = new Point(3, 1);
                    // }
                    // else {
                    //     Point a = scroller.TranslatePoint(default, this) ?? default;
                    //     location = new Point(Math.Max(a.X, 3), 1);
                    // }

                    dc.DrawText(this.myDisplayNameFormattedText, new Point(3, 1));
                }

                if (this.ClipModel?.MediaFrameOffset > 0) {
                    double pixelX = TimelineUtils.FrameToPixel(this.ClipModel.MediaFrameOffset, this.TimelineZoom);
                    dc.DrawRectangle(Brushes.White, null, new Rect(pixelX, 0, 1, 5));
                }

                // IBrush? outlineBrush = this.PART_ClipBorderRoot!.BorderBrush;// this.IsSelected ? Brushes.YellowGreen : this.BorderBrush;
                // if (outlineBrush != null) {
                //     Thickness border = this.IsSelected ? new Thickness(2) : new Thickness(1,0);
                //     if (border.IsUniform) {
                //         dc.DrawRectangle(new Pen(outlineBrush, border.Left), finalRect.Deflate(border.Left / 2.0));
                //     }
                //     else {
                //         if (!DoubleUtils.AreClose(border.Left, 0.0))
                //             dc.DrawLine(new Pen(outlineBrush, border.Left), new Point(finalRect.X + (border.Left / 2.0) , 0), new Point(finalRect.Y + (border.Left / 2.0), finalRect.Height));
                //         if (!DoubleUtils.AreClose(border.Right, 0.0))
                //             dc.DrawLine(new Pen(outlineBrush, border.Right), new Point(finalRect.Width - (border.Right / 2.0) , 0), new Point(finalRect.Width - (border.Right / 2.0), finalRect.Height));
                //     }
                // }
            }
        }
    }

    public void OnZoomChanged(double newZoom) {
        if (this.PART_AutomationEditor != null)
            this.PART_AutomationEditor.HorizontalZoom = newZoom;
    }

    internal static void InternalUpdateIsSelected(TimelineClipControl clip, bool isSelected) {
        clip.SetAndRaise(IsSelectedProperty, ref clip.isSelected, isSelected);
        clip.ZIndex = isSelected ? 10 : 1;
    }

    ITrackElement IClipElement.TrackUI => this.Track?.TrackElement ?? throw GetInvalidElementException();

    Clip IClipElement.Clip => this.ClipModel ?? throw GetInvalidElementException();

    #region Drag dropping items into this clip

    protected void OnDragEnter(DragEventArgs e) {
        this.OnDragOver(e);
    }

    protected void OnDragOver(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop) {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        EnumDropType outputEffects = EnumDropType.None;
        EnumDropType inputEffects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (inputEffects != EnumDropType.None && this.ClipModel is Clip target) {
            if (e.Data.Get(ResourceDropRegistry.DropTypeText) is List<BaseResource> resources) {
                if (resources.Count == 1 && resources[0] is ResourceItem) {
                    outputEffects = ClipDropRegistry.DropRegistry.CanDrop(target, resources[0], inputEffects);
                }
            }
            // else if (e.Data.GetData(EffectProviderListBox.EffectProviderDropType) is EffectProviderEntry provider) {
            //     outputEffects = ClipDropRegistry.DropRegistry.CanDrop(target, provider, inputEffects);
            // }
            else {
                outputEffects = ClipDropRegistry.DropRegistry.CanDropNative(target, new DataObjectWrapper(e.Data), inputEffects);
            }

            if (outputEffects != EnumDropType.None) {
                this.OnAcceptDrop();
                e.DragEffects = (DragDropEffects) outputEffects;
            }
            else {
                this.IsDroppableTargetOver = false;
                e.DragEffects = DragDropEffects.None;
            }
        }
    }

    private void OnAcceptDrop() {
        if (!this.IsDroppableTargetOver)
            this.IsDroppableTargetOver = true;
    }

    protected void OnDragLeave(DragEventArgs e) {
        // Dispatcher.UIThread.Invoke(() => this.ClearValue(IsDroppableTargetOverProperty), DispatcherPriority.Loaded);
        if (!this.IsPointerOver) {
            this.IsDroppableTargetOver = false;
        }
    }

    protected async void OnDrop(DragEventArgs e) {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.ClipModel is Clip clip)) {
            return;
        }

        EnumDropType effects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        if (e.DragEffects == DragDropEffects.None) {
            return;
        }

        try {
            this.isProcessingAsyncDrop = true;
            if (e.Data.Get(ResourceDropRegistry.DropTypeText) is List<BaseResource> items && items.Count == 1 && items[0] is ResourceItem) {
                await ClipDropRegistry.DropRegistry.OnDropped(clip, items[0], effects);
            }
            // else if (e.Data.GetData(EffectProviderListBox.EffectProviderDropType) is EffectProviderEntry provider) {
            //     await ClipDropRegistry.DropRegistry.OnDropped(clip, provider, effects);
            // }
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

    public void OnIsAutomationVisibilityChanged(bool isVisible) {
        this.PART_AutomationEditor!.IsVisible = isVisible;
    }

    private static InvalidOperationException GetInvalidElementException() => new InvalidOperationException("Invalid clip element");
}