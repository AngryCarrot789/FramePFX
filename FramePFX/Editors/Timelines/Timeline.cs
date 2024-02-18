using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using FramePFX.Editors.Automation;
using FramePFX.Editors.DataTransfer;
using FramePFX.Editors.Factories;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.RBC;
using FramePFX.Utils;
using FramePFX.Utils.Destroying;

namespace FramePFX.Editors.Timelines {
    public delegate void TimelineTrackIndexEventHandler(Timeline timeline, Track track, int index);
    public delegate void TimelineTrackMovedEventHandler(Timeline timeline, Track track, int oldIndex, int newIndex);
    public delegate void TimelineEventHandler(Timeline timeline);
    public delegate void PlayheadChangedEventHandler(Timeline timeline, long oldValue, long newValue);
    public delegate void ZoomEventHandler(Timeline timeline, double oldZoom, double newZoom, ZoomType zoomType);

    public class Timeline : ITransferableData, IDestroy {
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
                AutomationEngine.UpdateValues(this, value);
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

        public bool IsActive { get; private set; }

        public RenderManager RenderManager { get; }

        public TransferableData TransferableData { get; }

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
            this.TransferableData = new TransferableData(this);
            this.tracks = new List<Track>();
            this.Tracks = new ReadOnlyCollection<Track>(this.tracks);
            this.selectedTracks = new List<Track>();
            this.SelectedTracks = this.selectedTracks.AsReadOnly();
            this.maxDuration = 5000L;
            this.Zoom = 1.0d;
            this.RenderManager = new RenderManager(this);
        }

        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetLong(nameof(this.PlayHeadPosition), this.PlayHeadPosition);
            data.SetLong(nameof(this.StopHeadPosition), this.StopHeadPosition);
            data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
            RBEList list = data.CreateList(nameof(this.Tracks));
            foreach (Track track in this.tracks) {
                if (!(track.FactoryId is string registryId))
                    throw new Exception("Unknown track type: " + track.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Track.FactoryId), registryId);
                track.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            if (this.tracks.Count > 0) {
                throw new InvalidOperationException("Cannot read track RBE data while there are still tracks");
            }

            this.playHeadPosition = data.GetLong(nameof(this.PlayHeadPosition));
            this.stopHeadPosition = data.GetLong(nameof(this.StopHeadPosition));
            this.maxDuration = data.GetLong(nameof(this.MaxDuration));
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Tracks)).Cast<RBEDictionary>()) {
                string registryId = dictionary.GetString(nameof(Track.FactoryId));
                Track track = TrackFactory.Instance.NewTrack(registryId);
                track.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddTrack(track);
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            this.maxDuration = Math.Max(this.maxDuration, this.tracks.Count < 1 ? 0 : this.tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
            this.UpdateLargestFrame();
        }

        public void LoadDataIntoClone(Timeline clone) {
            if (this.tracks.Count > 0) {
                throw new InvalidOperationException("Cannot read track RBE data while there are still tracks");
            }

            clone.playHeadPosition = this.playHeadPosition;
            clone.stopHeadPosition = this.stopHeadPosition;
            clone.maxDuration = this.maxDuration;
            foreach (Track track in this.tracks) {
                clone.AddTrack(track.Clone());
            }

            // Recalculate a new max duration, just in case the clips somehow exceed the current value
            clone.maxDuration = this.maxDuration;
            clone.UpdateLargestFrame();
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

            if (this.largestFrameInUse > this.maxDuration) {
                this.MaxDuration = this.largestFrameInUse + 100;
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
            if (track == null)
                throw new ArgumentNullException(nameof(track), "Cannot add a null track");
            if (track.Timeline == this)
                throw new ArgumentException("Track already exists in this timeline. It must be removed first");
            if (track.Timeline != null)
                throw new ArgumentException("Track already exists in another timeline. It must be removed first");
            if (track.IndexInTimeline != -1)
                throw new InvalidOperationException("The track already exists in another timeline");
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

            this.UpdateIndexForInsertionOrRemoval(index);
            Track.InternalOnAddedToTimeline(track, this);
            this.TrackAdded?.Invoke(this, track, index);
            this.UpdateLargestFrame();
            this.InvalidateRender();
        }

        public bool RemoveTrack(Track track) {
            int index = track.IndexInTimeline;
            if (index == -1)
                return false;
            if (track.Timeline != this)
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

            this.UpdateIndexForInsertionOrRemoval(index);
            Track.InternalOnRemovedFromTimeline1(track, this);
            this.TrackRemoved?.Invoke(this, track, index);
            this.UpdateLargestFrame();
            this.InvalidateRender();
        }

        public void MoveTrackIndex(int oldIndex, int newIndex) {
            if (oldIndex != newIndex) {
                this.tracks.MoveItem(oldIndex, newIndex);
                this.UpdateIndexForTrackMove(oldIndex, newIndex);

                // update anchor
                TrackPoint anchor = this.RangedSelectionAnchor;
                if (anchor.TrackIndex != -1 && anchor.TrackIndex == oldIndex) {
                    this.RangedSelectionAnchor = new TrackPoint(anchor.Frame, newIndex);
                }

                this.TrackMoved?.Invoke(this, this.tracks[newIndex], oldIndex, newIndex);
                this.InvalidateRender();
            }
        }

        public virtual void Destroy() {
            // TODO: this is no good
            while (this.RenderManager.IsRendering)
                Thread.Sleep(1);
            using (this.RenderManager.SuspendRenderInvalidation()) {
                for (int i = this.tracks.Count - 1; i >= 0; i--) {
                    Track track = this.tracks[i];
                    this.RemoveTrackAt(i);
                    track.Destroy();
                }
            }

            this.RenderManager.Dispose();
        }

        private void UpdateIndexForInsertionOrRemoval(int index) {
            List<Track> list = this.tracks;
            for (int i = list.Count - 1; i >= index; i--) {
                Track.InternalSetPrecomputedTrackIndex(list[i], i);
            }
        }

        private void UpdateIndexForTrackMove(int oldIndex, int newIndex) {
            int min, max;
            if (newIndex < oldIndex) {
                min = newIndex;
                max = oldIndex;
            }
            else {
                min = oldIndex;
                max = newIndex;
            }

            Track.InternalSetPrecomputedTrackIndex(this.tracks[min], min);
            this.UpdateIndexForInsertionOrRemoval(max);
        }

        /// <summary>
        /// Sets all tracks to not-selected
        /// </summary>
        public void ClearTrackSelection() {
            List<Track> list = this.selectedTracks;
            for (int i = list.Count - 1; i >= 0; i--) {
                list[i].SetIsSelected(false);
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

        public void InvalidateRender() => this.RenderManager.InvalidateRender();

        public HashSet<Clip> GetSelectedClipsWith(Clip value) {
            HashSet<Clip> clips = new HashSet<Clip>(this.SelectedClips);
            if (value != null)
                clips.Add(value);
            return clips;
        }

        public int GetSelectedClipCountWith(Clip clip) {
            int count = 0;
            foreach (Track track in this.tracks) {
                count += track.SelectedClipCount;
            }

            if (clip != null && !clip.IsSelected) {
                count++;
            }

            return count;
        }

        public void DeleteTrack(Track track) {
            track.Destroy();
            this.RemoveTrack(track);
        }

        public void DeleteTrackAt(int index) {
            this.tracks[index].Destroy();
            this.RemoveTrackAt(index);
        }

        public void TryExpandForFrame(long frame) {
            if (frame > this.maxDuration) {
                this.MaxDuration = frame + 1000;
            }
        }

        // Called by the track directly, in order to guarantee that selection is
        // handled before any track IsSelectedChanged event handlers
        internal static void InternalOnTrackSelectedChanged(Track track) {
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

        internal static void InternalSetMainTimelineProjectReference(Timeline timeline, Project project) {
            // no need to tell clips or tracks that our project changed, since there is guaranteed
            // to be none, unless this method is called outside of the project's constructor which it
            // shouldn't have been anyway
            timeline.Project = project;
            timeline.IsActive = true;
            RenderManager.InternalOnTimelineProjectChanged(timeline.RenderManager, null, project);
        }

        // TODO: composition timelines
        // This will have to traverse the entire timeline tree, and possible any other composition clips within
        // the timeline to update all of their projects to the given one
        internal static void InternalSetCompositionTimelineProjectReference(Timeline timeline, Project project) {
            Project oldProject = timeline.Project;
            if (ReferenceEquals(oldProject, project)) {
                throw new InvalidOperationException("Cannot set same project instance");
            }

            timeline.Project = project;
            foreach (Track track in timeline.tracks) {
                Track.InternalOnTimelineProjectChanged(track, oldProject, project);
            }

            RenderManager.InternalOnTimelineProjectChanged(timeline.RenderManager, oldProject, project);
        }

        internal static void InternalOnTrackSelectionCleared(Track track) {

        }

        public static void InternalOnActiveTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
            oldTimeline.IsActive = false;
            newTimeline.IsActive = true;
        }
    }
}