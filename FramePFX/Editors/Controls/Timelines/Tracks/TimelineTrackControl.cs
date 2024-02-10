﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.AdvancedContextService.WPF;
using FramePFX.Editors.Contextual;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Resources;
using FramePFX.Editors.Controls.Timelines.Tracks.Clips;
using FramePFX.Editors.ResourceManaging;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity;
using FramePFX.Interactivity.DataContexts;
using FramePFX.PropertyEditing;
using FramePFX.Shortcuts.WPF;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Timelines.Tracks {
    /// <summary>
    /// A control which represents a track in a timeline sequence
    /// </summary>
    public class TimelineTrackControl : Control {
        private readonly struct MovedClip {
            public readonly TimelineClipControl control;
            public readonly Clip clip;

            public MovedClip(TimelineClipControl control, Clip clip) {
                this.control = control;
                this.clip = clip;
            }
        }

        private static readonly DependencyPropertyKey TrackColourBrushPropertyKey = DependencyProperty.RegisterReadOnly("TrackColourBrush", typeof(Brush), typeof(TimelineTrackControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty TrackColourBrushProperty = TrackColourBrushPropertyKey.DependencyProperty;
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimelineTrackControl), new PropertyMetadata(BoolBox.False, (d, e) => ((TimelineTrackControl) d).OnIsSelectedChanged((bool) e.NewValue)));

        public Brush TrackColourBrush {
            get => (Brush) this.GetValue(TrackColourBrushProperty);
            private set => this.SetValue(TrackColourBrushPropertyKey, value);
        }

        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value.Box());
        }

        /// <summary>
        /// Gets the timeline sequence that stores this track
        /// </summary>
        public TrackStoragePanel OwnerPanel { get; set; }

        /// <summary>
        /// Gets our panel's timeline control
        /// </summary>
        public TimelineControl TimelineControl { get; private set; }

        /// <summary>
        /// Gets the panel which stores this track's clip items
        /// </summary>
        public ClipStoragePanel ClipStoragePanel { get; private set; }

        /// <summary>
        /// Gets this track's automation editor
        /// </summary>
        public AutomationSequenceEditor AutomationEditor { get; private set; }

        public Track Track { get; private set; }

        private bool isUpdatingSelectedProperty;
        private bool isProcessingAsyncDrop;
        private MovedClip? clipBeingMoved;
        public Visibility desiredAutomationVisibility;
        private readonly DataContext actionSystemDataContext;

        public TimelineTrackControl() {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.TrackColourBrush = new LinearGradientBrush();
            this.UseLayoutRounding = true;
            this.AllowDrop = true;
            this.Focusable = true;
            UIInputManager.SetActionSystemDataContext(this, this.actionSystemDataContext = new DataContext());
            AdvancedContextMenu.SetContextGenerator(this, TrackContextRegistry.Instance);
        }

        public long GetFrameAtMousePoint(MouseDevice device) {
            return this.GetFrameAtMousePoint(device.GetPosition(this));
        }

        public long GetFrameAtMousePoint(Point pointRelativeToThis) {
            return TimelineUtils.PixelToFrame(pointRelativeToThis.X, this.TimelineControl?.Timeline?.Zoom ?? 1.0, true);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.ClipStoragePanel = (ClipStoragePanel) this.GetTemplateChild("PART_TrackClipPanel") ?? throw new Exception("Missing PART_TrackClipPanel");
            this.ClipStoragePanel.Track = this;
            this.AutomationEditor = (AutomationSequenceEditor) this.GetTemplateChild("PART_AutomationSequenceEditor") ?? throw new Exception("Missing PART_AutomationSequenceEditor");
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (this.Track != null) {
                Timeline timeline = this.Track.Timeline;

                // update context data, used by action system and context menu system
                if (e.ChangedButton == MouseButton.Left || e.ChangedButton == MouseButton.Right) {
                    this.actionSystemDataContext.Set(DataKeys.TrackContextMouseFrameKey, this.GetFrameAtMousePoint(e.MouseDevice));
                }

                if (timeline != null && timeline.HasAnySelectedTracks)
                    timeline.ClearTrackSelection();

                // don't focus if the click hit a clip, since the clip will be focused right after so it's pointless
                this.Track.SetIsSelected(true, !(e.OriginalSource is TimelineClipControl));
                VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
            }
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e) {
            base.OnDragOver(e);
            e.Handled = true;
            this.actionSystemDataContext.Set(DataKeys.TrackDropFrameKey, this.GetFrameAtMousePoint(e.GetPosition(this)));
            if (this.isProcessingAsyncDrop || this.Track == null) {
                e.Effects = DragDropEffects.None;
                return;
            }

            EnumDropType inputEffects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (inputEffects == EnumDropType.None) {
                e.Effects = DragDropEffects.None;
                return;
            }

            EnumDropType outputEffects = EnumDropType.None;
            if (e.Data.GetData(ResourceDropRegistry.ResourceDropType) is List<BaseResource> resources) {
                if (resources.Count == 1 && resources[0] is ResourceItem) {
                    outputEffects = TrackDropRegistry.DropRegistry.CanDrop(this.Track, resources[0], inputEffects, this.actionSystemDataContext);
                }
            }
            else {
                outputEffects = TrackDropRegistry.DropRegistry.CanDropNative(this.Track, new DataObjectWrapper(e.Data), inputEffects, this.actionSystemDataContext);
            }

            e.Effects = (DragDropEffects) outputEffects;
        }

        protected override async void OnDrop(DragEventArgs e) {
            base.OnDrop(e);
            e.Handled = true;
            if (this.isProcessingAsyncDrop || this.Track == null) {
                return;
            }

            EnumDropType effects = DropUtils.GetDropAction((int) e.KeyStates, (EnumDropType) e.Effects);
            if (e.Effects == DragDropEffects.None) {
                return;
            }

            try {
                this.isProcessingAsyncDrop = true;
                if (e.Data.GetData(ResourceDropRegistry.ResourceDropType) is List<BaseResource> items) {
                    if (items.Count == 1 && items[0] is ResourceItem) {
                        await TrackDropRegistry.DropRegistry.OnDropped(this.Track, items[0], effects, this.actionSystemDataContext);
                    }
                }
                // else if (e.Data.GetData(EffectProviderTreeViewItem.ProviderDropType) is EffectProvider provider) {
                //     await Track.DropRegistry.OnDropped(track, provider, effects, this.GetDropDataContext(e.GetPosition(this)));
                // }
                else {
                    await TrackDropRegistry.DropRegistry.OnDroppedNative(this.Track, new DataObjectWrapper(e.Data), effects, this.actionSystemDataContext);
                }
            }
            finally {
                this.isProcessingAsyncDrop = false;
            }
        }

        static TimelineTrackControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineTrackControl), new FrameworkPropertyMetadata(typeof(TimelineTrackControl)));
        }

        private void OnClipAdded(Track track, Clip clip, int index) {
            this.ClipStoragePanel.InsertClip(clip, index);
        }

        private void OnClipRemoved(Track track, Clip clip, int index) {
            this.ClipStoragePanel.RemoveClipInternal(index);
        }

        private void OnClipMovedTracks(Clip clip, Track oldTrack, int oldIndex, Track newTrack, int newIndex) {
            if (oldTrack == this.Track) {
                TimelineTrackControl dstTrack = this.OwnerPanel.GetTrackByModel(newTrack);
                if (dstTrack == null) {
                    // Instead of throwing, we could just remove the track or insert a new track, instead of
                    // trying to re-use existing controls, at the cost of performance.
                    // However, moving clips between tracks in different timelines is not directly supported
                    // so there's no need to support it here
                    throw new Exception("Could not find destination track. Is the UI timeline corrupted or did the clip move between timelines?");
                }

                TimelineClipControl control = (TimelineClipControl) this.ClipStoragePanel.MyInternalChildren[oldIndex];
                this.ClipStoragePanel.RemoveClipInternal(oldIndex, false);
                dstTrack.clipBeingMoved = new MovedClip(control, clip);
            }
            else if (newTrack == this.Track) {
                if (!(this.clipBeingMoved is MovedClip movedClip)) {
                    throw new Exception("Clip control being moved is null. Is the UI timeline corrupted or did the clip move between timelines?");
                }

                this.ClipStoragePanel.InsertClip(movedClip.control, movedClip.clip, newIndex);
                this.clipBeingMoved = null;
            }
        }

        private void UpdateTrackColour() {
            if (this.Track == null) {
                return;
            }

            SKColor col = this.Track.Colour;
            // ((SolidColorBrush) this.TrackColourBrush).Color = Color.FromArgb(col.Alpha, col.Red, col.Green, col.Blue);

            LinearGradientBrush brush = (LinearGradientBrush) this.TrackColourBrush;
            brush.StartPoint = new Point(0, 0);
            brush.EndPoint = new Point(1, 0);

            // const byte sub = 40;
            const byte sub = 80;
            Color primary = Color.FromArgb(col.Alpha, col.Red, col.Green, col.Blue);
            Color secondary = Color.FromArgb(col.Alpha, (byte) Math.Max(col.Red - sub, 0), (byte) Math.Max(col.Green - sub, 0), (byte) Math.Max(col.Blue - sub, 0));

            brush.GradientStops.Clear();
            brush.GradientStops.Add(new GradientStop(primary, 0.0));
            brush.GradientStops.Add(new GradientStop(secondary, 1.0));
        }

        public void OnAdding(TrackStoragePanel parent, Track track) {
            this.TimelineControl = parent.TimelineControl ?? throw new Exception("Parent track panel has no timeline control associated with it");
            this.OwnerPanel = parent;
            this.Track = track;

            track.ClipAdded += this.OnClipAdded;
            track.ClipRemoved += this.OnClipRemoved;
            track.ClipMovedTracks += this.OnClipMovedTracks;
            track.HeightChanged += this.OnTrackHeightChanged;
            track.ColourChanged += this.OnTrackColourChanged;
            this.Track.IsSelectedChanged += this.TrackOnIsSelectedChanged;
            this.actionSystemDataContext.Set(DataKeys.TrackKey, track);
        }

        public void OnAdded() {
            this.UpdateTrackColour();
            int i = 0;
            foreach (Clip clip in this.Track.Clips) {
                this.ClipStoragePanel.InsertClip(clip, i++);
            }

            this.IsSelected = this.Track?.IsSelected ?? false;
        }

        public void OnRemoving() {
            this.Track.ClipAdded -= this.OnClipAdded;
            this.Track.ClipRemoved -= this.OnClipRemoved;
            this.Track.ClipMovedTracks -= this.OnClipMovedTracks;
            this.Track.HeightChanged -= this.OnTrackHeightChanged;
            this.Track.ColourChanged -= this.OnTrackColourChanged;
            this.Track.IsSelectedChanged -= this.TrackOnIsSelectedChanged;
            this.ClipStoragePanel.ClearClipsInternal();
            this.actionSystemDataContext.Set(DataKeys.TrackKey, null);
        }

        public void OnRemoved() {
            this.OwnerPanel = null;
            this.Track = null;
        }

        public void OnIndexMoving(int oldIndex, int newIndex) {

        }

        public void OnIndexMoved(int oldIndex, int newIndex) {

        }

        private void OnIsSelectedChanged(bool selected) {
            if (this.isUpdatingSelectedProperty)
                return;
            this.Track.SetIsSelected(selected, false);
        }

        private void TrackOnIsSelectedChanged(Track track, bool isPrimarySelection) {
            try {
                this.isUpdatingSelectedProperty = true;
                this.IsSelected = track.IsSelected;
                if (track.IsSelected && isPrimarySelection) {
                    this.Focus();
                }
            }
            finally {
                this.isUpdatingSelectedProperty = false;
            }
        }

        public void OnZoomChanged(double newZoom) {
            // this.InvalidateMeasure();
            foreach (TimelineClipControl clip in this.ClipStoragePanel.MyInternalChildren) {
                clip.OnZoomChanged(newZoom);
            }
        }

        private void OnTrackHeightChanged(Track track) {
            this.UpdateAutomationVisibility();
            this.InvalidateMeasure();
            this.ClipStoragePanel.InvalidateMeasure();
            this.OwnerPanel?.InvalidateVisual();
        }

        private void OnTrackColourChanged(Track track) {
            this.UpdateTrackColour();
            foreach (TimelineClipControl clip in this.ClipStoragePanel.MyInternalChildren) {
                clip.InvalidateVisual();
            }
        }

        public void OnClipSpanChanged() {
            this.ClipStoragePanel.InvalidateArrange();
        }

        public IEnumerable<TimelineClipControl> GetClips() => this.ClipStoragePanel.GetClips();
        public TimelineClipControl GetClipAt(int index) => this.ClipStoragePanel.GetClipAt(index);

        public void SetAutomationVisibility(Visibility visibility) {
            this.desiredAutomationVisibility = visibility;
            this.UpdateAutomationVisibility();
        }

        public void UpdateAutomationVisibility() {
            if (DoubleUtils.AreClose(this.Track.Height, Track.MinimumHeight)) {
                this.AutomationEditor.Visibility = Visibility.Collapsed;
            }
            else {
                this.AutomationEditor.Visibility = this.desiredAutomationVisibility;
            }
        }
    }
}