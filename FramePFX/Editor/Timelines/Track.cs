using System;
using System.Collections;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editor.Timelines {
    public delegate void ClipInsertedIntoTrackEventHandler(Track track, Clip clip, int index);
    public delegate void ClipRemovedFromTrackEventHandler(Track track, Clip clip, int index);

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
        public ClipList Clips => this.clips;

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

        public event ClipInsertedIntoTrackEventHandler ClipInserted;
        public event ClipRemovedFromTrackEventHandler ClipRemoved;

        protected Track() {
            this.clips = new ClipList();
            this.cache = new ClipRangeCache();
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
        }

        /// <summary>
        /// Invoked when a clip's frame span changes, only when it is placed in this specific track. This MUST be called, otherwise the cache will get corrupted
        /// </summary>
        /// <param name="clip">The clip whose frame span changed</param>
        /// <param name="oldSpan">The old frame span</param>
        public void OnClipFrameSpanChanged(Clip clip, FrameSpan oldSpan) {
            if (!ReferenceEquals(clip.Track, this))
                throw new Exception("Clip's track does not match the current instance");
            this.cache.OnSpanChanged(clip, oldSpan);
            // this.Timeline?.UpdateLargestFrame();
        }

        public static void SetTimeline(Track track, Timeline timeline) {
            Timeline oldTimeline = track.Timeline;
            if (!ReferenceEquals(oldTimeline, timeline)) {
                track.Timeline = timeline;
                track.OnTimelineChanging(oldTimeline);
                foreach (Clip clip in track.Clips) {
                    Clip.InternalOnTrackTimelineChanged(clip, oldTimeline, timeline);
                }

                track.OnTimelineChanged(oldTimeline);
            }
        }

        public static void OnTimelineProjectChanged(Track track, Project oldProject, Project newProject) {
            track.OnProjectChanging(oldProject, newProject);
            foreach (Clip clip in track.clips) {
                Clip.InternalOnTrackTimelineProjectChanged(clip, oldProject, newProject);
            }

            track.OnProjectChanged(oldProject, newProject);
        }

        /// <summary>
        /// Called when this track is about to be moved to a new timeline. <see cref="Timeline"/> is
        /// the previous timeline, and <see cref="newTimeline"/> is the new one
        /// </summary>
        /// <param name="oldTimeline">The previous timeline. May be null, meaning this track was added to a timeline</param>
        protected virtual void OnTimelineChanging(Timeline oldTimeline) {
        }

        /// <summary>
        /// Called when this track is moved from one timeline to another
        /// </summary>
        /// <param name="oldTimeline">The previous timeline. May be null, meaning this track was added to a timeline</param>
        protected virtual void OnTimelineChanged(Timeline oldTimeline) {
        }

        protected virtual void OnProjectChanging(Project oldProject, Project newProject) {
        }

        protected virtual void OnProjectChanged(Project oldProject, Project newProject) {
        }

        public bool TryGetIndexOfClip(Clip clip, out int index) {
            return (index = this.IndexOfClip(clip)) != -1;
        }

        public int IndexOfClip(Clip clip) {
            // List<Clip> list = this.clips;
            // for (int i = 0, count = list.Count; i < count; i++) {
            //     if (ReferenceEquals(this.clips[i], clip)) {
            //         return i;
            //     }
            // }
            // return -1;
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

        // private static void CheckForRecursion(Timeline timeline, Clip clip, HashSet<Timeline> timelines = null) {
        //     if (timelines == null)
        //         timelines = new HashSet<Timeline>();
        //     else if (timelines.Contains(timeline))
        //         throw new Exception("Recursive error while adding clip");
        //     else
        //         timelines.Add(timeline);
        //     if (timeline is CompositionTimeline composition) {
        //         foreach (ResourcePath path in composition.Owner.References) {
        //             if (path.Owner.ResourceHelper.Owner is Clip ownerClip && ownerClip.Track != null && ownerClip.Track.Timeline != null) {
        //                 CheckForRecursion(ownerClip.Track.Timeline, clip, timelines);
        //             }
        //         }
        //     }
        // }
        // 
        // private static bool DoesTimelineUseResourceId(Timeline timeline, ulong id) {
        //     foreach (Track track in timeline.Tracks) {
        //         foreach (Clip clip in track.clips) {
        //             foreach (IBaseResourcePathKey key in clip.ResourceHelper.RegisteredKeys) {
        //                 if (key.Path != null && key.Path.ResourceId == id) {
        //                     return true;
        //                 }
        //             }
        //         }
        //     }
        // 
        //     return false;
        // }

        public void InsertClip(int index, Clip clip) {
            if (clip.Track != null && clip.Track.TryGetIndexOfClip(clip, out _))
                throw new Exception("Clip already exists and is valid in another track: " + clip.Track);

            Clip.InternalSetTrack(clip, this);
            this.clips.Insert(index, clip);
            this.cache.OnClipAdded(clip);
            this.ClipInserted?.Invoke(this, clip, index);
        }

        public bool RemoveClip(Clip clip) {
            if (!this.TryGetIndexOfClip(clip, out int index))
                return false;
            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception("Expected clip's track to equal this instance");
            this.clips.RemoveAt(index);
            this.cache.OnClipRemoved(clip);
            Clip.InternalSetTrack(clip, null);
            this.ClipRemoved?.Invoke(this, clip, index);
        }

        public bool MoveClipToTrack(Clip clip, Track newTrack) {
            if (!this.TryGetIndexOfClip(clip, out int index))
                return false;
            this.MoveClipToTrack(index, newTrack);
            return true;
        }

        /// <summary>
        /// Removes the clip at the given index from ourself, then adds that clip to the given track
        /// </summary>
        /// <param name="index"></param>
        /// <param name="newTrack"></param>
        public void MoveClipToTrack(int index, Track newTrack) {
            Clip clip = this.clips[index];
            this.RemoveClipAt(index);
            newTrack.AddClip(clip);
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
                foreach (Clip clip in this.Clips)
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

        public bool IsRegionEmpty(FrameSpan span) => this.cache.IsRegionEmpty(span);
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

        // public void Add(Clip item) {
        //     if (this.size == this.items.Length)
        //         this.EnsureCapacity(this.size + 1);
        //     this.items[this.size++] = item;
        // }

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