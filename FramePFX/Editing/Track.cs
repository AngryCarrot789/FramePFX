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

using System.Runtime.CompilerServices;
using PFXToolKitUI.Composition;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.Utils.Events;
using SkiaSharp;

namespace FramePFX.Editing;

/// <summary>
/// The base class for a track
/// </summary>
public abstract class Track : IComponentManager, ITransferableData {
    private readonly ClipList clips;
    private readonly HashSet<Clip> clipSet;

    public ComponentStorage ComponentStorage => field ??= new ComponentStorage(this);

    /// <summary>
    /// Gets the timeline that this track resides in
    /// </summary>
    public Timeline? Timeline {
        get => field;
        internal set => PropertyHelper.SetAndRaiseINE(ref field, value, this, static (t, o, n) => t.OnTimelineChanged(o, n));
    }

    /// <summary>
    /// Gets or sets the display name of this track
    /// </summary>
    public string? DisplayName {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.DisplayNameChanged);
    }

    /// <summary>
    /// Gets or sets the track colour
    /// </summary>
    public SKColor Colour {
        get => field;
        set => PropertyHelper.SetAndRaiseINE(ref field, value, this, this.ColourChanged);
    } = TrackUtils.RandomColour();

    /// <summary>
    /// Gets the project associated with this track
    /// </summary>
    public Project? Project => this.Timeline?.Project;

    /// <summary>
    /// Gets an enumerable of clips
    /// </summary>
    public IEnumerable<Clip> Clips => this.clipSet;

    /// <summary>
    /// Gets the type of clip allowed in this track
    /// </summary>
    public ClipType AcceptedClipType => this.InternalAcceptedClipType;
    
    internal abstract ClipType InternalAcceptedClipType { get; }
    
    public TransferableData TransferableData => field ??= new TransferableData(this);

    public event EventHandler<ValueChangedEventArgs<Timeline?>>? TimelineChanged;
    public event EventHandler? DisplayNameChanged;
    public event EventHandler? ColourChanged;

    public event EventHandler<ClipEventArgs>? ClipAdded;
    public event EventHandler<ClipEventArgs>? ClipRemoved;

    // private readonly ClipRangeCache cache;

    protected Track() {
        this.clipSet = new HashSet<Clip>();
        this.clips = new ClipList();
    }
    
    protected virtual void OnTimelineChanged(Timeline? oldTimeline, Timeline? newTimeline) {
        this.TimelineChanged?.Invoke(this, new ValueChangedEventArgs<Timeline?>(oldTimeline, newTimeline));
    }

    public void AddClip(Clip clip) {
        ArgumentNullException.ThrowIfNull(clip);
        CheckClipType(this.AcceptedClipType, clip.ClipType);

        if (clip.Track != null)
            throw new InvalidOperationException("Clip already added to a track");
        if (!this.clipSet.Add(clip))
            throw new InvalidOperationException("Clip already added to this track...???");

        this.clips.Add(clip);
        clip.Track = this;

        this.ClipAdded?.Invoke(this, new ClipEventArgs(clip));
    }

    public void RemoveClip(Clip clip) {
        ArgumentNullException.ThrowIfNull(clip);
        CheckClipType(this.AcceptedClipType, clip.ClipType);

        if (clip.Track == null)
            throw new InvalidOperationException("Clip does not exist in a track");
        if (!this.clipSet.Remove(clip))
            throw new InvalidOperationException("Clip not added to this track...???");

        this.clips.Remove(clip.Span, clip);
        clip.Track = null;

        this.ClipRemoved?.Invoke(this, new ClipEventArgs(clip));
    }

    public bool ContainsClip(Clip clip) {
        CheckClipType(this.AcceptedClipType, clip.ClipType);
        return this.clips.Contains(clip);
    }

    public Clip? GetPrimaryClipAt(TimeSpan frame) {
        return this.clips.GetPrimaryClipAt(frame);
    }

    public void GetClipsInRange(List<Clip> dstList, ClipSpan span) {
        this.clips.GetClipsInRange(dstList, span);
    }

    public int ExtractClipsAt(List<Clip> dstList, long frame) {
        return this.clips.ExtractClipsAt(dstList, frame);
    }

    public IEnumerable<Clip> GetClipsAtFrame(long frame) {
        return this.clips.GetClipsAtFrame(frame);
    }

    public bool IsRegionEmpty(ClipSpan span) {
        return this.clips.IsRegionEmpty(span);
    }

    internal void InternalOnClipSpanChanged(Clip clip, ClipSpan oldSpan, ClipSpan newSpan) {
        this.clips.OnSpanChanged(clip, oldSpan);
    }

    private static void CheckClipType(ClipType trackType, ClipType clipType) {
        if (trackType != clipType) {
            throw new InvalidOperationException("Invalid clip type. Expected " + trackType + ", got " + clipType);
        }
    }
}

public readonly struct ClipEventArgs(Clip clip) {
    /// <summary>
    /// Gets the clip associated with the event
    /// </summary>
    public Clip Clip { get; } = clip;
}

/// <summary>
/// A class that stores clips in chunks of 67108864 ticks to efficiently locate clips at a particular frame,
/// rather than having to scan the entire track's clip list
/// </summary>
public class ClipList {
    private readonly SortedList<long, Section> Map;

    /// <summary>
    /// The smallest frame that any clip takes up based on their span's begin property.
    /// This is basically calculated as:
    /// <code>
    /// foreach (clip in track) value = min(value, clip.Span.FrameBegin)
    /// </code>
    /// </summary>
    public TimeSpan SmallestActiveTime { get; private set; }

    /// <summary>
    /// The largest frame that any clip takes up based on their span's end index property.
    /// This is basically calculated as:
    /// <code>
    /// foreach (clip in track) value = max(value, clip.Span.FrameEndIndex)
    /// </code>
    /// </summary>
    public TimeSpan LargestActiveTime { get; private set; }

    /// <summary>
    /// The previous value of <see cref="SmallestActiveTime"/> before it changed
    /// </summary>
    public TimeSpan PreviousSmallestActiveTime { get; private set; }

    /// <summary>
    /// The previous version of <see cref="LargestActiveTime"/> before it changed
    /// </summary>
    public TimeSpan PreviousLargestActiveTime { get; private set; }

    /// <summary>
    /// Called when a clip is added, removed or its span changed
    /// </summary>
    public event EventHandler? FrameDataChanged;

    public ClipList() {
        this.Map = new SortedList<long, Section>();
    }

    public Clip? GetPrimaryClipAt(TimeSpan location) {
        if (!this.Map.TryGetValue(GetIndex(location.Ticks), out Section? list)) {
            return null;
        }

        for (int i = list.size - 1; i >= 0; i--) {
            Clip clip = list.items[i];
            if (clip.IsPointInRange(location.Ticks)) {
                return clip;
            }
        }

        return null;
    }

    public void GetClipsInRange(List<Clip> dstList, ClipSpan span) {
        long idxA = GetIndex(span.Start.Ticks);
        long idxB = GetIndex(span.End.Ticks);
        for (long idx = idxA; idx <= idxB; idx++) {
            if (!this.Map.TryGetValue(idx, out Section? list)) {
                continue;
            }

            for (int i = list.size - 1; i >= 0; i--) {
                Clip clip = list.items[i];
                if (clip.Span.IntersectedBy(span))
                    dstList.Add(clip);
            }
        }
    }

    public int ExtractClipsAt(List<Clip> dstList, long frame) {
        int c = dstList.Count;
        if (this.Map.TryGetValue(GetIndex(frame), out Section? list)) {
            for (int i = list.size - 1; i >= 0; i--) {
                Clip clip = list.items[i];
                if (clip.Span.IntersectedBy(frame)) {
                    dstList.Add(clip);
                }
            }
        }

        return dstList.Count - c;
    }

    public IEnumerable<Clip> GetClipsAtFrame(long frame) {
        if (this.Map.TryGetValue(GetIndex(frame), out Section? list)) {
            List<Clip> clips = new List<Clip>();
            for (int i = list.size - 1; i >= 0; i--) {
                Clip clip = list.items[i];
                if (clip.Span.IntersectedBy(frame))
                    clips.Add(clip);
            }

            return clips;
        }

        return Enumerable.Empty<Clip>();
    }

    public void Add(Clip clip) {
        ClipSpan span = clip.Span;
        GetRange(span, out long a, out long b);
        this.AddClipInRange(clip, a, b);
        this.PreviousSmallestActiveTime = this.SmallestActiveTime;
        this.SmallestActiveTime = new TimeSpan(Math.Min(this.SmallestActiveTime.Ticks, span.Start.Ticks));
        this.PreviousLargestActiveTime = this.LargestActiveTime;
        this.LargestActiveTime = new TimeSpan(Math.Max(this.LargestActiveTime.Ticks, span.End.Ticks));
        this.FrameDataChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Remove(ClipSpan location, Clip clip) {
        GetRange(location, out long a, out long b);
        this.RemoveClipInRange(clip, a, b);
        this.ProcessSmallestAndLargestFrame();
    }

    #region Processor functions

    private void AddClipInRange(Clip clip, long min, long max) {
        for (long frame = min; frame <= max; frame++) {
            if (!this.Map.TryGetValue(frame, out Section? list))
                this.Map[frame] = list = new Section();
            else if (list.Contains(clip))
                throw new Exception("Did not expect clip to already exist in list");

            list.Add(clip);
        }
    }

    private void RemoveClipInRange(Clip clip, long min, long max) {
        for (long i = min; i <= max; i++) {
            int index = this.Map.IndexOfKey(i);
            if (index != -1) {
                Section list = this.Map.Values[index];
                if (list.RemoveClipAndGetIsEmpty(clip)) {
                    this.Map.RemoveAt(index);
                }
            }
            else {
                throw new Exception("Expected ClipList to exist at index: " + i);
            }
        }
    }

    public void OnSpanChanged(Clip clip, ClipSpan oldSpan) {
        ClipSpan newSpan = clip.Span;
        if (oldSpan == newSpan) {
            return;
        }

        GetRange(oldSpan, out long oldA, out long oldB);
        GetRange(newSpan, out long newA, out long newB);
        if (oldA == newA && oldB == newB) {
            // ClipList list = this.Map[oldA];
            // list.OnClipSpanChanged(clip, oldSpan);
        }

        for (long frame = oldA; frame <= oldB; frame++) {
            if (this.Map[frame].RemoveClipAndGetIsEmpty(clip)) {
                this.Map.Remove(frame);
            }
        }

        // Add the clip to the new grouped range
        for (long frame = newA; frame <= newB; frame++) {
            if (!this.Map.TryGetValue(frame, out Section? list)) {
                this.Map[frame] = list = new Section();
            }

            list.Add(clip);
        }

        this.ProcessSmallestAndLargestFrame();
    }

    #endregion

    private void ProcessSmallestAndLargestFrame() {
        long min = 0, max = 0;
        int index = this.Map.Count - 1;
        if (index >= 0) {
            Section list = this.Map.Values[index];
            for (int i = 0; i < list.size; i++) {
                max = Math.Max(list.items[i].Span.End.Ticks, max);
            }

            min = max;
            list = this.Map.Values[0];
            for (int i = 0; i < list.size; i++) {
                min = Math.Min(list.items[i].Span.Start.Ticks, min);
                if (min < 1) {
                    break;
                }
            }
        }

        this.PreviousSmallestActiveTime = this.SmallestActiveTime;
        this.SmallestActiveTime = new TimeSpan(min);
        this.PreviousLargestActiveTime = this.LargestActiveTime;
        this.LargestActiveTime = new TimeSpan(max);
        this.FrameDataChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Util functions

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void GetRange(ClipSpan span, out long a, out long b) {
        a = GetIndex(span.Start.Ticks);
        b = GetIndex(span.End.Ticks);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static long GetIndex(long location) => location >> 26;

    #endregion

    public bool IsRegionEmpty(ClipSpan span) {
        GetRange(span, out long a, out long b);
        for (long i = a; i <= b; i++) {
            if (this.Map.TryGetValue(i, out Section? list) && IntersectsAny(list, span))
                return false;
        }

        return true;
    }

    private static bool IntersectsAny(Section list, ClipSpan span) {
        for (int j = list.size - 1; j >= 0; j--) {
            if (list.items[j].Span.IntersectedBy(span))
                return true;
        }

        return false;
    }

    public bool Contains(Clip clip) {
        if (!this.Map.TryGetValue(GetIndex(clip.Span.Start.Ticks), out Section? list)) {
            return false;
        }

        for (int i = list.size - 1; i >= 0; i--) {
            if (list.items[i] == clip) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// A compact optimized list implementation for clips only
    /// </summary>
    private class Section {
        private const int DefaultCapacity = 4;
        private const int CapacityLimit = 0x7FEFFFFF;
        public Clip[] items;
        public int size;
        private static readonly Clip[] EmptyArray = new Clip[0];

        public Section() => this.items = EmptyArray;

        public void Add(Clip item) {
            if (this.size == this.items.Length)
                this.EnsureCapacity(this.size + 1);
            this.items[this.size++] = item;
        }

        public int IndexOf(Clip item) {
            Clip[] array = this.items;
            for (int i = this.size - 1; i >= 0; i--) {
                Clip clip = array[i];
                if (item == clip)
                    return i;
            }

            return -1;
        }

        public bool Contains(Clip item) => this.IndexOf(item) != -1;

        private void EnsureCapacity(int min) {
            int length = this.items.Length;
            if (length >= min)
                return;

            int newCount = length == 0 ? DefaultCapacity : length * 2;
            if (newCount > CapacityLimit)
                newCount = CapacityLimit;
            if (newCount < min)
                newCount = min;

            if (newCount < this.size)
                throw new Exception("List is too large to increase capacity");

            if (newCount == length)
                return;

            Clip[] newItems = new Clip[newCount];
            if (this.size > 0)
                Array.Copy(this.items, 0, newItems, 0, this.size);

            this.items = newItems;
        }

        public bool RemoveClipAndGetIsEmpty(Clip item) {
            int index = this.IndexOf(item);
            if (index == -1)
                throw new Exception("Expected item to exist in list");

            --this.size;
            if (index < this.size)
                Array.Copy(this.items, index + 1, this.items, index, this.size - index);

            this.items[this.size] = null; // prevent memory leak

            return this.size == 0;
        }
    }
}