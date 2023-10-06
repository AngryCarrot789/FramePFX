using System;
using System.Collections.Generic;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.Editor.ZSystem;
using FramePFX.Logger;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class Track : ZObject, IProjectBound, IAutomatable {
        private readonly List<Clip> clips;
        public int IndexInTimeline;

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
        public string TrackColour { get; set; }

        public long PreviousLargestFrameInUse => this.cache.PreviousLargestActiveFrame;

        public long LargestFrameInUse => this.cache.LargestActiveFrame;

        /// <summary>
        /// This track's automation data
        /// </summary>
        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        private readonly ClipRangeCache cache;
        private bool isPerformingOptimisedCacheRemoval;

        protected Track() {
            this.clips = new List<Clip>();
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
            this.cache.OnLocationChanged(clip, oldSpan);
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

        /// <summary>
        /// Gets the index of the given clip within this track
        /// </summary>
        /// <param name="clip">The clip</param>
        /// <param name="index">The index of the clip</param>
        /// <returns>True if the clip is stored in this track, or false if it is not</returns>
        public bool GetClipIndex(Clip clip, out int index) {
            index = clip.IndexInTrack;
            if (index == -1) {
                return false;
            }
            else if (index < this.clips.Count && ReferenceEquals(clip, this.clips[index])) {
                return true;
            }
            else if (!ReferenceEquals(clip.Track, this)) {
                index = -1;
                return false;
            }
            else {
                // this section down here shouldn't really be reachable... but who knows
                index = this.clips.IndexOf(clip);
                if (index == -1) {
                    AppLogger.WriteLine("[FATAL] Clip's cached index and owner track was still valid, but the track did not contain the clip");
                    return false;
                }
                else {
                    AppLogger.WriteLine("[FATAL] Clip's cached index within the track was invalid, but the owner track was still valid");
                    clip.IndexInTrack = index;
                    return true;
                }
            }
        }

        public void GetClipsAtFrame(long frame, List<Clip> list) {
            List<Clip> src = this.clips;
            int count = src.Count, i = 0;
            while (i < count) {
                Clip clip = src[i++];
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
            if (clip.Track != null && clip.Track.GetClipIndex(clip, out _))
                throw new Exception("Clip already exists and is valid in another track: " + clip.Track);
            Clip.SetTrack(clip, this);
            this.clips.Insert(index, clip);
            clip.IndexInTrack = index;
            this.cache.OnClipAdded(clip);
        }

        public bool RemoveClip(Clip clip) {
            if (!this.GetClipIndex(clip, out int index))
                return false;
            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception("Expected clip's track to equal this instance");
            this.clips.RemoveAt(index);
            clip.IndexInTrack = -1;
            if (!this.isPerformingOptimisedCacheRemoval)
                this.cache.OnClipRemoved(clip);
            Clip.SetTrack(clip, null);
        }

        public void RemoveClips(IEnumerable<int> indices) {
            int offset = 0;
            this.isPerformingOptimisedCacheRemoval = true;
            foreach (int index in indices)
                this.RemoveClipAt(index + offset++);
            this.isPerformingOptimisedCacheRemoval = false;
        }

        public void MakeTopMost(Clip clip, int oldIndex, int newIndex) {
            this.clips.MoveItem(oldIndex, newIndex);
            clip.IndexInTrack = newIndex;
            this.cache.MakeTopMost(clip);
        }

        public bool MoveClipToTrack(Clip clip, Track newTrack) {
            if (!this.GetClipIndex(clip, out int index))
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
            this.clips.RemoveAt(index);
            clip.IndexInTrack = -1;
            this.cache.OnClipRemoved(clip);
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
            data.SetString(nameof(this.TrackColour), this.TrackColour);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Clips));
            foreach (Clip clip in this.clips) {
                Clip.WriteSerialisedWithId(list.AddDictionary(), clip);
            }
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            this.Height = data.GetDouble(nameof(this.Height), 60);
            this.TrackColour = data.TryGetString(nameof(this.TrackColour), out string colour) ? colour : TrackColours.GetRandomColour();
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEBase entry in data.GetList(nameof(this.Clips)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                Clip clip = Clip.ReadSerialisedWithId(dictionary);
                this.AddClip(clip);
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
}