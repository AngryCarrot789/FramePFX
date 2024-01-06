using System;
using System.Collections;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.Timelines.Events;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines {

    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class Track : IProjectBound, IAutomatable {
        private readonly ClipList clips;

        /// <summary>
        /// The timeline that created this track
        /// </summary>
        public Timeline Timeline { get; private set; }

        public Project Project => this.Timeline?.Project;

        /// <summary>
        /// This track's clips (unordered)
        /// </summary>
        public IReadOnlyList<Clip> Clips => this.clips;

        /// <summary>
        /// This track's registry ID, used to create instances dynamically through the <see cref="TrackFactory"/>
        /// </summary>
        public string FactoryId => TrackFactory.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// A readable layer name
        /// </summary>
        public string DisplayName { get; set; }

        public double Height { get; set; }
        public SKColor TrackColour { get; set; }

        public long PreviousLargestFrameInUse => this.cache.PreviousLargestActiveFrame;

        public long LargestFrameInUse => this.cache.LargestActiveFrame;

        /// <summary>
        /// This track's automation data
        /// </summary>
        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        private readonly ClipRangeCache cache;
        private readonly ClipSpanChangedEventHandler clipSpanChangedHandler;

        /// <summary>
        /// An event fired when this track is moved from one timeline to another. So far, this is only called once
        /// during the project load phase. This track will exist in the new timeline and not the old timeline
        /// </summary>
        public event TrackMoveEventHandler TrackMoved;

        public event ProjectChangedEventHandler ProjectChanging;
        public event ProjectChangedEventHandler ProjectChanged;

        private int IndexInTimeline {
            get {
                int index = this.Timeline.IndexOfTrack(this);
                return index >= 0 ? index : throw new Exception("This track does not exist in its new timeline???");
            }
        }

        protected Track() {
            this.clips = new ClipList();
            this.cache = new ClipRangeCache();
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
            this.clipSpanChangedHandler = this.OnClipSpanChanged;
        }

        private void OnClipSpanChanged(Clip clip, FrameSpan oldSpan, FrameSpan newSpan) {
            if (!ReferenceEquals(clip.Track, this))
                throw new Exception("Clip's track does not match the current instance");
            this.cache.OnSpanChanged(clip, oldSpan);
        }

        public static void SetTimeline(Track track, Timeline timeline, int indexOfTrack) {
            Timeline oldTimeline = track.Timeline;
            if (!ReferenceEquals(oldTimeline, timeline)) {
                track.Timeline = timeline;
                foreach (Clip clip in track.clips)
                    Clip.InternalOnTrackTimelineChanged(clip, oldTimeline, timeline);
                track.OnTimelineChanged(oldTimeline, indexOfTrack);
            }
        }

        public static void OnTimelineProjectChanged(Track track, Project oldProject, Project newProject) {
            track.OnProjectChanging(oldProject, newProject);
            foreach (Clip clip in track.clips)
                Clip.InternalOnTrackTimelineProjectChanged(clip, oldProject, newProject);
            track.OnProjectChanged(oldProject, newProject);
        }

        /// <summary>
        /// Called when this track is moved from one timeline to another
        /// </summary>
        /// <param name="oldTimeline">The previous timeline. May be null, meaning this track was added to a timeline</param>
        protected virtual void OnTimelineChanged(Timeline oldTimeline, int oldIndexInTimeline) {
            this.TrackMoved?.Invoke(oldTimeline, oldIndexInTimeline, this.Timeline, this.IndexInTimeline);
        }

        protected virtual void OnProjectChanging(Project oldProject, Project newProject) {
            this.ProjectChanging?.Invoke(this, oldProject, newProject);
        }

        protected virtual void OnProjectChanged(Project oldProject, Project newProject) {
            this.ProjectChanged?.Invoke(this, oldProject, newProject);
        }

        public bool TryGetIndexOfClip(Clip clip, out int index) {
            return (index = this.IndexOfClip(clip)) != -1;
        }

        public int IndexOfClip(Clip clip) {
            return this.clips.IndexOf(clip);
        }

        public void GetClipsAtFrame(long frame, List<Clip> list) {
            Clip[] arr = this.clips.items;
            int count = this.clips.size, i = 0;
            while (i < count) {
                Clip clip = arr[i++];
                if (clip.IntersectsFrameAt(frame)) {
                    list.Add(clip);
                }
            }
        }

        public Clip GetClipAtFrame(long frame) {
            return this.cache.GetPrimaryClipAt(frame);
            // cannot use binary search until Clips is ordered
            // List<Clip> src = this.Clips;
            // int a = 0, b = src.Count - 1;
            // while (a <= b) {
            //     int mid = (a + b) / 2;
            //     Clip clip = src[mid];
            //     if (clip.IntersectsFrameAt(frame)) {
            //         return clip;
            //     }
            //     else if (frame < clip.FrameBegin) {
            //         b = mid - 1;
            //     }
            //     else {
            //         a = mid + 1;
            //     }
            // }
            // return null;
        }

        public void AddClip(Clip clip) => this.InsertClip(this.clips.Count, clip);

        public void InsertClip(int index, Clip clip) {
            if (clip.Track != null && clip.Track.TryGetIndexOfClip(clip, out _))
                throw new Exception("Clip already exists and is valid in another track: " + clip.Track);
            if (!this.IsClipTypeAcceptable(clip))
                throw new Exception("This track does not accept the clip");
            this.clips.Insert(index, clip);
            this.cache.OnClipAdded(clip);
            clip.ClipSpanChanged += this.clipSpanChangedHandler;
            Clip.InternalSetTrack(clip, this);
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception("Expected clip's track to equal this instance");
            this.clips.RemoveAt(index);
            this.cache.OnClipRemoved(clip);
            clip.ClipSpanChanged -= this.clipSpanChangedHandler;
            Clip.InternalSetTrack(clip, null);
        }

        /// <summary>
        /// Moves a clip from this track to the new track, from the old index to the new index
        /// </summary>
        /// <param name="newTrack">The target track</param>
        /// <param name="oldIndex">The index of the clip in this track</param>
        /// <param name="newIndex">The index of the clip in the target track</param>
        public void MoveClipToTrack(Track newTrack, int oldIndex, int newIndex) {
            Clip clip = this.clips[oldIndex];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception("Expected clip's track to equal this instance");
            if (!newTrack.IsClipTypeAcceptable(clip))
                throw new Exception("New track does not accept the clip being moved");
            this.clips.RemoveAt(oldIndex);
            this.cache.OnClipRemoved(clip);
            clip.ClipSpanChanged -= this.clipSpanChangedHandler;
            newTrack.clips.Insert(newIndex, clip);
            newTrack.cache.OnClipAdded(clip);
            clip.ClipSpanChanged += newTrack.clipSpanChangedHandler;
            Clip.InternalSetTrack(clip, newTrack);
        }

        #region Cloning

        public Track Clone(TrackCloneFlags flags = TrackCloneFlags.DefaultFlags) {
            Track clone = this.NewInstanceForClone();
            clone.DisplayName = this.DisplayName;
            clone.Height = this.Height;
            clone.TrackColour = this.TrackColour;
            this.LoadDataIntoClonePre(clone, flags);
            if ((flags & TrackCloneFlags.AutomationData) != 0)
                this.AutomationData.LoadDataIntoClone(clone.AutomationData);

            if ((flags & TrackCloneFlags.Clips) != 0) {
                foreach (Clip clip in this.clips)
                    clone.AddClip(clip.Clone());
            }

            this.LoadDataIntoClonePost(clone, flags);
            return clone;
        }

        protected abstract Track NewInstanceForClone();

        protected virtual void LoadDataIntoClonePre(Track clone, TrackCloneFlags flags) {
        }

        protected virtual void LoadDataIntoClonePost(Track clone, TrackCloneFlags flags) {
        }

        public bool IsRegionEmpty(FrameSpan span) => this.cache.IsRegionEmpty(span);

        #endregion

        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetDouble(nameof(this.Height), this.Height);
            data.SetUInt(nameof(this.TrackColour), (uint) this.TrackColour);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Clips));
            foreach (Clip clip in this.clips) {
                Clip.WriteSerialisedWithId(list.AddDictionary(), clip);
            }
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            this.Height = data.GetDouble(nameof(this.Height), 60);
            this.TrackColour = data.TryGetUInt(nameof(this.TrackColour), out uint colour) ? new SKColor(colour) : TrackColours.GetRandomColour();
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEDictionary dictionary in data.GetList(nameof(this.Clips)).Cast<RBEDictionary>()) {
                this.AddClip(Clip.ReadSerialisedWithId(dictionary));
            }
        }

        public abstract bool IsClipTypeAcceptable(Clip clip);

        /// <summary>
        /// Clears all clips in this track
        /// </summary>
        public void Clear() {
            using (ErrorList list = new ErrorList()) {
                for (int i = this.clips.Count - 1; i >= 0; i--) {
                    try {
                        this.RemoveClipAt(i);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
            }
        }

        public override string ToString() {
            return $"{this.GetType().Name} ({this.clips.Count.ToString()} clips between {this.cache.SmallestActiveFrame.ToString()} and {this.cache.LargestActiveFrame.ToString()})";
        }

        public static FrameSpan GetSpanUntilClipOrFuckIt(Track track, long frame, long defaultDuration = 300, long maximumDurationToClip = 100000000) {
            if (TryGetSpanUntilClip(track, frame, out var span, defaultDuration, maximumDurationToClip))
                return span;
            return new FrameSpan(frame, defaultDuration);
        }

        public static bool TryGetSpanUntilClip(Track track, long frame, out FrameSpan span, long unlimitedDuration = 300, long maxDuration = 100000000U) {
            long minimum = long.MaxValue;
            if (track.clips.Count > 0) {
                foreach (Clip clip in track.clips) {
                    if (clip.FrameBegin > frame) {
                        if (clip.IntersectsFrameAt(frame)) {
                            span = default;
                            return false;
                        }
                        else {
                            minimum = Math.Min(clip.FrameBegin, minimum);
                            if (minimum <= frame) {
                                break;
                            }
                        }
                    }
                }
            }

            if (minimum > frame && minimum != long.MaxValue) {
                span = new FrameSpan(frame, Math.Min(minimum - frame, maxDuration));
            }
            else {
                span = new FrameSpan(frame, unlimitedDuration);
            }

            return true;
        }
    }

    public class ClipList : IReadOnlyList<Clip> {
        private const int DefaultCapacity = 4;
        public Clip[] items;
        public int size;
        private static readonly Clip[] EmptyArray = new Clip[0];

        public int Count => this.size;

        public Clip this[int index] => index < this.size ? this.items[index] : throw new IndexOutOfRangeException("Index is too large");

        public Span<Clip> Span => new Span<Clip>(this.items, 0, this.size);

        public ClipList() => this.items = EmptyArray;

        public void Insert(int index, Clip item) {
            if (index > this.size)
                throw new Exception("Index out of bounds");
            if (this.size == this.items.Length)
                this.EnsureCapacity(this.size + 1);
            if (index < this.size)
                Array.Copy(this.items, index, this.items, index + 1, this.size - index);
            this.items[index] = item;
            ++this.size;
        }

        public int IndexOf(Clip item) {
            Clip[] array = this.items;
            for (int i = 0, count = this.size; i < count; i++) {
                if (item == array[i]) {
                    return i;
                }
            }

            return -1;
        }

        public bool Contains(Clip item) => this.IndexOf(item) != -1;

        public bool RemoveClipAndGetIsEmpty(Clip item) {
            int index = this.IndexOf(item);
            if (index == -1)
                throw new Exception("Expected item to exist in list");
            this.RemoveAt(index);
            return this.size == 0;
        }

        public void RemoveAt(int index) {
            if (index >= this.size)
                throw new Exception("Index out of bounds");
            --this.size;
            if (index < this.size)
                Array.Copy(this.items, index + 1, this.items, index, this.size - index);
            this.items[this.size] = null;
        }

        private void EnsureCapacity(int min) {
            int length = this.items.Length;
            if (length >= min)
                return;
            int num = length == 0 ? DefaultCapacity : length * 2;
            if (num > 0x7FEFFFFF)
                num = 0x7FEFFFFF;
            if (num < min)
                num = min;

            if (num < this.size)
                throw new Exception("List is too large to increase capacity");
            if (num == length)
                return;

            Clip[] objArray = new Clip[num];
            if (this.size > 0)
                Array.Copy(this.items, 0, objArray, 0, this.size);
            this.items = objArray;
        }

        public Span<Clip>.Enumerator GetEnumerator() => this.Span.GetEnumerator();

        IEnumerator<Clip> IEnumerable<Clip>.GetEnumerator() {
            for (int i = 0; i < this.size; i++) {
                yield return this.items[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<Clip>) this).GetEnumerator();
    }
}