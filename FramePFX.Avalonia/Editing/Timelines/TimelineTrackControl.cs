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
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using FramePFX.Avalonia.AdvancedMenuService;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Converters;
using FramePFX.Avalonia.Editing.Automation;
using FramePFX.Avalonia.Editing.ResourceManaging.Trees;
using FramePFX.Avalonia.Editing.Timelines.Selection;
using FramePFX.Avalonia.Interactivity;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing;
using FramePFX.Editing.Automation.Keyframes;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.UI;
using FramePFX.Interactivity;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using SkiaSharp;
using Track = FramePFX.Editing.Timelines.Tracks.Track;

namespace FramePFX.Avalonia.Editing.Timelines;

public class TimelineTrackControl : TemplatedControl
{
    public static readonly DirectProperty<TimelineTrackControl, Track?> TrackProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, Track?>(nameof(Track), o => o.Track);
    public static readonly DirectProperty<TimelineTrackControl, bool> IsSelectedProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, bool>(nameof(IsSelected), o => o.IsSelected);
    public static readonly DirectProperty<TimelineTrackControl, ILinearGradientBrush?> ClipHeaderBrushProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, ILinearGradientBrush?>(nameof(ClipHeaderBrush), o => o.ClipHeaderBrush);
    public static readonly DirectProperty<TimelineTrackControl, ISolidColorBrush?> TrackColourForegroundBrushProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, ISolidColorBrush?>(nameof(TrackColourForegroundBrush), o => o.TrackColourForegroundBrush);
    public static readonly StyledProperty<AutomationSequence?> AutomationSequenceProperty = AvaloniaProperty.Register<TimelineTrackControl, AutomationSequence?>(nameof(AutomationSequence));
    public static readonly DirectProperty<TimelineTrackControl, FrameSpan?> DropFrameSpanProperty = AvaloniaProperty.RegisterDirect<TimelineTrackControl, FrameSpan?>(nameof(DropFrameSpan), o => o.DropFrameSpan);

    private FrameSpan? _dropFrameSpan;
    private Track? myTrack;
    private ILinearGradientBrush? clipHeaderBrush;
    private ISolidColorBrush? _trackColourForegroundBrush;
    internal readonly ContextData contextData;
    private MovedClip? clipBeingMoved;
    private bool internalIsSelected;
    private bool isProcessingAsyncDrop;
    private readonly PropertyBinder<AutomationSequence?> automationSequenceBinder;
    private AutomationSequenceEditorControl? PART_AutomationEditor;
    private Border? dropBorder;

    public FrameSpan? DropFrameSpan
    {
        get => this._dropFrameSpan;
        private set => this.SetAndRaise(DropFrameSpanProperty, ref this._dropFrameSpan, value);
    }
    
    public Track? Track
    {
        get => this.myTrack;
        private set
        {
            Track? oldTrack = this.myTrack;
            this.SetAndRaise(TrackProperty, ref this.myTrack, value);
            this.ClipStoragePanel?.OnTrackChanged(oldTrack, value);
        }
    }

    /// <summary>
    /// Gets the storage panel this track exists in
    /// </summary>
    public TrackStoragePanel? TrackStoragePanel { get; private set; }

    /// <summary>
    /// Gets the timeline control that this track exists in
    /// </summary>
    public TimelineControl? TimelineControl { get; private set; }

    public ClipStoragePanel? ClipStoragePanel { get; private set; }

    /// <summary>
    /// Gets or sets if we have a timeline and track associated
    /// </summary>
    public bool IsConnected { get; private set; }

    public double TimelineZoom => this.TimelineControl?.Zoom ?? 1.0;

    public ILinearGradientBrush? ClipHeaderBrush
    {
        get => this.clipHeaderBrush;
        private set => this.SetAndRaise(ClipHeaderBrushProperty, ref this.clipHeaderBrush, value);
    }

    public ISolidColorBrush? TrackColourForegroundBrush
    {
        get => this._trackColourForegroundBrush;
        private set => this.SetAndRaise(TrackColourForegroundBrushProperty, ref this._trackColourForegroundBrush, value);
    }

    // Selection must be done via the TimelineSelectionManager

    /// <summary>
    /// Gets whether this track control is selected
    /// </summary>
    public bool IsSelected
    {
        get => this.internalIsSelected;
        private set => this.SetAndRaise(IsSelectedProperty, ref this.internalIsSelected, value);
    }

    public AutomationSequence? AutomationSequence
    {
        get => this.GetValue(AutomationSequenceProperty);
        set => this.SetValue(AutomationSequenceProperty, value);
    }

    public ClipSelectionManager SelectionManager { get; }

    public ITrackElement? TrackElement { get; internal set; }

    public TimelineTrackControl()
    {
        this.ClipHeaderBrush = new LinearGradientBrush();
        this.TrackColourForegroundBrush = new SolidColorBrush();
        this.SelectionManager = new ClipSelectionManager(this);
        this.Focusable = true;
        this.UseLayoutRounding = true;
        DataManager.SetContextData(this, this.contextData = new ContextData());
        this.automationSequenceBinder = new PropertyBinder<AutomationSequence?>(this, AutomationSequenceProperty, AutomationSequenceEditorControl.AutomationSequenceProperty);
        DragDrop.SetAllowDrop(this, true);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        AdvancedContextMenu.SetContextRegistry(this, Track.TimelineTrackContextRegistry);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        AdvancedContextMenu.SetContextRegistry(this, null);
    }

    static TimelineTrackControl()
    {
        PointerPressedEvent.AddClassHandler<TimelineTrackControl>((c, e) => c.OnPreviewPointerPressed(e), RoutingStrategies.Tunnel);
        DragDrop.DragEnterEvent.AddClassHandler<TimelineTrackControl>((o, e) => o.OnDragEnter(e));
        DragDrop.DragOverEvent.AddClassHandler<TimelineTrackControl>((o, e) => o.OnDragOver(e));
        DragDrop.DragLeaveEvent.AddClassHandler<TimelineTrackControl>((o, e) => o.OnDragLeave(e));
        DragDrop.DropEvent.AddClassHandler<TimelineTrackControl>((o, e) => o.OnDrop(e));
        DropFrameSpanProperty.Changed.AddClassHandler<TimelineTrackControl, FrameSpan?>((d, e) => d.OnDropFrameSpanChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
    }

    private void OnDropFrameSpanChanged(FrameSpan? oldValue, FrameSpan? newValue) {
        if (!newValue.HasValue)
        {
            this.dropBorder!.IsVisible = false;
        }
        else
        {
            this.dropBorder!.IsVisible = true;
            FrameSpan span = newValue.Value;
            double pxBegin = TimelineUtils.FrameToPixel(span.Begin, this.TimelineZoom);
            double pxDuration = TimelineUtils.FrameToPixel(span.Duration, this.TimelineZoom);
            this.dropBorder!.Margin = new Thickness(pxBegin, 0, 0, 0);
            this.dropBorder!.Width = pxDuration;
        }
    }

    internal static void InternalSetIsSelected(TimelineTrackControl control, bool isSelected) => control.IsSelected = isSelected;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        this.ClipStoragePanel = e.NameScope.GetTemplateChild<ClipStoragePanel>("PART_TrackClipPanel");
        this.ClipStoragePanel.Connect(this);

        this.PART_AutomationEditor = e.NameScope.GetTemplateChild<AutomationSequenceEditorControl>("PART_AutomationEditor");
        this.PART_AutomationEditor.HorizontalZoom = this.TimelineZoom;
        if (this.TimelineControl?.Timeline is Timeline timeline)
            this.PART_AutomationEditor.FrameDuration = timeline.MaxDuration;
        
        this.automationSequenceBinder.SetTargetControl(this.PART_AutomationEditor);

        this.dropBorder = e.NameScope.GetTemplateChild<Border>("PART_DropSpanBorder");
        this.dropBorder.ZIndex = 50;
    }

    public void OnMaxDurationChanged(long newMaxDuration)
    {
        if (this.PART_AutomationEditor != null)
            this.PART_AutomationEditor.FrameDuration = newMaxDuration;
    }

    public virtual void OnConnecting(TrackStoragePanel timelineControl, Track track)
    {
        if (this.Track != null)
            throw new InvalidOperationException("Already connected to a track");

        Validate.NotNull(timelineControl);
        Validate.NotNull(track);

        this.TimelineControl = timelineControl.TimelineControl ?? throw new InvalidOperationException("TrackStoragePanel does not have a timeline control associated");
        this.TrackStoragePanel = timelineControl;
        this.Track = track;

        track.ClipAdded += this.OnClipAdded;
        track.ClipRemoved += this.OnClipRemoved;
        track.ClipMovedTracks += this.OnClipMovedTracks;
        track.HeightChanged += this.OnTrackHeightChanged;
        track.ColourChanged += this.OnTrackColourChanged;
    }

    public virtual void OnConnected()
    {
        this.IsConnected = true;
        this.UpdateTrackColour();

        int i = 0;
        foreach (Clip clip in this.Track!.Clips)
        {
            this.ClipStoragePanel!.InsertClip(clip, i++);
        }
    }

    public virtual void OnDisconnecting()
    {
        if (this.Track == null)
            throw new InvalidOperationException("Not connected to a track");

        this.Track.ClipAdded -= this.OnClipAdded;
        this.Track.ClipRemoved -= this.OnClipRemoved;
        this.Track.ClipMovedTracks -= this.OnClipMovedTracks;
        this.Track.HeightChanged -= this.OnTrackHeightChanged;
        this.Track.ColourChanged -= this.OnTrackColourChanged;
        this.ClipStoragePanel!.ClearClipsInternal();
    }

    public virtual void OnDisconnected()
    {
        this.IsConnected = false;
        this.Track = null;
        this.TimelineControl = null;
        this.TrackStoragePanel = null;
    }

    public virtual void OnIndexMoving(int oldIndex, int newIndex) {
    }

    public virtual void OnIndexMoved(int oldIndex, int newIndex) {
    }

    private void OnClipAdded(Track track, Clip clip, int index)
    {
        this.ClipStoragePanel!.InsertClip(clip, index);
    }

    private void OnClipRemoved(Track track, Clip clip, int index)
    {
        this.ClipStoragePanel!.RemoveClipInternal(index);
    }

    private void OnClipMovedTracks(Clip clip, Track oldTrack, int oldIndex, Track newTrack, int newIndex)
    {
        if (oldTrack == this.Track)
        {
            TimelineTrackControl? dstTrack = this.TrackStoragePanel!.GetTrackByModel(newTrack);
            if (dstTrack == null)
            {
                // Instead of throwing, we could just remove the track or insert a new track, instead of
                // trying to re-use existing controls, at the cost of performance.
                // However, moving clips between tracks in different timelines is not directly supported
                // so there's no need to support it here
                throw new Exception("Could not find destination track. Is the UI timeline corrupted or did the clip move between timelines?");
            }

            TimelineClipControl control = this.ClipStoragePanel![oldIndex];
            this.ClipStoragePanel.RemoveClipInternal(oldIndex, false);
            dstTrack.clipBeingMoved = new MovedClip(control, clip);
        }
        else if (newTrack == this.Track)
        {
            if (!(this.clipBeingMoved is MovedClip movedClip))
            {
                throw new Exception("Clip control being moved is null. Is the UI timeline corrupted or did the clip move between timelines?");
            }

            this.ClipStoragePanel!.InsertClip(movedClip.control, movedClip.clip, newIndex);
            this.clipBeingMoved = null;
            movedClip.control.initiatedDragPointer?.Capture(movedClip.control);
        }
    }

    private void OnTrackHeightChanged(Track track)
    {
        this.InvalidateMeasure();
        this.ClipStoragePanel!.InvalidateMeasure();
        this.TrackStoragePanel!.InvalidateVisual();
    }

    private void OnTrackColourChanged(Track track)
    {
        this.UpdateTrackColour();
        foreach (TimelineClipControl clip in this.ClipStoragePanel!)
        {
            clip.InvalidateVisual();
        }
    }

    private void UpdateTrackColour()
    {
        if (this.Track == null)
        {
            return;
        }

        SKColor col = this.Track.Colour;
        // ((SolidColorBrush) this.TrackColourBrush).Color = Color.FromArgb(col.Alpha, col.Red, col.Green, col.Blue);

        LinearGradientBrush brush = (LinearGradientBrush) this.ClipHeaderBrush!;
        brush.StartPoint = new RelativePoint(new Point(0, 0), RelativeUnit.Relative);
        brush.EndPoint = new RelativePoint(new Point(1, 0), RelativeUnit.Relative);

        // const byte sub = 40;
        const byte sub = 80;
        Color primary = Color.FromArgb(col.Alpha, col.Red, col.Green, col.Blue);
        Color secondary = Color.FromArgb(col.Alpha, (byte) Math.Max(col.Red - sub, 0), (byte) Math.Max(col.Green - sub, 0), (byte) Math.Max(col.Blue - sub, 0));

        brush.GradientStops.Clear();
        brush.GradientStops.Add(new GradientStop(primary, 0.0));
        brush.GradientStops.Add(new GradientStop(secondary, 1.0));

        ((SolidColorBrush) this.TrackColourForegroundBrush!).Color = PerceivedForegroundConverter.GetColour(primary);
    }

    private readonly struct MovedClip
    {
        public readonly TimelineClipControl control;
        public readonly Clip clip;

        public MovedClip(TimelineClipControl control, Clip clip)
        {
            this.control = control;
            this.clip = clip;
        }
    }

    public void OnClipSpanChanged() => this.ClipStoragePanel?.InvalidateArrange();

    public void OnZoomChanged(double newZoom)
    {
        // this.InvalidateMeasure();
        if (this.PART_AutomationEditor != null)
            this.PART_AutomationEditor.HorizontalZoom = newZoom;
        foreach (TimelineClipControl clip in this.ClipStoragePanel!)
        {
            clip.OnZoomChanged(newZoom);
        }
    }

    public void OnPreviewPointerPressed(PointerPressedEventArgs e)
    {
        if (this.Track != null)
        {
            // update context data, used by action system and context menu system
            PointerUpdateKind change = e.GetCurrentPoint(this).Properties.PointerUpdateKind;
            if (change == PointerUpdateKind.LeftButtonPressed || change == PointerUpdateKind.RightButtonPressed)
            {
                this.contextData.Set(DataKeys.TrackContextMouseFrameKey, this.GetFrameAtMousePoint(e));
                DataManager.InvalidateInheritedContext(this);
            }

            // don't focus if the click hit a clip, since the clip will be focused right after so it's pointless
            if (this.IsConnected && this.TrackElement != null)
            {
                this.TimelineControl!.TrackSelectionManager!.SetSelection(this.TrackElement);
            }

            // If we didn't hit a clip, then clear clip selection
            if (!(e.Source is TimelineClipControl))
            {
                this.SelectionManager.Clear();
                
                // this.TimelineControl!.ClipSelectionManager!.Clear();
            }

            // Used to have to manually update property editor, but now that we have the selection managers,
            // we can use those + RapidDispatchAction or RateLimitedDispatchAction to limit activity
            // VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
        }
    }

    private long GetFrameAtMousePoint(PointerPressedEventArgs e) => this.GetFrameAtMousePoint(e.GetPosition(this));

    public long GetFrameAtMousePoint(Point pointRelativeToThis)
    {
        return TimelineUtils.PixelToFrame(pointRelativeToThis.X, this.TimelineControl?.Zoom ?? 1.0, true);
    }

    public void OnIsAutomationVisibilityChanged(bool isVisible)
    {
        this.PART_AutomationEditor!.IsVisible = isVisible;
    }

    #region Drag dropping items into this clip

    protected void OnDragEnter(DragEventArgs e) => this.OnDragOver(e);

    protected void OnDragOver(DragEventArgs e)
    {
        e.Handled = true;
        if (this.isProcessingAsyncDrop)
        {
            e.DragEffects = DragDropEffects.None;
            return;
        }

        EnumDropType inputEffects = DropUtils.GetDropAction(e.KeyModifiers, (EnumDropType) e.DragEffects);
        e.DragEffects = DragDropEffects.None;
        if (inputEffects != EnumDropType.None && this.Track is Track target)
        {
            if (ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? resources) && resources.Count == 1 && resources[0] is ResourceItem item)
            {
                ResourceToClipDropRegistry dropRegistry = RZApplication.Instance.Services.GetService<ResourceToClipDropRegistry>();
                if (dropRegistry.TryGetValue(item.GetType(), out IResourceDropInformation? info))
                {
                    long duration = info.GetClipDurationForDrop(target, item);
                    if (duration > 0)
                    {
                        e.DragEffects = DragDropEffects.Copy;
                        this.DropFrameSpan = new FrameSpan(this.GetFrameAtMousePoint(e.GetPosition(this)), duration);
                    }
                }
            }
            // else if (e.Data.GetData(EffectProviderListBox.EffectProviderDropType) is EffectProviderEntry provider) {
            //     outputEffects = ClipDropRegistry.DropRegistry.CanDrop(target, provider, inputEffects);
            // }
        }
    }

    protected void OnDragLeave(DragEventArgs e)
    {
        // Dispatcher.UIThread.Invoke(() => this.ClearValue(IsDroppableTargetOverProperty), DispatcherPriority.Loaded);
        if (!this.IsPointerOver)
        {
            this.DropFrameSpan = null;
        }
    }

    protected async void OnDrop(DragEventArgs e)
    {
        e.Handled = true;
        if (this.isProcessingAsyncDrop || !(this.Track is Track track) || e.DragEffects == DragDropEffects.None)
        {
            return;
        }

        try
        {
            this.isProcessingAsyncDrop = true;
            if (ResourceTreeViewItem.GetResourceListFromDragEvent(e, out List<BaseResource>? resources) && resources.Count == 1 && resources[0] is ResourceItem item)
            {
                ResourceToClipDropRegistry dropRegistry = RZApplication.Instance.Services.GetService<ResourceToClipDropRegistry>();
                if (dropRegistry.TryGetValue(item.GetType(), out IResourceDropInformation? info))
                {
                    long duration = info.GetClipDurationForDrop(track, item);
                    if (duration > 0)
                    {
                        await info.OnDroppedInTrack(track, item, new FrameSpan(this.GetFrameAtMousePoint(e.GetPosition(this)), duration));
                    }
                }
            }
        }
        finally
        {
            this.isProcessingAsyncDrop = false;
            this.DropFrameSpan = null;
        }
    }

    #endregion
}