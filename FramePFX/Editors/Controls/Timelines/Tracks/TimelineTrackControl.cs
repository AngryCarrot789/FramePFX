using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Editors.Controls.Automation;
using FramePFX.Editors.Controls.Binders;
using FramePFX.Editors.Controls.Timelines.Tracks.Clips;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
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
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TimelineTrackControl), new PropertyMetadata(BoolBox.False));

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
        public TimelineControl OwnerTimeline { get; private set; }

        /// <summary>
        /// Gets the panel which stores this track's clip items
        /// </summary>
        public ClipStoragePanel StoragePanel { get; private set; }

        /// <summary>
        /// Gets this track's automation editor
        /// </summary>
        public AutomationSequenceEditor AutomationEditor { get; private set; }

        public Track Track { get; private set; }

        private readonly GetSetAutoPropertyBinder<Track> isSelectedBinder = new GetSetAutoPropertyBinder<Track>(IsSelectedProperty, nameof(VideoTrack.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v);
        private MovedClip? clipBeingMoved;
        private Visibility desiredAutomationVisibility;

        public TimelineTrackControl() {
            this.HorizontalAlignment = HorizontalAlignment.Stretch;
            this.VerticalAlignment = VerticalAlignment.Top;
            this.TrackColourBrush = new LinearGradientBrush();
            this.UseLayoutRounding = true;
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.StoragePanel = (ClipStoragePanel) this.GetTemplateChild("PART_TrackClipPanel") ?? throw new Exception("Missing PART_TrackClipPanel");
            this.StoragePanel.Track = this;
            this.AutomationEditor = (AutomationSequenceEditor) this.GetTemplateChild("PART_AutomationSequenceEditor") ?? throw new Exception("Missing PART_AutomationSequenceEditor");
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseDown(e);
            if (this.Track != null) {
                if (this.Track.Timeline.HasAnySelectedTracks)
                    this.Track.Timeline.ClearTrackSelection();
                this.Track.IsSelected = true;
            }
        }

        static TimelineTrackControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TimelineTrackControl), new FrameworkPropertyMetadata(typeof(TimelineTrackControl)));
        }

        private void OnClipAdded(Track track, Clip clip, int index) {
            this.StoragePanel.InsertClip(clip, index);
        }

        private void OnClipRemoved(Track track, Clip clip, int index) {
            this.StoragePanel.RemoveClipInternal(index);
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

                TimelineClipControl control = (TimelineClipControl) this.StoragePanel.MyInternalChildren[oldIndex];
                this.StoragePanel.RemoveClipInternal(oldIndex, false);
                dstTrack.clipBeingMoved = new MovedClip(control, clip);
            }
            else if (newTrack == this.Track) {
                if (!(this.clipBeingMoved is MovedClip movedClip)) {
                    throw new Exception("Clip control being moved is null. Is the UI timeline corrupted or did the clip move between timelines?");
                }

                this.StoragePanel.InsertClip(movedClip.control, movedClip.clip, newIndex);
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
            this.OwnerTimeline = parent.TimelineControl ?? throw new Exception("Parent track panel has no timeline control associated with it");
            this.OwnerPanel = parent;
            this.Track = track;

            track.ClipAdded += this.OnClipAdded;
            track.ClipRemoved += this.OnClipRemoved;
            track.ClipMovedTracks += this.OnClipMovedTracks;
            track.HeightChanged += this.OnTrackHeightChanged;
            track.ColourChanged += this.OnTrackColourChanged;
            UIInputManager.SetActionSystemDataContext(this, new DataContext().Set(DataKeys.TrackKey, track));
        }

        public void OnAdded() {
            this.isSelectedBinder.Attach(this, this.Track);
            this.UpdateTrackColour();
            int i = 0;
            foreach (Clip clip in this.Track.Clips) {
                this.StoragePanel.InsertClip(clip, i++);
            }
        }

        public void OnRemoving() {
            this.Track.ClipAdded -= this.OnClipAdded;
            this.Track.ClipRemoved -= this.OnClipRemoved;
            this.Track.ClipMovedTracks -= this.OnClipMovedTracks;
            this.Track.HeightChanged -= this.OnTrackHeightChanged;
            this.Track.ColourChanged -= this.OnTrackColourChanged;
            this.isSelectedBinder.Detatch();
            this.StoragePanel.ClearClipsInternal();
            UIInputManager.ClearActionSystemDataContext(this);
        }

        public void OnRemoved() {
            this.OwnerPanel = null;
            this.Track = null;
        }

        public void OnIndexMoving(int oldIndex, int newIndex) {

        }

        public void OnIndexMoved(int oldIndex, int newIndex) {

        }

        public void OnZoomChanged(double newZoom) {
            foreach (TimelineClipControl clip in this.StoragePanel.MyInternalChildren) {
                clip.OnZoomChanged(newZoom);
            }
        }

        private void OnTrackHeightChanged(Track track) {
            this.UpdateAutomationVisibility();
            this.InvalidateMeasure();
            this.StoragePanel.InvalidateMeasure();
            this.OwnerPanel?.InvalidateVisual();
        }

        private void OnTrackColourChanged(Track track) {
            this.UpdateTrackColour();
            foreach (TimelineClipControl clip in this.StoragePanel.MyInternalChildren) {
                clip.InvalidateVisual();
            }
        }

        public void OnClipSpanChanged() {
            this.StoragePanel.InvalidateArrange();
        }

        public IEnumerable<TimelineClipControl> GetClips() => this.StoragePanel.GetClips();

        public void SetAutomationVisibility(Visibility visibility) {
            this.desiredAutomationVisibility = visibility;
            this.UpdateAutomationVisibility();
        }

        private void UpdateAutomationVisibility() {
            if (DoubleUtils.AreClose(this.Track.Height, Track.MinimumHeight)) {
                this.AutomationEditor.Visibility = Visibility.Collapsed;
            }
            else {
                this.AutomationEditor.Visibility = this.desiredAutomationVisibility;
            }
        }
    }
}
