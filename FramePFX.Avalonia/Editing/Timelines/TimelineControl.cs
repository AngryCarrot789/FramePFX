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
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.MarkupExtensions;
using FramePFX.Avalonia.Editing.Playheads;
using FramePFX.Avalonia.Editing.Timelines.Selection;
using FramePFX.Avalonia.Editing.Timelines.TrackSurfaces;
using PFXToolKitUI.Avalonia;
using PFXToolKitUI.Avalonia.AdvancedMenuService;
using PFXToolKitUI.Avalonia.AvControls;
using PFXToolKitUI.Avalonia.Interactivity;
using PFXToolKitUI.Avalonia.Interactivity.Contexts;
using PFXToolKitUI.Avalonia.Utils;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Toolbars;
using FramePFX.Editing.UI;
using FramePFX.PropertyEditing;
using PFXToolKitUI;
using PFXToolKitUI.Avalonia.Toolbars.Toolbars;
using PFXToolKitUI.Interactivity;
using PFXToolKitUI.Toolbars;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Collections.Observable;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines;

public delegate void AvaloniaPropertyChangedEventHandler<T>(object sender, AvaloniaPropertyChangedEventArgs<T> e);

/// <summary>
/// The main control for a timeline in a video editor
/// </summary>
public class TimelineControl : TemplatedControl, ITimelineElement {
    public static readonly StyledProperty<Timeline?> TimelineProperty = AvaloniaProperty.Register<TimelineControl, Timeline?>(nameof(Timeline));
    public static readonly DirectProperty<TimelineControl, double> ZoomProperty = AvaloniaProperty.RegisterDirect<TimelineControl, double>(nameof(Zoom), o => o.Zoom, unsetValue: 1.0);
    public static readonly StyledProperty<bool> IsTrackAutomationVisibleProperty = AvaloniaProperty.Register<TimelineControl, bool>(nameof(IsTrackAutomationVisible), false);
    public static readonly StyledProperty<bool> IsClipAutomationVisibleProperty = AvaloniaProperty.Register<TimelineControl, bool>(nameof(IsClipAutomationVisible), true);

    private readonly ModelControlDictionary<Track, ITrackElement> trackElementMap;

    public IModelControlDictionary<Track, ITrackElement> TrackElementMap => this.trackElementMap;

    public Timeline? Timeline {
        get => this.GetValue(TimelineProperty);
        set => this.SetValue(TimelineProperty, value);
    }

    public bool IsTrackAutomationVisible {
        get => this.GetValue(IsTrackAutomationVisibleProperty);
        set => this.SetValue(IsTrackAutomationVisibleProperty, value);
    }

    public bool IsClipAutomationVisible {
        get => this.GetValue(IsClipAutomationVisibleProperty);
        set => this.SetValue(IsClipAutomationVisibleProperty, value);
    }

    public double Zoom => this.myZoomFactor;

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

    public InputElement? RulerBorder { get; private set; }

    public FlatLinePlayHeadControl? PlayHeadInSequence { get; private set; }

    public FlatLinePlayHeadControl? StopHeadInSequence { get; private set; }

    public GrippedPlayHeadControl? PlayHeadInRuler { get; private set; }

    public TimelineRuler? TimelineRuler { get; private set; }

    public TimelineLoopControl? LoopControl { get; private set; }

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

    public EditorWindow? EditorOwner { get; set; }

    IVideoEditorWindow ITimelineElement.VideoEditor => this.EditorOwner ?? throw new InvalidOperationException("Not connected to an editor window");

    public IReadOnlyList<ITrackElement> Tracks { get; }

    ISelectionManager<ITrackElement> ITimelineElement.Selection => this.TrackSelectionManager!;

    ISelectionManager<IClipElement> ITimelineElement.ClipSelection => this.ClipSelectionManager!;

    private double myZoomFactor = 1.0;
    internal readonly List<TrackElementImpl> myTrackElements;

    public event UITimelineModelChanged? TimelineModelChanging;
    public event UITimelineModelChanged? TimelineModelChanged;
    public event AvaloniaPropertyChangedEventHandler<double>? ZoomChanged;

    private StackPanel? PART_ToolBar_West;
    private StackPanel? PART_ToolBar_East;
    private StackPanel? PART_TrackControlSurfaceToolBar;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeWestButtonListHandler;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeEastButtonListHandler;
    private ObservableItemProcessorIndexing<ToolBarButton>? disposeControlSurfaceListHandler;

    public TimelineControl() {
        this.Tracks = new TrackListImpl(this);
        this.myTrackElements = new List<TrackElementImpl>();
        this.trackElementMap = new ModelControlDictionary<Track, ITrackElement>();
        DataManager.GetContextData(this).Set(DataKeys.TimelineUIKey, this);
    }

    static TimelineControl() {
        
        
        TimelineProperty.Changed.AddClassHandler<TimelineControl, Timeline?>((d, e) => d.OnTimelineChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        IsTrackAutomationVisibleProperty.Changed.AddClassHandler<TimelineControl, bool>((d, e) => d.OnIsTrackAutomationVisibilityChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        IsClipAutomationVisibleProperty.Changed.AddClassHandler<TimelineControl, bool>((d, e) => d.OnIsClipAutomationVisibilityChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        ZoomProperty.Changed.AddClassHandler<TimelineControl, double>((d, e) => d.ZoomChanged?.Invoke(d, e));
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
        this.RulerBorder = e.NameScope.GetTemplateChild<InputElement>("PART_RulerBorder");
        this.PlayHeadInSequence = e.NameScope.GetTemplateChild<FlatLinePlayHeadControl>("PART_PlayHeadControl");
        this.StopHeadInSequence = e.NameScope.GetTemplateChild<FlatLinePlayHeadControl>("PART_StopHeadControl");
        this.PlayHeadInRuler = e.NameScope.GetTemplateChild<GrippedPlayHeadControl>("PART_RulerPlayHead");
        this.TimelineRuler = e.NameScope.GetTemplateChild<TimelineRuler>("PART_Ruler");
        this.LoopControl = e.NameScope.GetTemplateChild<TimelineLoopControl>("PART_LoopControl");
        this.PlayHeadInfoTextControl = e.NameScope.GetTemplateChild<PlayheadPositionTextControl>("PART_PlayheadPositionPreviewControl");
        this.PART_ToolBar_West = e.NameScope.GetTemplateChild<StackPanel>("PART_ToolBar_West");
        this.PART_ToolBar_East = e.NameScope.GetTemplateChild<StackPanel>("PART_ToolBar_East");
        this.PART_TrackControlSurfaceToolBar = e.NameScope.GetTemplateChild<StackPanel>("PART_TrackControlSurfaceToolBar");

        ToggleButton toggleTrackAutomationButton = e.NameScope.GetTemplateChild<ToggleButton>("PART_ToggleTrackAutomation");
        toggleTrackAutomationButton.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(this.IsTrackAutomationVisible), BindingMode.TwoWay) { Source = this });

        ToggleButton toggleClipAutomationButton = e.NameScope.GetTemplateChild<ToggleButton>("PART_ToggleClipAutomation");
        toggleClipAutomationButton.Bind(ToggleButton.IsCheckedProperty, new Binding(nameof(this.IsClipAutomationVisible), BindingMode.TwoWay) { Source = this });

        this.TrackSelectionManager = new TrackSelectionManager(this, this.myTrackElements);
        this.TrackSelectionManager.LightSelectionChanged += this.OnTrackChanged;

        this.ClipSelectionManager = new TimelineClipSelectionManager(this);
        this.ClipSelectionManager.LightSelectionChanged += this.OnSelectionChanged;

        this.PlayHeadInSequence!.TimelineControl = this;
        this.StopHeadInSequence!.TimelineControl = this;
        this.PlayHeadInRuler!.TimelineControl = this;
        this.TimelineContentGrid.PointerPressed += this.OnTimelineContentGridPointerPressed;
        AdvancedContextMenu.SetContextRegistry(this.TimelineContentGrid, Timeline.ContextRegistry);

        this.RulerBorder.PointerPressed += (s, ex) => this.MovePlayHeadToMouseCursor(ex.GetPosition(this.TimelineContentGrid).X, true, false, ex);

        // Has to be a 'preview' handler in WPF speak, since we need to prevent the base scroll viewer scrolling down even if CTRL is held
        this.TimelineScrollViewer.AddHandler(PointerWheelChangedEvent, this.TimelineScrollViewerOnPointerWheelChanged, RoutingStrategies.Tunnel);
        this.RulerBorder.AddHandler(PointerWheelChangedEvent, this.TimeStampBoardScrollViewerOnPointerWheelChanged, RoutingStrategies.Tunnel);
    }

    public void OnConnectedToEditor(EditorWindow window) {
        this.EditorOwner = window;
        TemplateUtils.Apply(this);

        TimelineToolBarManager tlManager = TimelineToolBarManager.GetInstance(window.VideoEditor);
        ControlSurfaceListToolBarManager csManager = ControlSurfaceListToolBarManager.GetInstance(window.VideoEditor);

        this.disposeWestButtonListHandler = CreateToolbarBinder(tlManager.WestButtons, this.PART_ToolBar_West!);
        this.disposeEastButtonListHandler = CreateToolbarBinder(tlManager.EastButtons, this.PART_ToolBar_East!);
        this.disposeControlSurfaceListHandler = CreateToolbarBinder(csManager.Buttons, this.PART_TrackControlSurfaceToolBar!);
    }

    private static ObservableItemProcessorIndexing<ToolBarButton> CreateToolbarBinder(IObservableList<ToolBarButton> list, StackPanel stackPanel) {
        int originalCounter = stackPanel.Children.Count;
        return ObservableItemProcessor.MakeIndexable(list, (sender, index, item) => {
            AbstractAvaloniaButtonElement btnImpl = (AbstractAvaloniaButtonElement) item.Button;
            if (btnImpl.Button is IIconButton button) {
                button.IconMaxWidth = 18;
                button.IconMaxHeight = 18;
            }

            btnImpl.Button.MinWidth = 26;
            btnImpl.Button.Height = 26;
            btnImpl.Button.Padding = new Thickness(4);
            btnImpl.Button.BorderThickness = default;
            btnImpl.Button.Bind(BackgroundProperty, new DynamicResourceExtension("ABrush.Tone6.Background.Static"));
            btnImpl.Button.Background = null;

            stackPanel.Children.Insert(index + originalCounter, btnImpl.Button);
            item.UpdateCanExecuteLater();
        }, (sender, index, item) => {
            stackPanel.Children.RemoveAt(index + originalCounter);
        }, (sender, oldIndex, newIndex, item) => {
            stackPanel.Children.MoveItem(oldIndex + originalCounter, newIndex + originalCounter);
        }).AddExistingItems();
    }

    public void OnDisconnectedFromEditor() {
        this.EditorOwner = null;
        this.disposeWestButtonListHandler!.Dispose();
        this.disposeEastButtonListHandler!.Dispose();
        this.disposeControlSurfaceListHandler!.Dispose();
        this.PART_ToolBar_West!.Children.Clear();
        this.PART_ToolBar_East!.Children.Clear();
        this.PART_TrackControlSurfaceToolBar!.Children.Clear();
    }

    private void OnIsTrackAutomationVisibilityChanged(bool oldValue, bool newValue) => this.UpdateIsTrackAutomationVisible(newValue, null);

    private void OnIsClipAutomationVisibilityChanged(bool oldValue, bool newValue) => this.UpdateIsTrackAutomationVisible(null, newValue);

    private void UpdateIsTrackAutomationVisible(bool? trackVisible, bool? clipVisible) {
        if (!clipVisible.HasValue && !trackVisible.HasValue) {
            return;
        }

        foreach (TimelineTrackControl track in this.TrackStorage!.GetTracks()) {
            UpdateTrackAutomationVisible(track, trackVisible, clipVisible, true);
        }

        foreach (TrackControlSurfaceItem track in this.SurfaceTrackList!.GetTracks()) {
            UpdateTrackAutomationVisible(track, trackVisible);
        }
    }

    public static void UpdateTrackAutomationVisible(TimelineTrackControl track, bool? trackVisible, bool? clipVisible, bool clipsToo) {
        if (trackVisible.HasValue)
            track.OnIsAutomationVisibilityChanged(trackVisible.Value);

        if (clipsToo) {
            foreach (TimelineClipControl clip in track.ClipStoragePanel!.GetClips()) {
                UpdateClipAutomationVisible(clip, trackVisible, clipVisible);
            }
        }
    }

    public static void UpdateTrackAutomationVisible(TrackControlSurfaceItem track, bool? trackVisible) {
        if (trackVisible.HasValue)
            track.OnIsAutomationVisibilityChanged(trackVisible.Value);
    }

    public static void UpdateClipAutomationVisible(TimelineClipControl clip, bool? trackVisible, bool? clipVisible) {
        if (clipVisible.HasValue)
            clip.OnIsAutomationVisibilityChanged(clipVisible.Value);
        if (trackVisible.HasValue)
            clip.Opacity = trackVisible.Value ? 0.7 : 1.0;
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
                this.PlayHeadInRuler!.EnableDragging(ex);
                // this.PlayHeadInSequence!.EnableDragging(ex);
            }
        }
    }

    public void SetZoom(double newZoom) {
        double oldZoom = this.Zoom;
        newZoom = Maths.Clamp(newZoom, 0.1, 200);
        if (Maths.Equals(oldZoom, newZoom)) {
            return;
        }

        this.SetAndRaise(ZoomProperty, ref this.myZoomFactor, newZoom);
        this.OnTimelineZoomed(oldZoom, newZoom);
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
        if (ReferenceEquals(oldTimeline, newTimeline)) {
            // Should never reach this, but just for clarity, might as well check it
            return;
        }

        using MultiChangeToken myContextBatch = DataManager.GetContextData(this).BeginChange();
        this.TimelineModelChanging?.Invoke(this, oldTimeline, newTimeline);
        if (oldTimeline != null) {
            oldTimeline.MaxDurationChanged -= this.OnTimelineMaxDurationChanged;
            oldTimeline.TrackAdded -= this.OnTrackAddedEvent;
            oldTimeline.TrackRemoved -= this.OnTrackRemovedEvent;
            oldTimeline.TrackMoved -= this.OnTimelineTrackIndexMoved;
            for (int i = this.myTrackElements.Count - 1; i >= 0; i--) {
                this.RemoveTrackElement(i);
            }

            myContextBatch.Context.Set(DataKeys.TimelineKey, null);
        }

        this.TrackStorage!.SetTimelineControl(this);
        this.TrackStorage.Timeline = newTimeline; // it's crucial the timeline is set before we add track event handlers
        this.SurfaceTrackList!.TimelineControl = this;
        this.SurfaceTrackList.Timeline = newTimeline;
        this.PlayHeadInfoTextControl.Timeline = newTimeline;
        this.TimelineRuler!.TimelineControl = this;
        this.LoopControl!.TimelineControl = this;
        if (newTimeline != null) {
            newTimeline.MaxDurationChanged += this.OnTimelineMaxDurationChanged;
            newTimeline.TrackAdded += this.OnTrackAddedEvent;
            newTimeline.TrackRemoved += this.OnTrackRemovedEvent;
            newTimeline.TrackMoved += this.OnTimelineTrackIndexMoved;
            int i = 0;
            foreach (Track track in newTimeline.Tracks) {
                this.InsertTrackElement(track, i++, true);
            }

            myContextBatch.Context.Set(DataKeys.TimelineKey, newTimeline);
            this.UpdateIsTrackAutomationVisible(this.IsTrackAutomationVisible, this.IsClipAutomationVisible);
        }

        this.TrackSelectionManager!.UpdateSelection();

        this.TimelineModelChanged?.Invoke(this, oldTimeline, newTimeline);

        this.SetZoom(1.0);
        ApplicationPFX.Instance.Dispatcher.Invoke(this.UpdateTimelineViewportSize, DispatchPriority.Loaded);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e) {
        base.OnSizeChanged(e);
        this.UpdateTimelineViewportSize();
    }

    private void UpdateTimelineViewportSize() {
        if (this.TimelineScrollViewer != null) {
            if (this.Timeline is Timeline timeline) {
                double widthVp = this.TimelineScrollViewer.Viewport.Width;
                double rawTimelineWidth = timeline.MaxDuration;
                double realTimelineWidth = rawTimelineWidth * this.Zoom;
                // !DoubleUtils.AreClose(realTimelineWidth, this.TimelineScrollViewer.Extent.Width) || 
                if (realTimelineWidth < widthVp) {
                    double newZoomRatio = widthVp / rawTimelineWidth;
                    this.SetZoom(newZoomRatio);
                }
            }
        }
    }

    private void InsertTrackElement(Track track, int i, bool isLoadingTimeline = false) {
        // We have to assume this method is invoked after the surface and real timeline are added
        TrackControlSurfaceItem surfaceItem = this.SurfaceTrackList!.GetTrack(i);
        TimelineTrackControl trackControl = this.TrackStorage!.GetTrack(i);
        this.myTrackElements.Insert(i, new TrackElementImpl(this, surfaceItem, trackControl, track));
        UpdateTrackAutomationVisible(trackControl, this.IsTrackAutomationVisible, this.IsClipAutomationVisible, true);
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
                this.TimelineContentGrid.ClearValue(WidthProperty);
        }
    }

    private void TimelineScrollViewerOnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
        this.OnMouseWheel((ScrollViewer) sender!, e);
    }

    private void TimeStampBoardScrollViewerOnPointerWheelChanged(object? sender, PointerWheelEventArgs e) {
        this.OnMouseWheel(this.TimelineScrollViewer!, e);
    }

    private void OnMouseWheel(ScrollViewer scroller, PointerWheelEventArgs e) {
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
            this.SetZoom(newZoom); // let the coerce function clamp the zoom value
            newZoom = this.Zoom;

            // managed to get zooming towards the cursor working
            double mouse_x = e.GetPosition(scroller).X;
            double target_offset = (scroller.Offset.X + mouse_x) / oldZoom;
            double scaled_target_offset = target_offset * newZoom;
            double new_offset = scaled_target_offset - mouse_x;
            scroller.Offset = new Vector(new_offset, scroller.Offset.Y);
            e.Handled = true;
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

    private void OnTimelineZoomed(double oldZoom, double newZoom) {
        this.TrackStorage?.OnZoomChanged(newZoom);
        this.TimelineRuler?.OnZoomChanged(newZoom);
        this.UpdateContentGridSize();
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
            track.SelectionManager.SetSelection(tuple.clips.Select(x => track.ClipStoragePanel!.ItemMap.GetControl(x)));
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

    // A fake track element implementation, because there's technically 2 controls for a single track
    internal class TrackElementImpl : ITrackElement {
        public readonly TimelineControl Timeline;
        public readonly TrackControlSurfaceItem SurfaceControl;
        public readonly TimelineTrackControl TrackControl;
        private Track? myTrack;

        ITimelineElement ITrackElement.Timeline => this.myTrack != null ? this.Timeline : throw new InvalidOperationException("Invalid track element");

        public ISelectionManager<IClipElement> Selection => this.TrackControl.SelectionManager;

        public Track Track => this.myTrack ?? throw new InvalidOperationException("Invalid track element");

        public bool IsSelected {
            get => this.Timeline.TrackSelectionManager!.IsSelected(this);
            set {
                if (this.myTrack == null)
                    throw new InvalidOperationException("Invalid track element");

                if (value) {
                    this.Timeline.TrackSelectionManager!.Select(this);
                }
                else {
                    this.Timeline.TrackSelectionManager!.Unselect(this);
                }
            }
        }

        public TrackElementImpl(TimelineControl timeline, TrackControlSurfaceItem surfaceControl, TimelineTrackControl trackControl, Track track) {
            this.Timeline = timeline;
            this.SurfaceControl = surfaceControl;
            this.TrackControl = trackControl;
            this.myTrack = track;

            using (MultiChangeToken batch = DataManager.GetContextData(this.SurfaceControl).BeginChange())
                batch.Context.Set(DataKeys.TrackKey, this.Track).Set(DataKeys.TrackUIKey, this.SurfaceControl.TrackElement = this);

            using (MultiChangeToken batch = DataManager.GetContextData(this.TrackControl).BeginChange())
                batch.Context.Set(DataKeys.TrackKey, this.Track).Set(DataKeys.TrackUIKey, this.TrackControl.TrackElement = this);
        }

        public IClipElement GetClipFromModel(Clip clip) {
            if (this.myTrack == null)
                throw new InvalidOperationException("Invalid track element");

            return this.TrackControl.ClipStoragePanel!.ItemMap.GetControl(clip);
        }

        public void Destroy() {
            DataManager.GetContextData(this.SurfaceControl).Remove(DataKeys.TrackKey, DataKeys.TrackUIKey);
            DataManager.GetContextData(this.TrackControl).Remove(DataKeys.TrackKey, DataKeys.TrackUIKey);
            this.myTrack = null;
        }

        public void UpdateSelected(bool state) {
            TimelineTrackControl.InternalSetIsSelected(this.TrackControl, state);
            TrackControlSurfaceItem.InternalSetIsSelected(this.SurfaceControl, state);
        }
    }
}