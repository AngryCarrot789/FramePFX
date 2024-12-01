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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using FramePFX.Avalonia.Editing.Playheads;
using FramePFX.Avalonia.Editing.Timelines.Selection;
using FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines;

/// <summary>
/// The main control for a timeline in a video editor
/// </summary>
public class TimelineControl : TemplatedControl, ITimelineElement {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TimelineControl, Timeline?>(nameof(Timeline));
    public static readonly DirectProperty<TimelineControl, double> ZoomProperty = AvaloniaProperty.RegisterDirect<TimelineControl, double>(nameof(Zoom), o => o.Zoom, unsetValue: 1.0);

    private readonly ModelControlDictionary<Track, ITrackElement> trackElementMap;

    public IModelControlDictionary<Track, ITrackElement> TrackElementMap => this.trackElementMap;

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public double Zoom {
        get => this.myZoomFactor;
        private set => this.SetAndRaise(ZoomProperty, ref this.myZoomFactor, value);
    }

    /// <summary>
    /// Gets the list box which stores the control surfaces for the tracks
    /// </summary>
    public TrackControlSurfaceList? SurfaceTrackList { get; private set; }

    /// <summary>
    /// Gets the panel which stores all of the true track controls
    /// </summary>
    public TrackStoragePanel? TrackStorage { get; private set; }

    public TimelineScrollableContentGrid? TimelineContentGrid { get; private set; }

    public ScrollViewer? TimelineScrollViewer { get; private set; }

    public ScrollViewer? TrackListScrollViewer { get; private set; }

    public Border? TimelineBorder { get; private set; }

    public Border TimestampBorder { get; private set; }

    public PlayHeadControl? PlayHead { get; private set; }

    public StopHeadControl? StopHead { get; private set; }

    public PlayheadPositionTextControl PlayHeadInfoTextControl { get; private set; }

    /// <summary>
    /// Gets the timeline selection manager. Not null when our templated has been applied, as in, it is fully visible
    /// </summary>
    public TrackSelectionManager? TrackSelectionManager { get; private set; }

    /// <summary>
    /// Gets the special selection manager used for managing selected clips in all tracks
    /// </summary>
    public TimelineClipSelectionManager? ClipSelectionManager { get; private set; }

    public bool HasAnySelectedClips => this.ClipSelectionManager!.Count > 0;

    public IVideoEditorUI VideoEditor { get; set; }

    public IReadOnlyList<ITrackElement> Tracks { get; }

    ISelectionManager<ITrackElement> ITimelineElement.Selection => this.TrackSelectionManager!;

    ISelectionManager<IClipElement> ITimelineElement.ClipSelection => this.ClipSelectionManager!;

    private double myZoomFactor = 1.0;
    private readonly ContextData contextData;
    internal readonly List<TrackElementImpl> myTrackElements;

    // A fake track element implementation, because there's technically 2 controls for a single track (the surface and real track)
    internal class TrackElementImpl : ITrackElement {
        public readonly TimelineControl Timeline;
        public readonly TrackControlSurfaceItem SurfaceControl;
        public readonly TimelineTrackControl TrackControl;

        ITimelineElement ITrackElement.Timeline => this.Timeline;

        public ISelectionManager<IClipElement> Selection => this.TrackControl.Selection;

        public Track Track { get; }

        public bool IsSelected {
            get => this.Timeline.TrackSelectionManager!.IsSelected(this);
            set {
                if (value) {
                    this.Timeline.TrackSelectionManager!.Select(this);
                }
                else {
                    this.Timeline.TrackSelectionManager!.Unselect(this);
                }
            }
        }

        public TrackElementImpl(TimelineControl timeline, TrackControlSurfaceItem surfaceControl, TimelineTrackControl trackControl, Track track, bool isLoadingTimeline) {
            this.Timeline = timeline;
            this.SurfaceControl = surfaceControl;
            this.TrackControl = trackControl;
            this.Track = track;

            this.SurfaceControl.contextData.Set(DataKeys.TrackKey, this.Track).Set(DataKeys.TrackUIKey, this.SurfaceControl.TrackElement = this);
            this.TrackControl.contextData.Set(DataKeys.TrackKey, this.Track).Set(DataKeys.TrackUIKey, this.TrackControl.TrackElement = this);
            
            // We can invalidate the surface control since it's in a different branch of the visual tree
            DataManager.InvalidateInheritedContext(this.SurfaceControl);
            
            // Don't invalidate if the TimelineControl is adding the clips for the TimelineChanged event,
            // because there's no point since the timeline invalidates its own context after all tracks
            // are loaded which 
            if (!isLoadingTimeline) {
                DataManager.InvalidateInheritedContext(this.TrackControl);
            }
        }

        public IClipElement GetClipFromModel(Clip clip) {
            return this.TrackControl.ClipStoragePanel!.ItemMap.GetControl(clip);
        }

        public void Destroy() {
            this.SurfaceControl.contextData.Set(DataKeys.TrackKey, null).Set(DataKeys.TrackUIKey, null);
            DataManager.InvalidateInheritedContext(this.SurfaceControl);

            this.TrackControl.contextData.Set(DataKeys.TrackKey, null).Set(DataKeys.TrackUIKey, null);
            DataManager.InvalidateInheritedContext(this.TrackControl);
        }

        public void UpdateSelected(bool state) {
            TimelineTrackControl.InternalSetIsSelected(this.TrackControl, state);
            TrackControlSurfaceItem.InternalSetIsSelected(this.SurfaceControl, state);
        }
    }

    public TimelineControl() {
        this.Tracks = new TrackListImpl(this);
        this.myTrackElements = new List<TrackElementImpl>();
        this.trackElementMap = new ModelControlDictionary<Track, ITrackElement>();
        DataManager.SetContextData(this, this.contextData = new ContextData().Set(DataKeys.TimelineUIKey, this));
    }

    static TimelineControl() {
        TimelineProperty.Changed.AddClassHandler<TimelineControl, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    ITrackElement ITimelineElement.GetTrackFromModel(Track track) => this.trackElementMap.GetControl(track);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.SurfaceTrackList = e.NameScope.GetTemplateChild<TrackControlSurfaceList>("PART_TrackListBox");
        this.TrackStorage = e.NameScope.GetTemplateChild<TrackStoragePanel>("PART_Timeline");
        this.TimelineContentGrid = e.NameScope.GetTemplateChild<TimelineScrollableContentGrid>("PART_ContentGrid");
        this.TimelineScrollViewer = e.NameScope.GetTemplateChild<ScrollViewer>("PART_SequenceScrollViewer");
        this.TrackListScrollViewer = e.NameScope.GetTemplateChild<ScrollViewer>("PART_TrackListScrollViewer");
        this.TimelineBorder = e.NameScope.GetTemplateChild<Border>("PART_TimelineSequenceBorder");
        this.TimestampBorder = e.NameScope.GetTemplateChild<Border>("PART_TimestampBoard");
        this.PlayHead = e.NameScope.GetTemplateChild<PlayHeadControl>("PART_PlayHeadControl");
        this.StopHead = e.NameScope.GetTemplateChild<StopHeadControl>("PART_StopHeadControl");
        this.PlayHeadInfoTextControl = e.NameScope.GetTemplateChild<PlayheadPositionTextControl>("PART_PlayheadPositionPreviewControl");

        this.TrackSelectionManager = new TrackSelectionManager(this, this.myTrackElements);
        ((ILightSelectionManager<ITrackElement>) this.TrackSelectionManager).SelectionChanged += this.OnTrackChanged;

        this.ClipSelectionManager = new TimelineClipSelectionManager(this);
        ((ILightSelectionManager<IClipElement>) this.ClipSelectionManager).SelectionChanged += this.OnSelectionChanged;

        this.PlayHead!.TimelineControl = this;
        this.StopHead!.TimelineControl = this;
        this.TimelineContentGrid.PointerPressed += this.OnTimelineContentGridPointerPressed;

        this.TimestampBorder.PointerPressed += (s, ex) => this.MovePlayHeadToMouseCursor(ex.GetPosition((Visual?) s).X, true, false, ex);
    }

    private void OnSelectionChanged(ILightSelectionManager<IClipElement> sender) {
        VideoEditorPropertyEditorHelper.UpdateClipSelectionAsync(this);
    }

    private void OnTrackChanged(ILightSelectionManager<ITrackElement> sender) {
        VideoEditorPropertyEditorHelper.UpdateTrackSelectionAsync(this);
    }

    public void SetPlayHeadToMouseCursor(double x) {
        this.MovePlayHeadToMouseCursor(x, false);
    }

    private void MovePlayHeadToMouseCursor(double x, bool enableThumbDragging = true, bool updateStopHead = true, PointerEventArgs? ex = null) {
        if (!(this.Timeline is Timeline timeline)) {
            return;
        }

        if (x >= 0d) {
            long frameX = TimelineUtils.PixelToFrame(x, this.Zoom, true);
            if (frameX != timeline.PlayHeadPosition && frameX >= 0 && frameX < timeline.MaxDuration) {
                timeline.PlayHeadPosition = frameX;
                if (updateStopHead) {
                    timeline.StopHeadPosition = frameX;
                }
            }

            if (enableThumbDragging && ex != null) {
                this.PlayHead!.EnableDragging(ex);
            }
        }
    }

    public void SetZoom(double newZoom, ZoomType type, SKPointD mousePoint) {
        double oldZoom = this.Zoom;
        newZoom = Maths.Clamp(newZoom, 0.1, 200);
        if (Maths.Equals(oldZoom, newZoom)) {
            return;
        }

        this.Zoom = newZoom;
        this.OnTimelineZoomed(oldZoom, newZoom, type, mousePoint);
    }

    private void OnTimelineContentGridPointerPressed(object? sender, PointerPressedEventArgs e) {
        // User clicked the dark grey area, so update the play head position
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }
        
        if ((e.Source == sender || e.Source is ClipStoragePanel) && this.Timeline is Timeline timeline) {
            timeline.PlayHeadPosition = timeline.StopHeadPosition = TimelineClipControl.GetCursorFrame(this.TrackStorage!, e);
            this.ClipSelectionManager?.Clear();
        }
    }

    private void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        if (oldTimeline != null) {
            oldTimeline.MaxDurationChanged -= this.OnTimelineMaxDurationChanged;
            oldTimeline.TrackAdded -= this.OnTrackAddedEvent;
            oldTimeline.TrackRemoved -= this.OnTrackRemovedEvent;
            oldTimeline.TrackMoved -= this.OnTimelineTrackIndexMoved;
            for (int i = this.myTrackElements.Count - 1; i >= 0; i--) {
                this.RemoveTrackElement(i);
            }

            this.contextData.Set(DataKeys.TimelineKey, null);
        }

        this.TrackStorage!.SetTimelineControl(this);
        this.TrackStorage.Timeline = newTimeline; // it's crucial the timeline is set before we add track event handlers
        this.SurfaceTrackList!.TimelineControl = this;
        this.SurfaceTrackList.Timeline = newTimeline;
        this.PlayHeadInfoTextControl.Timeline = newTimeline;
        if (newTimeline != null) {
            newTimeline.MaxDurationChanged += this.OnTimelineMaxDurationChanged;
            newTimeline.TrackAdded += this.OnTrackAddedEvent;
            newTimeline.TrackRemoved += this.OnTrackRemovedEvent;
            newTimeline.TrackMoved += this.OnTimelineTrackIndexMoved;
            int i = 0;
            foreach (Track track in newTimeline.Tracks) {
                this.InsertTrackElement(track, i++, true);
            }

            this.contextData.Set(DataKeys.TimelineKey, newTimeline);
        }

        // Disable for now since i can't get it to work properly
        this.StopHead!.IsVisible = false;
        this.TrackSelectionManager!.UpdateSelection();
        DataManager.InvalidateInheritedContext(this);
    }

    private void InsertTrackElement(Track track, int i, bool isLoadingTimeline = false) {
        // We have to assume this method is invoked after the surface and real timeline are added
        TrackControlSurfaceItem surfaceItem = this.SurfaceTrackList!.GetTrack(i);
        TimelineTrackControl trackControl = this.TrackStorage!.GetTrack(i);
        this.myTrackElements.Insert(i, new TrackElementImpl(this, surfaceItem, trackControl, track, isLoadingTimeline));
    }

    private void RemoveTrackElement(int i) {
        TrackElementImpl element = this.myTrackElements[i];
        this.TrackSelectionManager!.Unselect(element);
        element.Destroy();
        this.myTrackElements.RemoveAt(i);
    }

    private void MoveTrackElement(int oldIndex, int newIndex) {
        TrackElementImpl item = this.myTrackElements[oldIndex];
        this.myTrackElements.RemoveAt(oldIndex);
        this.myTrackElements.Insert(newIndex, item);
    }

    private void OnTimelineTrackIndexMoved(Timeline timeline, Track track, int oldindex, int newindex) {
        this.MoveTrackElement(oldindex, newindex);

        TrackControlSurfaceItem movedTrack = this.SurfaceTrackList!.GetTrack(newindex);
        movedTrack.initiatedDragPointer?.Capture(movedTrack);
    }

    private void OnTrackAddedEvent(Timeline timeline, Track track, int index) {
        this.InsertTrackElement(track, index);
        this.TrackSelectionManager?.UpdateSelection(this.myTrackElements[index]);
        this.OnTrackAddedOrRemoved(timeline, index);
    }

    private void OnTrackRemovedEvent(Timeline timeline, Track track, int index) {
        this.OnTrackAddedOrRemoved(timeline, index);
        this.RemoveTrackElement(index);
    }

    private void OnTrackAddedOrRemoved(Timeline timeline, int index) {
        this.UpdateBorderThicknesses(timeline);
    }

    private void UpdateBorderThicknesses(Timeline timeline) {
        // Just a cool feature to hide the border when there's no tracks, not necessary but meh
        Thickness thickness = new Thickness(0, 0, 0, (timeline.Tracks.Count < 1) ? 0 : 1);
        this.TimelineBorder!.BorderThickness = thickness;
        this.SurfaceTrackList!.BorderThickness = thickness;
    }

    private void OnTimelineMaxDurationChanged(Timeline timeline) {
        this.UpdateContentGridSize();
    }

    private void UpdateContentGridSize() {
        if (this.TimelineContentGrid != null) {
            if (this.Timeline is Timeline timeline)
                this.TimelineContentGrid.Width = TimelineUtils.FrameToPixel(timeline.MaxDuration, this.Zoom);
            else
                this.TimelineContentGrid.ClearValue(Layoutable.WidthProperty);
        }
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e) {
        base.OnPointerWheelChanged(e);
        ScrollViewer? scroller = this.TimelineScrollViewer;
        if (scroller == null) {
            return;
        }

        KeyModifiers mods = e.KeyModifiers;
        if ((mods & KeyModifiers.Alt) != 0) {
            if (VisualTreeUtils.TryGetParent(e.Source as AvaloniaObject, out TimelineTrackControl? track)) {
                track.Track!.Height = Maths.Clamp(track.Track.Height + (e.Delta.Y / 120d * 8), TimelineClipControl.HeaderSize, 200d);
            }

            e.Handled = true;
        }
        else if ((mods & KeyModifiers.Control) != 0) {
            e.Handled = true;
            bool shift = (mods & KeyModifiers.Shift) != 0;
            double multiplier = (shift ? 0.2 : 0.4);
            if (e.Delta.Y > 0) {
                multiplier = 1d + multiplier;
            }
            else {
                multiplier = 1d - multiplier;
            }

            double oldZoom = this.Zoom;
            double newZoom = Math.Max(oldZoom * multiplier, 0.0001d);
            double minZoom = scroller.Viewport.Width / (scroller.Extent.Width / oldZoom); // add 0.000000000000001 to never disable scroll bar
            newZoom = Math.Max(minZoom, newZoom);
            Point mPos = e.GetPosition(this);
            this.SetZoom(newZoom, ZoomType.Direct, new SKPointD(mPos.X, mPos.Y)); // let the coerce function clamp the zoom value
            newZoom = this.Zoom;

            // managed to get zooming towards the cursor working
            double mouse_x = e.GetPosition(scroller).X;
            double target_offset = (scroller.Offset.X + mouse_x) / oldZoom;
            double scaled_target_offset = target_offset * newZoom;
            double new_offset = scaled_target_offset - mouse_x;
            scroller.Offset = new Vector(new_offset, scroller.Offset.Y);
        }
        else if ((mods & KeyModifiers.Shift) != 0) {
            if (e.Delta.Y < 0 || e.Delta.X < 0) {
                for (int i = 0; i < 6; i++) {
                    scroller.LineRight();
                }
            }
            else {
                for (int i = 0; i < 6; i++) {
                    scroller.LineLeft();
                }
            }

            e.Handled = true;
        }
    }

    private void OnTimelineZoomed(double oldZoom, double newZoom, ZoomType zoomType, SKPointD mPos) {
        this.TrackStorage?.OnZoomChanged(newZoom);
        this.UpdateContentGridSize();

        ScrollViewer? scroller = this.TimelineScrollViewer;
        if (scroller == null) {
            return;
        }

        switch (zoomType) {
            case ZoomType.Direct: break;
            case ZoomType.ViewPortBegin: {
                break;
            }
            case ZoomType.ViewPortMiddle: {
                break;
            }
            case ZoomType.ViewPortEnd: {
                break;
            }
            case ZoomType.PlayHead: {
                break;
            }
            case ZoomType.MouseCursor: {
                double mouse_x = AvCore.GetRelativePosition(scroller, new Point(mPos.X, mPos.Y)).X;
                double target_offset = (scroller.Offset.X + mouse_x) / oldZoom;
                double scaled_target_offset = target_offset * newZoom;
                double new_offset = scaled_target_offset - mouse_x;
                scroller.Offset = new Vector(new_offset, scroller.Offset.Y);
                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(zoomType), zoomType, null);
        }
    }

    public void MakeSingleSelection(IClipElement clipToSelect) {
        this.ClipSelectionManager!.SetSelection(clipToSelect);
    }

    public void MakeFrameRangeSelection(FrameSpan span, int trackSrcIdx = -1, int trackEndIndex = -1) {
        Timeline? timeline = this.Timeline;
        if (timeline == null) {
            return;
        }

        this.ClipSelectionManager!.Clear();
        List<(int, List<Clip>)> items = new List<(int, List<Clip>)>();
        if (trackSrcIdx == -1 || trackEndIndex == -1) {
            int i = 0;
            foreach (Track track in timeline.Tracks) {
                items.Add((i++, track.GetClipsInSpan(span)));
            }
        }
        else {
            for (int i = trackSrcIdx; i < trackEndIndex; i++) {
                items.Add((i, timeline.Tracks[i].GetClipsInSpan(span)));
            }
        }

        foreach ((int trackIndex, List<Clip> clips) tuple in items) {
            TimelineTrackControl track = (TimelineTrackControl) this.TrackStorage!.Children[tuple.trackIndex];
            track.SelectionManager!.SetSelection(tuple.clips.Select(x => track.ClipStoragePanel!.ItemMap.GetControl(x)));
        }
    }

    private class TrackListImpl : IReadOnlyList<ITrackElement> {
        public readonly TimelineControl control;

        public TrackListImpl(TimelineControl control) {
            this.control = control;
        }

        public IEnumerator<ITrackElement> GetEnumerator() {
            return this.control.myTrackElements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this.control.TrackStorage!.Children.Count;

        public ITrackElement this[int index] => this.control.myTrackElements[index];
    }
}