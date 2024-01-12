using System;
using System.Collections;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// The event args for when one or more clips are added or removed to or from a track
    /// </summary>
    public class ClipsAddedOrRemovedEventArgs {
        public Clip[] Clips { get; }

        public Track OldTrack { get; }

        public Track NewTrack { get; }

        public bool IsAdding => this.OldTrack == null;
        public bool IsRemoving => this.OldTrack != null;

        public ClipsAddedOrRemovedEventArgs(Clip[] clips, Track oldTrack, Track newTrack) {
            if (oldTrack == null && newTrack == null)
                throw new ArgumentException("Cannot use null old and new tracks");

            this.Clips = clips;
            this.OldTrack = oldTrack;
            this.NewTrack = newTrack;
        }
    }

    public delegate void ClipsAddedOrRemovedEventHandler(Track track, ClipsAddedOrRemovedEventArgs e);

    public delegate void TrackEventHandler(Track track);

    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class Track : IProjectBound, IAutomatable {
        private readonly ClipList clips;

        /// <summary>
        /// The timeline that this track is placed in. This does not change once set.
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
        private readonly FrameSpanChangedEventHandler frameSpanChangedHandler;

        public event ClipsAddedOrRemovedEventHandler ClipsAddedOrRemoved;

        private int IndexInTimeline {
            get {
                int index = this.Timeline.IndexOfTrack(this);
                return index >= 0 ? index : throw new Exception("This track does not exist in its new timeline???");
            }
        }

        /// <summary>
        /// An event fired when our largest frame changes
        /// </summary>
        public event TrackEventHandler LargestFrameChanged;

        protected Track() {
            this.clips = new ClipList();
            this.cache = new ClipRangeCache();
            this.cache.PropertyChanged += this.OnCachePropertyChanged;
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
            this.frameSpanChangedHandler = this.OnClipSpanChanged;
        }

        private void OnCachePropertyChanged(IObservableObject sender, string name) {
            switch (name) {
                case nameof(ClipRangeCache.LargestActiveFrame):
                    this.LargestFrameChanged?.Invoke(this);
                    break;
            }
        }

        private void OnClipSpanChanged(Clip clip, FrameSpan oldSpan, FrameSpan newSpan) {
            if (!ReferenceEquals(clip.Track, this))
                throw new Exception("Clip's track does not match the current instance");
            this.cache.OnSpanChanged(clip, oldSpan);
        }

        public static void SetTimeline(Track track, Timeline timeline) {
            Timeline oldTimeline = track.Timeline;
            if (!ReferenceEquals(oldTimeline, timeline)) {
                track.Timeline = timeline;
                ResourceManager manager = timeline?.Project?.ResourceManager;
                foreach (Clip clip in track.clips)
                    clip.ResourceHelper.SetManager(manager);
                track.OnTimelineChanged(oldTimeline, timeline);
            }
        }

        public static void OnTimelineProjectChanged(Track track, ProjectChangedEventArgs e) {
            track.OnProjectChanged(e ?? throw new ArgumentNullException(nameof(e)));
        }

        protected virtual void OnProjectChanged(ProjectChangedEventArgs e) {
            foreach (Clip clip in this.clips) {
                Clip.OnProjectChangedInternal(clip, e);
            }
        }

        /// <summary>
        /// Called when this track is moved from one timeline to another
        /// </summary>
        /// <param name="oldTimeline">The previous timeline. May be null, meaning this track was added to a timeline</param>
        protected virtual void OnTimelineChanged(Timeline oldTimeline, Timeline newTimeline) {
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
            clip.FrameSpanChanged += this.frameSpanChangedHandler;
            Clip.InternalSetTrack(clip, this);
            this.ClipsAddedOrRemoved?.Invoke(this, new ClipsAddedOrRemovedEventArgs(new Clip[] {clip}, null, this));
        }

        /// <summary>
        /// Removes this clip from the track. This method does not destroy the clip, that must be done before-hand
        /// </summary>
        /// <param name="index">The index to remove the clip at</param>
        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                AppLogger.WriteLine("WARNING: The clip being removed references a track that is not equal to the track that contains the clip");
            this.clips.RemoveAt(index);
            this.cache.OnClipRemoved(clip);
            clip.FrameSpanChanged -= this.frameSpanChangedHandler;
            Clip.InternalSetTrack(clip, null);
            this.ClipsAddedOrRemoved?.Invoke(this, new ClipsAddedOrRemovedEventArgs(new Clip[] {clip}, this, null));
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
            clip.FrameSpanChanged -= this.frameSpanChangedHandler;
            newTrack.clips.Insert(newIndex, clip);
            newTrack.cache.OnClipAdded(clip);
            clip.FrameSpanChanged += newTrack.frameSpanChangedHandler;
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
        public void RemoveAllClips() {
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

        public static FrameSpan GetSpanUntilClipOrLimitedDuration(Track track, long frame, long defaultDuration = 300, long maximumDurationToClip = 100000000) {
            if (TryGetSpanUntilClip(track, frame, out var span, defaultDuration, maximumDurationToClip))
                return span;
            return new FrameSpan(frame, defaultDuration);
        }

        /// <summary>
        /// Tries to calculate a frame span that can fill in the space, starting at the frame parameter and extending
        /// either the unlimitedDuration parameter or the amount of space between frame and the nearest clip.
        /// When a clip intersects frame, this method returns false. Use <see cref="GetSpanUntilClipOrLimitedDuration"/> to return a span with defaultDuration instead
        /// </summary>
        /// <param name="track"></param>
        /// <param name="frame"></param>
        /// <param name="span">The output span</param>
        /// <param name="defaultDuration">The default duration for the span when there are no clips in the way</param>
        /// <param name="maxDurationLimit">An upper limit for how long the output span can be</param>
        /// <returns></returns>
        public static bool TryGetSpanUntilClip(Track track, long frame, out FrameSpan span, long defaultDuration = 300, long maxDurationLimit = 100000000U) {
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
                span = new FrameSpan(frame, Math.Min(minimum - frame, maxDurationLimit));
            }
            else {
                span = new FrameSpan(frame, defaultDuration);
            }

            return true;
        }

        /// <summary>
        /// Destroys this track, recursively destroying all resources associated with it (e.g. all effects,
        /// all clips and their effects and so on). This is literally just dispose but with a different name,
        /// as a destroyed track could be reused, though it shouldn't really be
        /// </summary>
        public void Destroy() {
            using (ErrorList list = new ErrorList()) {
                for (int i = this.clips.Count - 1; i >= 0; i--) {
                    Clip clip = this.clips[i];
                    clip.Destroy();

                    try {
                        this.RemoveClipAt(i);
                    }
                    catch (Exception e) {
                        list.Add(e);
                    }
                }
            }
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