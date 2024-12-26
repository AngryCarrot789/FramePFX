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
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FramePFX.Avalonia.AdvancedMenuService;
using FramePFX.Avalonia.Editing.Timelines.Selection;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.UI;
using FramePFX.Utils;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;

public class TrackControlSurfaceItem : ContentControl {
    public static readonly DirectProperty<TrackControlSurfaceItem, Track?> TrackProperty = AvaloniaProperty.RegisterDirect<TrackControlSurfaceItem, Track?>(nameof(Track), o => o.Track);
    public static readonly DirectProperty<TrackControlSurfaceItem, bool> IsSelectedProperty = AvaloniaProperty.RegisterDirect<TrackControlSurfaceItem, bool>(nameof(IsSelected), o => o.IsSelected);

    private Track? myTrack;
    private bool wasFocusedBeforeMoving;
    private bool internalIsSelected;
    private Point clickPos;
    private PixelPoint lastMovePosAbs;
    private bool wasSelectedOnPress;
    private DragState dragState;
    internal IPointer? initiatedDragPointer;
    private bool isMovingBetweenTracks, hasMovedTrackExFlag;
    private ContentPresenter? PART_ContentPresenter;

    public Track? Track {
        get => this.myTrack;
        private set => this.SetAndRaise(TrackProperty, ref this.myTrack, value);
    }

    // public int IndexInList { get; internal set; } = -1;
    public int IndexInList => this.TrackList!.IndexOf(this);

    public TrackControlSurfaceList? TrackList { get; private set; }

    public bool IsSelected {
        get => this.internalIsSelected;
        private set => this.SetAndRaise(IsSelectedProperty, ref this.internalIsSelected, value);
    }

    public ITrackElement? TrackElement { get; internal set; }

    private static readonly PropertyInfo IsPointerOverPropertyInfo = typeof(InputElement).GetProperty("IsPointerOver")!;

    public TrackControlSurfaceItem() {
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        Dispatcher.UIThread.InvokeAsync(() => this.isMovingBetweenTracks = false, DispatcherPriority.Send);
        AdvancedContextMenu.SetContextRegistry(this, Track.TrackControlSurfaceContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e) {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    static TrackControlSurfaceItem() {
        FocusableProperty.OverrideDefaultValue<TrackControlSurfaceItem>(true);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_ContentPresenter = e.NameScope.GetTemplateChild<ContentPresenter>("PART_ContentPresenter");
    }

    private enum DragState {
        None,
        Initiated,
        Running
    }

    #region Model Connections

    public void OnAddingToList(TrackControlSurfaceList ownerList, Track track, int index) {
        this.Track = track ?? throw new ArgumentNullException(nameof(track));
        this.TrackList = ownerList;
        this.Track.HeightChanged += this.OnTrackHeightChanged;
        this.Content = ownerList.GetContentObject(track);
    }

    public void OnAddedToList() {
        TrackControlSurface control = (TrackControlSurface) this.Content!;
        control.ApplyStyling();
        control.ApplyTemplate();
        control.Connect(this);
        this.Height = this.Track!.Height;
    }

    public void OnRemovingFromList() {
        this.Track!.HeightChanged -= this.OnTrackHeightChanged;
        TrackControlSurface content = (TrackControlSurface) this.Content!;
        content.Disconnect();
        this.Content = null;
        this.TrackList!.ReleaseContentObject(this.Track.GetType(), content);
    }

    public void OnRemovedFromList() {
        this.TrackList = null;
        this.Track = null;

        // WORKAROUND for avalonia's broken pointer over system
        SetPointerNotOver(this);
        foreach (Visual child in this.GetVisualChildren()) {
            if (child is InputElement element) {
                SetPointerNotOver(element);
            }
        }
    }

    #endregion

    private static void SetPointerNotOver(InputElement element) {
        IsPointerOverPropertyInfo.SetValue(element, BoolBox.False);
        ((IPseudoClasses) element.Classes).Remove(":pointerover");
    }

    public void OnIndexMoving(int oldIndex, int newIndex) {
        this.wasFocusedBeforeMoving = this.IsFocused;
    }

    public void OnIndexMoved(int oldIndex, int newIndex) {
        this.Height = this.Track!.Height;
        if (this.wasFocusedBeforeMoving) {
            this.wasFocusedBeforeMoving = false;
            this.Focus();
        }
    }

    private void OnTrackHeightChanged(Track track) {
        this.Height = track.Height;
    }

    private void SetDragState(DragState state) {
        if (this.dragState != state) {
            if (state < DragState.Initiated) {
                this.initiatedDragPointer = null;
            }

            if (state == DragState.Running) {
                this.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
            }
            else {
                this.ClearValue(CursorProperty);
            }

            this.dragState = state;
        }
    }

    private bool IsHitObjectOnUs(object? source) {
        return source != null && (ReferenceEquals(source, this) || ReferenceEquals(source, this.PART_ContentPresenter));
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        if (e.Handled || this.TrackList == null || this.TrackElement == null) {
            return;
        }

        // The mouse didn't click the track area, maybe it clicked
        // the toggle visibility button, so ignore the event
        if (!this.IsHitObjectOnUs(e.Source)) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        TimelineControl? timelineControl = this.TrackList?.TimelineControl;
        if (timelineControl == null || timelineControl.Timeline == null) {
            return;
        }

        this.Focus();
        this.clickPos = e.GetPosition(this);
        this.lastMovePosAbs = this.PointToScreen(this.clickPos);
        this.initiatedDragPointer = e.Pointer;
        this.SetDragState(DragState.Initiated);

        e.Handled = true;
        if (!ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(this);

        bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
        if ((e.KeyModifiers & KeyModifiers.Shift) != 0) {
            this.TrackList!.SelectRange(this, e);
        }
        else {
            this.wasSelectedOnPress = this.IsSelected;
            if (isToggle) {
                if (this.wasSelectedOnPress) {
                    // do nothing; toggle selection in mouse release
                }
                else {
                    timelineControl.TrackSelectionManager!.Select(this.TrackElement);
                }
            }
            else if (timelineControl.ClipSelectionManager!.Count < 2 || !this.wasSelectedOnPress) {
                // Set as only selection if 0 or 1 items selected, or we aren't selected
                timelineControl.TrackSelectionManager!.SetSelection(this.TrackElement);
            }

            this.TrackList!.SetRangeAnchor(this, e);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e) {
        base.OnPointerReleased(e);
        if (e.Handled || this.TrackList == null || this.TrackElement == null) {
            return;
        }

        PointerPoint point = e.GetCurrentPoint(this);
        if (point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased) {
            return;
        }

        e.Handled = true;
        DragState lastDragState = this.dragState;

        this.SetDragState(DragState.None);
        if (ReferenceEquals(e.Pointer.Captured, this))
            e.Pointer.Capture(null);

        TimelineControl? timelineControl = this.TrackList?.TimelineControl;
        if (timelineControl == null || timelineControl.Timeline == null) {
            return;
        }

        TrackSelectionManager selector = timelineControl.TrackSelectionManager!;
        if (lastDragState == DragState.None || lastDragState == DragState.Initiated) {
            bool isToggle = (e.KeyModifiers & KeyModifiers.Control) != 0;
            int selCount = selector.Count;
            if ((e.KeyModifiers & KeyModifiers.Shift) == 0 && (!isToggle || (selCount == 1 && selector.IsSelected(this.TrackElement)))) {
                this.TrackList!.SetRangeAnchor(this, e);
            }

            if (selCount == 0) {
                // very rare scenario, shouldn't really occur
                selector.SetSelection(this.TrackElement);
            }
            else if (isToggle && this.wasSelectedOnPress) {
                // Check we want to toggle, check we were selected on click and we probably are still selected,
                // and also check that the last drag wasn't completed/cancelled just because it feels more normal that way
                selector.Unselect(this.TrackElement);
            }
            else if (selCount > 1 && !isToggle) {
                selector.SetSelection(this.TrackElement);
            }
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        if (e.Handled || this.TrackList == null || this.TrackElement == null) {
            return;
        }

        if (!this.IsHitObjectOnUs(e.Source)) {
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
            e.Pointer.Capture(null);
            return;
        }

        const int MinimumHorizontalDragDistance = 2;
        const int MinimumVerticalDragDistance = 2;

        if (hasMovedX && this.dragState == DragState.Initiated) {
            const double minDragX = MinimumHorizontalDragDistance;
            const double minDragY = MinimumVerticalDragDistance;
            if (Math.Abs(mPos.X - this.clickPos.X) < minDragX && Math.Abs(mPos.Y - this.clickPos.Y) < minDragY) {
                return;
            }

            if ((e.KeyModifiers & KeyModifiers.Control) != 0) {
                // phantom drag
                return;
            }

            this.SetDragState(DragState.Running);
        }
        else if (this.dragState == DragState.None) {
            return;
        }

        // TODO: Even when captured, we need to wait until the mouse cursor comes into view again
        // Have 3 tracks, minimize the middle one and drag between the top and bottom tracks,
        // The middle track you're dragging will glitch around everywhere each mouse move event
        // However it works fine when tracks are all the same height
        Vector mPosDifRel = mPos - this.clickPos;
        if (hasMovedY && !this.isMovingBetweenTracks && Math.Abs(mPosDifRel.Y) >= 1.0d) {
            // This doesn't work so good
            // if (this.hasMovedTrackExFlag && (mPos.X < 0 || mPos.X > this.Bounds.Width || mPos.Y < 0 || mPos.Y > this.Bounds.Height)) {
            //     return;
            // }

            this.hasMovedTrackExFlag = false;
            double totalHeight = 0.0;
            List<TrackControlSurfaceItem> tracks = this.TrackList!.GetTracks().ToList();
            Point mPosTL = e.GetPosition(this.TrackList);
            for (int targetIndex = 0, endIndex = tracks.Count - 1; targetIndex <= endIndex; targetIndex++) {
                TrackControlSurfaceItem targetLocation = tracks[targetIndex];
                if (DoubleUtils.GreaterThanOrClose(mPosTL.Y, totalHeight) && DoubleUtils.LessThanOrClose(mPosTL.Y, totalHeight + targetLocation.Bounds.Height)) {
                    Track? newTrack = targetLocation.Track;
                    if (newTrack != null && !ReferenceEquals(this.Track, newTrack)) {
                        this.isMovingBetweenTracks = true;
                        this.hasMovedTrackExFlag = true;
                        int oldIndex = this.Track!.IndexInTimeline;
                        this.Track!.Timeline!.MoveTrackIndex(oldIndex, targetIndex);
                        break;
                    }
                }

                // 1.0 includes the gap between tracks
                totalHeight += targetLocation.Bounds.Height + 1.0;
            }
        }
    }

    internal static void InternalSetIsSelected(TrackControlSurfaceItem control, bool isSelected) => control.IsSelected = isSelected;

    public void OnIsAutomationVisibilityChanged(bool isVisible) {
        ((TrackControlSurface?) this.Content)?.OnIsAutomationVisibilityChanged(isVisible);
    }
}