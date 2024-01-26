using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FramePFX.Destroying;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editors.Timelines {
    public delegate void TimelineTrackIndexEventHandler(Timeline timeline, Track track, int index);
    public delegate void TimelineTrackMovedEventHandler(Timeline timeline, Track track, int oldIndex, int newIndex);
    public delegate void TimelineEventHandler(Timeline timeline);
    public delegate void PlayheadChangedEventHandler(Timeline timeline, long oldValue, long newValue);
    public delegate void ZoomEventHandler(Timeline timeline, double oldZoom, double newZoom, ZoomType zoomType);

    public class Timeline : IDestroy {
        public Project Project { get; private set; }

        public TrackPoint RangedSelectionAnchor { get; set; } = TrackPoint.Invalid;

        public ReadOnlyCollection<Track> Tracks { get; }


        /// <summary>
        /// Gets or sets the total length of all tracks, in frames. This is incremented on demand when necessary, and is used for UI calculations
        /// </summary>
        public long MaxDuration {
            get => this.maxDuration;
            set {
                if (this.maxDuration == value)
                    return;
                this.maxDuration = value;
                this.MaxDurationChanged?.Invoke(this);
            }
        }

        public long StopHeadPosition {
            get => this.stopHeadPosition;
            set {
                if (this.stopHeadPosition == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Stophead cannot be negative");
                if (value >= this.maxDuration)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Stophead exceeds the timeline duration range (0 to TotalFrames)");

                long oldStopHead = this.stopHeadPosition;
                this.stopHeadPosition = value;
                this.StopHeadChanged?.Invoke(this, oldStopHead, value);
            }
        }

        /// <summary>
        /// The position of the play head, in frames
        /// </summary>
        public long PlayHeadPosition {
            get => this.playHeadPosition;
            set {
                if (this.playHeadPosition == value)
                    return;

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Playhead cannot be negative");
                if (value >= this.maxDuration)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Playhead exceeds the timeline duration range (0 to TotalFrames)");

                long oldPlayHead = this.playHeadPosition;
                this.playHeadPosition = value;
                this.PlayHeadChanged?.Invoke(this, oldPlayHead, value);
                AutomationEngine.UpdateValues(this);
            }
        }

        public long LargestFrameInUse {
            get => this.largestFrameInUse;
            private set {
                if (this.largestFrameInUse == value)
                    return;
                this.largestFrameInUse = value;
                this.LargestFrameInUseChanged?.Invoke(this);
            }
        }

        /// <summary>
        /// Returns an enumerable of selected tracks
        /// </summary>
        public ReadOnlyCollection<Track> SelectedTracks { get; }

        /// <summary>
        /// Returns an enumerable of all selected clips in all tracks
        /// </summary>
        public IEnumerable<Clip> SelectedClips => this.tracks.SelectMany(t => t.SelectedClips);

        /// <summary>
        /// Returns the track selection type based on how many tracks are selected.
        /// Does not require enumerating the tracks as track selection is cached
        /// </summary>
        public SelectionType TrackSelectionType {
            get {
                int count = this.selectedTracks.Count;
                if (count > 1)
                    return SelectionType.Multi;
                return count == 1 ? SelectionType.Single : SelectionType.None;
            }
        }

        /// <summary>
        /// Returns the clip selection type based on how many clips are selected in all tracks combined.
        /// This may require enumerating all tracks, but not all clips (since selected clips are cached)
        /// </summary>
        public SelectionType ClipSelectionType {
            get {
                int count = 0;
                foreach (Track track in this.tracks) {
                    count += track.SelectedClipCount;
                    if (count > 1) {
                        return SelectionType.Multi;
                    }
                }

                return count == 1 ? SelectionType.Single : SelectionType.None;
            }
        }

        /// <summary>
        /// Returns true when there is at least one selected clips in any track. This may
        /// require enumerating all tracks, but not all clips (since selected clips are cached)
        /// </summary>
        public bool HasAnySelectedClips => this.tracks.Any(track => track.SelectedClipCount > 0);

        public bool HasAnySelectedTracks => this.selectedTracks.Count > 0;

        public double Zoom { get; private set; }

        public event TimelineTrackIndexEventHandler TrackAdded;
        public event TimelineTrackIndexEventHandler TrackRemoved;
        public event TimelineTrackMovedEventHandler TrackMoved;
        public event TimelineEventHandler MaxDurationChanged;
        public event TimelineEventHandler LargestFrameInUseChanged;
        public event PlayheadChangedEventHandler PlayHeadChanged;
        public event PlayheadChangedEventHandler StopHeadChanged;
        public event ZoomEventHandler ZoomTimeline;

        private readonly List<Track> tracks;
        private readonly List<Track> selectedTracks;
        private long maxDuration;
        private long playHeadPosition;
        private long stopHeadPosition;
        private long largestFrameInUse;

        public Timeline() {
            this.tracks = new List<Track>();
            this.Tracks = new ReadOnlyCollection<Track>(this.tracks);
            this.selectedTracks = new List<Track>();
            this.SelectedTracks = this.selectedTracks.AsReadOnly();
            this.maxDuration = 5000L;
            this.Zoom = 1.0d;
        }

        public void UpdateLargestFrame() {
            IReadOnlyList<Track> list = this.Tracks;
            int count = list.Count;
            if (count > 0) {
                long max = list[0].LargestFrameInUse;
                for (int i = 1; i < count; i++) {
                    max = Math.Max(max, list[i].LargestFrameInUse);
                }

                this.LargestFrameInUse = max;
            }
            else {
                this.LargestFrameInUse = 0;
            }
        }

        public void SetZoom(double zoom, ZoomType type) {
            double oldZoom = this.Zoom;
            if (zoom > 200.0) {
                zoom = 200;
            }
            else if (zoom < 0.1) {
                zoom = 0.1;
            }

            if (Maths.Equals(oldZoom, zoom)) {
                return;
            }

            this.Zoom = zoom;
            this.ZoomTimeline?.Invoke(this, oldZoom, zoom, type);
        }

        public void AddTrack(Track track) => this.InsertTrack(this.tracks.Count, track);

        public void InsertTrack(int index, Track track) {
            if (this.tracks.Contains(track))
                throw new InvalidOperationException("This track already contains the track");
            this.tracks.Insert(index, track);
            if (track.IsSelected)
                this.selectedTracks.Add(track);

            // update anchor
            TrackPoint anchor = this.RangedSelectionAnchor;
            if (anchor.TrackIndex != -1) {
                if (index <= anchor.TrackIndex) {
                    this.RangedSelectionAnchor = new TrackPoint(anchor.Frame, anchor.TrackIndex + 1);
                }
            }

            Track.OnAddedToTimeline(track, this);
            this.TrackAdded?.Invoke(this, track, index);
            this.UpdateLargestFrame();
        }

        public bool RemoveTrack(Track track) {
            int index = this.tracks.IndexOf(track);
            if (index == -1)
                return false;
            this.RemoveTrackAt(index);
            return true;
        }

        public void RemoveTrackAt(int index) {
            Track track = this.tracks[index];
            this.tracks.RemoveAt(index);
            if (track.IsSelected)
                this.selectedTracks.Remove(track);

            // update anchor
            TrackPoint anchor = this.RangedSelectionAnchor;
            if (anchor.TrackIndex != -1) {
                if (this.tracks.Count == 0) {
                    this.RangedSelectionAnchor = TrackPoint.Invalid;
                }
                else if (index <= anchor.TrackIndex) {
                    this.RangedSelectionAnchor = new TrackPoint(anchor.Frame, anchor.TrackIndex - 1);
                }
            }

            Track.OnRemovedFromTimeline1(track, this);
            this.TrackRemoved?.Invoke(this, track, index);
            Track.OnRemovedFromTimeline2(this, track, index);
            this.UpdateLargestFrame();
        }

        public void MoveTrackIndex(int oldIndex, int newIndex) {
            if (oldIndex != newIndex) {
                this.tracks.MoveItem(oldIndex, newIndex);

                // update anchor
                TrackPoint anchor = this.RangedSelectionAnchor;
                if (anchor.TrackIndex != -1 && anchor.TrackIndex == oldIndex) {
                    this.RangedSelectionAnchor = new TrackPoint(anchor.Frame, newIndex);
                }

                this.TrackMoved?.Invoke(this, this.tracks[newIndex], oldIndex, newIndex);
            }
        }

        public virtual void Destroy() {
            for (int i = this.tracks.Count - 1; i >= 0; i--) {
                Track track = this.tracks[i];
                this.RemoveTrackAt(i);
                track.Destroy();
            }
        }

        // Called by the track directly, in order to guarantee that selection is
        // handled before any track IsSelectedChanged event handlers
        public static void OnIsTrackSelectedChanged(Track track) {
            // See comment about the track's version of this method
            if (track.Timeline == null) {
                return;
            }

            List<Track> selected = track.Timeline.selectedTracks;
            if (track.IsSelected) {
                selected.Add(track);
            }
            else if (selected.Count > 0) {
                if (selected[0] == track) {
                    selected.RemoveAt(0);
                }
                else { // assume back to front removal
                    int index = selected.LastIndexOf(track);
                    if (index == -1) {
                        throw new Exception("Track was never selected");
                    }

                    selected.RemoveAt(index);
                }
            }
        }

        public static void SetMainTimelineProjectReference(Timeline timeline, Project project) {
            // no need to tell clips or tracks that our project changed, since there is guaranteed
            // to be none, unless this method is called outside of the project's constructor
            timeline.Project = project;
        }

        // TODO: composition timelines
        public static void SetCompositionTimelineProjectReference(Timeline timeline, Project project) {

        }

        /// <summary>
        /// Sets all tracks to not-selected
        /// </summary>
        public void ClearTrackSelection() {
            List<Track> list = this.selectedTracks;
            for (int i = list.Count - 1; i >= 0; i--) {
                list[i].IsSelected = false;
            }
        }

        /// <summary>
        /// Sets all clips in all tracks to not-selected
        /// </summary>
        public void ClearClipSelection(Clip except = null) {
            foreach (Track track in this.tracks) {
                track.ClearClipSelection(except);
            }
        }

        public void MakeSingleSelection(Clip clipToSelect) {
            this.ClearClipSelection(clipToSelect);
            clipToSelect.IsSelected = true;
        }

        public void MakeFrameRangeSelection(FrameSpan span, int trackSrcIdx = -1, int trackEndIndex = -1) {
            this.ClearClipSelection();
            List<Clip> clips = new List<Clip>();
            if (trackSrcIdx == -1 || trackEndIndex == -1) {
                foreach (Track track in this.tracks) {
                    track.CollectClipsInSpan(clips, span);
                }
            }
            else {
                for (int i = trackSrcIdx; i < trackEndIndex; i++) {
                    this.tracks[i].CollectClipsInSpan(clips, span);
                }
            }

            foreach (Clip clip in clips) {
                clip.IsSelected = true;
            }
        }

        internal static void OnIsClipSelectedChanged(Clip clip) {
            // UI modifies the anchor directly
            // clip.Track.Timeline.RangedSelectionAnchor = clip.IsSelected ? clip : null;
        }

        internal static void OnClipRemovedFromTrack(Track track, Clip clip) {
            // if (track.Timeline.RangedSelectionAnchorClip == clip) {
            //     track.Timeline.RangedSelectionAnchorClip = null;
            // }
        }

        public void InvalidateRender() {
            this.Project?.RenderManager.InvalidateRender();
        }

        public static void OnTrackSelectionCleared(Track track) {

        }
    }
}