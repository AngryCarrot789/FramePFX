// 
// Copyright (c) 2026-2026 REghZy
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
using System.Diagnostics;
using FramePFX.Editing.Video;
using PFXToolKitUI.Composition;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.Events;

namespace FramePFX.Editing;

/// <summary>
/// Represents a video editor timeline containing tracks and clips
/// </summary>
public sealed class Timeline : IComponentManager {
    private readonly List<Track> tracks;

    public ComponentStorage ComponentStorage => field ??= new ComponentStorage(this);

    /// <summary>
    /// Gets the list of tracks in this timeline
    /// </summary>
    public ReadOnlyCollection<Track> Tracks { get; }

    /// <summary>
    /// Gets or sets the location of the play head, in ticks. There are 10 million ticks in one second. 
    /// </summary>
    public TimeSpan PlayHeadLocation {
        get => field;
        set {
            value = new TimeSpan(Math.Max(0, value.Ticks));
            this.MaximumDuration = new TimeSpan(Math.Max(this.MaximumDuration.Ticks, value.Ticks));
            PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => {
                t.PlayHeadLocationChanged?.Invoke(t, new ValueChangedEventArgs<TimeSpan>(o, n));
                t.RaiseRenderInvalidated(null, new ClipSpan(o, n));
            });
        }
    }

    /// <summary>
    /// Gets or sets the maximum duration of the timeline. 
    /// </summary>
    public TimeSpan MaximumDuration {
        get => field;
        set {
            value = new TimeSpan(Math.Max(0, value.Ticks));
            PropertyHelper.SetAndRaiseINE(ref field, value, this, this.MaximumDurationChanged);
        }
    } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the project associated with this timeline
    /// </summary>
    public Project? Project {
        get => field;
        internal set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnProjectChanged(o, n));
    }

    public RenderManager RenderManager => field ??= new RenderManager(this);

    public event EventHandler<TrackAddedOrRemovedEventArgs>? TrackAdded, TrackRemoved;
    public event EventHandler<TrackMovedEventArgs>? TrackMoved;

    public event EventHandler<ValueChangedEventArgs<TimeSpan>>? PlayHeadLocationChanged;
    public event EventHandler<ValueChangedEventArgs<TimeSpan>>? MaximumDurationChanged;
    public event EventHandler<ValueChangedEventArgs<Project?>>? ProjectChanged;

    /// <summary>
    /// An event fired when the render state of the timeline becomes invalid through other means than <see cref="PlayHeadLocation"/>
    /// changing. For example, opacity of a clip being changed
    /// </summary>
    public event EventHandler<TimelineRenderInvalidatedEventArgs>? RenderInvalidated;

    public Timeline() {
        this.tracks = new List<Track>();
        this.Tracks = this.tracks.AsReadOnly();
    }

    public void AddTrack(Track track) => this.InsertTrack(this.tracks.Count, track);

    public void InsertTrack(int index, Track track) {
        ArgumentNullException.ThrowIfNull(track);
        if (track.Timeline != null)
            throw new InvalidOperationException("Track already added to a timeline");
        if (this.tracks.Contains(track))
            throw new InvalidOperationException("Track already added to this timeline...???");

        this.tracks.Insert(index, track);
        track.Timeline = this;
        this.TrackAdded?.Invoke(this, new TrackAddedOrRemovedEventArgs(track, index));
    }

    public void RemoveTrackAt(int index) {
        Track track = this.tracks[index];
        if (track.Timeline == null)
            throw new InvalidOperationException("Track does not exist in a timeline");

        this.tracks.RemoveAt(index);
        track.Timeline = null;
        this.TrackRemoved?.Invoke(this, new TrackAddedOrRemovedEventArgs(track, index));
    }

    public bool RemoveTrack(Track track) {
        int index = this.tracks.IndexOf(track);
        if (index == -1)
            return false;

        this.RemoveTrackAt(index);
        return true;
    }

    public void MoveTrack(int oldIndex, int newIndex) {
        if (ArrayUtils.IsOutOfBounds(this.tracks.Count, oldIndex))
            throw new ArgumentOutOfRangeException(nameof(newIndex), oldIndex, "Old index out of bounds");
        if (ArrayUtils.IsOutOfBounds(this.tracks.Count, newIndex))
            throw new ArgumentOutOfRangeException(nameof(newIndex), newIndex, "New index out of bounds");

        Track track = this.tracks[oldIndex];
        this.tracks.RemoveAt(oldIndex);
        this.tracks.Insert(newIndex, track);
        this.TrackMoved?.Invoke(this, new TrackMovedEventArgs(track, oldIndex, newIndex));
    }

    public bool ContainsTrack(Track track) {
        bool b = track.Timeline == this;
        Debug.Assert(!b || this.tracks.Contains(track));

        return b;
    }

    internal void InternalOnClipSpanChanged(VideoTrack track, Clip clip, ClipSpan oldSpan, ClipSpan newSpan) {
        this.RaiseRenderInvalidated(track, ClipSpan.Union(oldSpan, newSpan));
    }

    /// <summary>
    /// Raises the <see cref="RenderInvalidated"/> event with no source track and <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public void RaiseRenderInvalidated() => this.RaiseRenderInvalidated(null, ClipSpan.MaxValue);

    public void RaiseRenderInvalidated(VideoTrack? source, ClipSpan span) {
        this.RenderInvalidated?.Invoke(this, new TimelineRenderInvalidatedEventArgs(source, span));
        this.RenderManager.InvalidateRender();
    }

    private void OnProjectChanged(Project? oldProject, Project? newProject) {
        this.ProjectChanged?.Invoke(this, new ValueChangedEventArgs<Project?>(oldProject, newProject));
        RenderManager.InternalOnTimelineProjectChanged(this.RenderManager, oldProject, newProject);
    }
}

public readonly struct TrackAddedOrRemovedEventArgs(Track track, int index) {
    public Track Track { get; } = track;
    public int Index { get; } = index;
}

public readonly struct TrackMovedEventArgs(Track track, int oldIndex, int newIndex) {
    public Track Track { get; } = track;
    public int OldIndex { get; } = oldIndex;
    public int NewIndex { get; } = newIndex;
}

/// <summary>
/// Contains information about the cause of timeline render invalidation
/// </summary>
public readonly struct TimelineRenderInvalidatedEventArgs(VideoTrack? sauce, ClipSpan span) {
    /// <summary>
    /// Gets the track that caused the render to become invalidated. Null if not caused by track invalidation
    /// </summary>
    public VideoTrack? Source { get; } = sauce;

    /// <summary>
    /// Gets the invalidated range. May be <see cref="ClipSpan.MaxValue"/>
    /// </summary>
    public ClipSpan Span { get; } = span;
}