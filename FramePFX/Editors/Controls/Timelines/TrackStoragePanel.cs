using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Editors.Controls.Timelines.Tracks;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Controls.Timelines {
    /// <summary>
    /// A stack panel based control, that stacks a collection of tracks on top of each other,
    /// with a 1 pixel gap between each track. This is what presents a timeline's actual tracks
    /// </summary>
    public class TrackStoragePanel : StackPanel {
        public static readonly DependencyProperty TimelineProperty =
            DependencyProperty.Register(
                "Timeline",
                typeof(Timeline),
                typeof(TrackStoragePanel),
                new PropertyMetadata(null, (d, e) => ((TrackStoragePanel) d).OnTimelineChanged((Timeline) e.OldValue, (Timeline) e.NewValue)));

        /// <summary>
        /// The model used to present the tracks, clips, etc. Event handlers will be added and removed when necessary
        /// </summary>
        public Timeline Timeline {
            get => (Timeline) this.GetValue(TimelineProperty);
            set => this.SetValue(TimelineProperty, value);
        }

        /// <summary>
        /// Gets or sets the timeline control that this sequence is placed in
        /// </summary>
        public TimelineControl TimelineControl { get; internal set; }

        private readonly Stack<TimelineTrackControl> cachedTracks;

        public TrackStoragePanel() {
            this.cachedTracks = new Stack<TimelineTrackControl>();
        }

        public void SetPlayHeadToMouseCursor(MouseDevice device) {
            if (this.TimelineControl != null) {
                Point point = device.GetPosition(this);
                this.TimelineControl.SetPlayHeadToMouseCursor(point.X);
            }
        }

        static TrackStoragePanel() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TrackStoragePanel), new FrameworkPropertyMetadata(typeof(TrackStoragePanel)));
        }

        public void OnZoomChanged(double newZoom) {
            // this.InvalidateMeasure();
            foreach (TimelineTrackControl track in this.InternalChildren) {
                track.OnZoomChanged(newZoom);
            }
        }

        private void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            if (oldTimeline == newTimeline)
                return;
            if (oldTimeline != null) {
                oldTimeline.TrackAdded -= this.OnTrackAdded;
                oldTimeline.TrackRemoved -= this.OnTrackRemoved;
                oldTimeline.TrackMoved -= this.OnTrackIndexMoved;
                oldTimeline.MaxDurationChanged -= this.OnMaxDurationChanged;
                for (int i = this.InternalChildren.Count - 1; i >= 0; i--) {
                    this.RemoveTrackInternal(i);
                }
            }

            if (newTimeline != null) {
                newTimeline.TrackAdded += this.OnTrackAdded;
                newTimeline.TrackRemoved += this.OnTrackRemoved;
                newTimeline.TrackMoved += this.OnTrackIndexMoved;
                newTimeline.MaxDurationChanged += this.OnMaxDurationChanged;
                int i = 0;
                foreach (Track track in newTimeline.Tracks) {
                    this.InsertTrackInternal(track, i++);
                }
            }
        }

        private void OnMaxDurationChanged(Timeline timeline) => this.InvalidateMeasure();

        private void OnTrackAdded(Timeline timeline, Track track, int index) {
            this.InsertTrackInternal(track, index);
        }

        private void OnTrackRemoved(Timeline timeline, Track track, int index) {
            this.RemoveTrackInternal(index);
        }

        private void OnTrackIndexMoved(Timeline timeline, Track track, int oldIndex, int newIndex) {
            TimelineTrackControl control = (TimelineTrackControl) this.InternalChildren[oldIndex];
            control.OnIndexMoving(oldIndex, newIndex);
            this.InternalChildren.RemoveAt(oldIndex);
            this.InternalChildren.Insert(newIndex, control);
            control.OnIndexMoved(oldIndex, newIndex);
            this.InvalidateMeasure();
        }

        private void InsertTrackInternal(Track track, int index) {
            TimelineTrackControl control = this.cachedTracks.Count > 0 ? this.cachedTracks.Pop() : new TimelineTrackControl();
            control.OwnerPanel = this;
            control.OnAdding(this, track);
            this.InternalChildren.Insert(index, control);
            control.InvalidateMeasure();
            control.ApplyTemplate();
            control.OnAdded();
            this.TimelineControl.UpdateTrackAutomationVisibility(control);
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        private void RemoveTrackInternal(int index) {
            TimelineTrackControl control = (TimelineTrackControl) this.InternalChildren[index];
            control.OnRemoving();
            this.InternalChildren.RemoveAt(index);
            control.OnRemoved();
            if (this.cachedTracks.Count < 4)
                this.cachedTracks.Push(control);
            this.InvalidateMeasure();
            this.InvalidateVisual();
        }

        protected override Size MeasureOverride(Size availableSize) {
            double totalHeight = 0d;
            double maxWidth = 0d;
            UIElementCollection items = this.InternalChildren;
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                TimelineTrackControl track = (TimelineTrackControl) items[i];
                track.Measure(availableSize);
                totalHeight += track.DesiredSize.Height;
                maxWidth = Math.Max(maxWidth, track.RenderSize.Width);
            }

            Timeline timeline = this.Timeline;

            // the gap between tracks, only when there's 2 or more tracks obviously
            if (count > 1) {
                totalHeight += count - 1;
            }

            return new Size(timeline != null ? (timeline.Zoom * timeline.MaxDuration) : maxWidth, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize) {
            double totalY = 0d;
            UIElementCollection items = this.InternalChildren;
            for (int i = 0, count = items.Count; i < count; i++) {
                TimelineTrackControl track = (TimelineTrackControl) items[i];
                track.Arrange(new Rect(new Point(0, totalY), new Size(finalSize.Width, track.DesiredSize.Height)));
                totalY += track.RenderSize.Height + 1d; // +1d for the gap between tracks
            }

            return finalSize;
        }

        /// <summary>
        /// Gets a track control from a model, or null if one does not exist
        /// </summary>
        /// <param name="track">The model</param>
        /// <returns>The control</returns>
        public TimelineTrackControl GetTrackByModel(Track track) {
            UIElementCollection list = this.InternalChildren;
            for (int i = 0, count = list.Count; i < count; i++) {
                TimelineTrackControl control = (TimelineTrackControl) list[i];
                if (control.Track == track) {
                    return control;
                }
            }

            return null;
        }

        public IEnumerable<TimelineTrackControl> GetTracks() => this.InternalChildren.Cast<TimelineTrackControl>();
    }
}
