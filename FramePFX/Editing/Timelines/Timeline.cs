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

using System.Collections.ObjectModel;
using FramePFX.Editing.Automation;
using FramePFX.Editing.Factories;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Utils.BTE;
using PFXToolKitUI.AdvancedMenuService;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Services;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Destroying;

namespace FramePFX.Editing.Timelines;

public delegate void TimelineTrackIndexEventHandler(Timeline timeline, Track track, int index);

public delegate void TimelineTrackMovedEventHandler(Timeline timeline, Track track, int oldIndex, int newIndex);

public delegate void TimelineEventHandler(Timeline timeline);

public delegate void PlayHeadChangedEventHandler(Timeline timeline, long oldValue, long newValue);

public delegate void TimelineScrubEventHandler(Timeline timeline, PlayHeadType type);

public class Timeline : ITransferableData, IServiceable, IDestroy {
    public static readonly ContextRegistry ContextRegistry = new ContextRegistry("Timeline");

    private FrameSpan? loopRegion;
    private bool isLoopRegionEnabled;

    public Project? Project { get; private set; }

    public TrackPoint RangedSelectionAnchor { get; set; } = TrackPoint.Invalid;

    public ReadOnlyCollection<Track> Tracks { get; }

    public RenderManager RenderManager { get; }

    public TransferableData TransferableData { get; }

    /// <summary>
    /// Returns true when this timeline is currently visible in the editor
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Gets or sets the total length of all tracks, in frames. This is incremented on demand when necessary, and is used for UI calculations
    /// </summary>
    public long MaxDuration {
        get => this.maxDuration;
        set {
            if (this.maxDuration == value)
                return;

            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), value, "Maximum duration cannot be negative");

            if (this.loopRegion.HasValue) {
                FrameSpan span = this.loopRegion.Value;
                if (span.EndIndex > value) {
                    // Recalculate a smaller loop region. Clear it if the new one doesn't fit anymore
                    span = span.WithEndIndexClamped(value);
                    this.LoopRegion = span.Duration < 1 ? null : span;
                }
            }

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
                throw new ArgumentOutOfRangeException(nameof(value), value, "StopHead cannot be negative");
            if (value >= this.maxDuration)
                throw new ArgumentOutOfRangeException(nameof(value), value, "StopHead exceeds the timeline duration range (0 to TotalFrames)");

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
                throw new ArgumentOutOfRangeException(nameof(value), value, "PlayHead cannot be negative");
            if (value >= this.maxDuration)
                throw new ArgumentOutOfRangeException(nameof(value), value, "PlayHead exceeds the timeline duration range (0 to TotalFrames)");

            long oldPlayHead = this.playHeadPosition;
            this.playHeadPosition = value;
            this.PlayHeadChanged?.Invoke(this, oldPlayHead, value);
            this.UpdateAutomation(value, true);
        }
    }

    /// <summary>
    /// Gets the effective duration of this timeline. That is, the <see cref="FrameSpan.EndIndex"/> of the clip that is
    /// furthest towards the right side of the timeline. This value is typically always less than <see cref="MaxDuration"/>
    /// </summary>
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
    /// Gets or sets the looping region. This is not effective until <see cref="IsLoopRegionEnabled"/> is true.
    /// Setting this to null intrinsically makes <see cref="IsLoopRegionEnabled"/> false
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Value's begin is negative or endIndex exceeds our <see cref="MaxDuration"/>
    /// </exception>
    public FrameSpan? LoopRegion {
        get => this.loopRegion;
        set {
            if (this.loopRegion == value)
                return;

            if (value.HasValue) {
                FrameSpan span = value.Value;
                if (span.Begin < 0 || span.EndIndex > this.maxDuration) {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Loop region exceeds timeline maximum duration");
                }
            }

            this.loopRegion = value;
            this.LoopRegionChanged?.Invoke(this);
        }
    }

    public bool IsLoopRegionEnabled {
        get => this.isLoopRegionEnabled;
        set {
            if (this.isLoopRegionEnabled == value)
                return;

            this.isLoopRegionEnabled = value;
            this.IsLoopRegionEnabledChanged?.Invoke(this);
        }
    }

    public ServiceManager ServiceManager { get; }

    /// <summary>
    /// Gets the play head being scrubbed currently. This is used to delay expensive
    /// operations (when a play head value changes) until the scrubbing ends
    /// </summary>
    public PlayHeadType ScrubbingPlayHead { get; private set; }

    public event TimelineTrackIndexEventHandler? TrackAdded;
    public event TimelineTrackIndexEventHandler? TrackRemoved;
    public event TimelineTrackMovedEventHandler? TrackMoved;
    public event TimelineEventHandler? MaxDurationChanged;
    public event TimelineEventHandler? LargestFrameInUseChanged;
    public event PlayHeadChangedEventHandler? PlayHeadChanged;
    public event PlayHeadChangedEventHandler? StopHeadChanged;
    public event TimelineEventHandler? IsLoopRegionEnabledChanged;
    public event TimelineEventHandler? LoopRegionChanged;
    public event TimelineScrubEventHandler? BeginScrub;
    public event TimelineScrubEventHandler? EndScrub;

    private readonly List<Track> tracks;
    private long maxDuration;
    private long playHeadPosition;
    private long stopHeadPosition;
    private long largestFrameInUse;

    public Timeline() {
        this.TransferableData = new TransferableData(this);
        this.tracks = new List<Track>();
        this.Tracks = new ReadOnlyCollection<Track>(this.tracks);
        this.maxDuration = 5000L;
        this.ServiceManager = new ServiceManager();
        this.RenderManager = new RenderManager(this);
    }

    static Timeline() {
        FixedContextGroup modGeneric = ContextRegistry.GetFixedGroup("modify.general");
        modGeneric.AddHeader("General");
        modGeneric.AddCommand("commands.editor.SelectAllClips", "Select All Clips", "Select all clips in the timeline");
        modGeneric.AddCommand("commands.editor.SplitClipsCommand", "Split Clip(s)", "Slice clips at the play head");

        FixedContextGroup modGenerate = ContextRegistry.GetFixedGroup("modify.generate");
        modGenerate.AddHeader("New Tracks");
        modGenerate.AddCommand("commands.editor.CreateVideoTrack", "New Video Track", "Creates a new video track");
    }

    public void UpdateAutomation(long playHead, bool invalidateRender = true) {
        using (this.RenderManager.SuspendRenderInvalidation()) {
            AutomationEngine.UpdateValues(this, playHead);
        }

        if (invalidateRender)
            this.InvalidateRender();
    }

    public virtual void WriteToBTE(BTEDictionary data) {
        data.SetLong(nameof(this.PlayHeadPosition), this.PlayHeadPosition);
        data.SetLong(nameof(this.StopHeadPosition), this.StopHeadPosition);
        data.SetLong(nameof(this.MaxDuration), this.MaxDuration);
        BTEList list = data.CreateList(nameof(this.Tracks));
        foreach (Track track in this.tracks) {
            if (!(track.FactoryId is string registryId))
                throw new Exception("Unknown track type: " + track.GetType());
            BTEDictionary trackTag = list.AddDictionary();
            trackTag.SetString(nameof(Track.FactoryId), registryId);
            Track.SerialisationRegistry.Serialise(track, trackTag.CreateDictionary("Data"));
        }
    }

    public virtual void ReadFromBTE(BTEDictionary data) {
        if (this.tracks.Count > 0) {
            throw new InvalidOperationException("Cannot read track BTE data while there are still tracks");
        }

        this.playHeadPosition = data.GetLong(nameof(this.PlayHeadPosition));
        this.stopHeadPosition = data.GetLong(nameof(this.StopHeadPosition));
        this.maxDuration = data.GetLong(nameof(this.MaxDuration));
        foreach (BTEDictionary trackTag in data.GetList(nameof(this.Tracks)).Cast<BTEDictionary>()) {
            string registryId = trackTag.GetString(nameof(Track.FactoryId));
            Track track = TrackFactory.Instance.NewTrack(registryId);
            Track.SerialisationRegistry.Deserialise(track, trackTag.GetDictionary("Data"));
            this.AddTrack(track);
        }

        // Recalculate a new max duration, just in case the clips somehow exceed the current value
        this.maxDuration = Math.Max(this.maxDuration, this.tracks.Count < 1 ? 0 : this.tracks.Max(x => x.Clips.Count < 1 ? 0 : x.Clips.Max(y => y.FrameSpan.EndIndex)));
        this.UpdateLargestFrame();
    }

    public void LoadDataIntoClone(Timeline clone) {
        if (this.tracks.Count > 0) {
            throw new InvalidOperationException("Cannot read track BTE data while there are still tracks");
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

    public void InvalidateRender() => this.RenderManager.InvalidateRender();

    public void DeleteTrack(Track track) {
        track.Destroy();
        this.RemoveTrack(track);
    }

    public void DeleteTrackAt(int index) {
        this.tracks[index].Destroy();
        this.RemoveTrackAt(index);
    }

    public void TryExpandForFrame(long frame) {
        if (frame >= this.maxDuration) {
            this.MaxDuration = frame + 1000;
        }
    }

    public void BeginScrubPlayHead(PlayHeadType type) {
        if (this.ScrubbingPlayHead != PlayHeadType.None)
            throw new InvalidOperationException("Already scrubbing");

        this.ScrubbingPlayHead = type;
        this.BeginScrub?.Invoke(this, type);
        AppLogger.Instance.WriteLine("Begin Scrub Play Head: " + type);
    }

    public void EndScrubPlayHead(PlayHeadType type) {
        if (this.ScrubbingPlayHead != type) {
            throw new InvalidOperationException("Attempt to end scrubbing on a different playhead type");
        }

        this.EndScrub?.Invoke(this, type);
        this.ScrubbingPlayHead = PlayHeadType.None;
        AppLogger.Instance.WriteLine("End Scrub Play Head: " + type);
    }

    internal static void InternalSetMainTimelineProjectReference(Timeline timeline, Project project) {
        // no need to tell clips or tracks that our project changed, since there is guaranteed
        // to be none, unless this method is called outside of the project's constructor which it
        // shouldn't have been anyway
        timeline.Project = project;
        timeline.IsActive = true;
        RenderManager.InternalOnTimelineProjectChanged(timeline.RenderManager, null, project);
    }

    internal static void InternalSetCompositionTimelineProjectReference(Timeline timeline, Project? project) {
        Project? oldProject = timeline.Project;
        if (ReferenceEquals(oldProject, project)) {
            throw new InvalidOperationException("Cannot set same project instance");
        }

        timeline.Project = project;
        foreach (Track track in timeline.tracks) {
            Track.InternalOnTimelineProjectChanged(track, oldProject, project);
        }

        if (project != null) {
            InternalLoadResources(timeline, project.ResourceManager);
        }
        else {
            InternalUnloadResources(timeline);
        }

        RenderManager.InternalOnTimelineProjectChanged(timeline.RenderManager, oldProject, project);
    }

    public static void InternalOnActiveTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
        oldTimeline.IsActive = false;
        newTimeline.IsActive = true;
    }

    public int IndexOf(Track track) {
        return track.Timeline == this ? track.IndexInTimeline : -1;
    }

    public static void InternalLoadResources(Timeline timeline, ResourceManager manager) {
        foreach (Track track in timeline.tracks) {
            foreach (Clip clip in track.Clips) {
                clip.ResourceHelper.OnResourceManagerLoaded(manager);
            }
        }
    }

    public static void InternalUnloadResources(Timeline timeline) {
        foreach (Track track in timeline.tracks) {
            foreach (Clip clip in track.Clips) {
                clip.ResourceHelper.OnResourceManagerUnloaded();
            }
        }
    }
}