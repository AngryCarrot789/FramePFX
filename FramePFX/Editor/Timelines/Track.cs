using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Automation;
using FramePFX.Editor.Registries;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class Track : IAutomatable {
        private readonly List<Clip> clips;

        /// <summary>
        /// The timeline that created this track
        /// </summary>
        public Timeline Timeline { get; private set; }

        /// <summary>
        /// This track's clips (unordered)
        /// </summary>
        public IReadOnlyList<Clip> Clips => this.clips;

        /// <summary>
        /// This track's registry ID, used to create instances dynamically through the <see cref="TrackRegistry"/>
        /// </summary>
        public string RegistryId => TrackRegistry.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// A readable layer name
        /// </summary>
        public string DisplayName { get; set; }

        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public double Height { get; set; }
        public string TrackColour { get; set; }

        /// <summary>
        /// This track's automation data
        /// </summary>
        public AutomationData AutomationData { get; }

        public bool IsAutomationChangeInProgress { get; set; }

        protected Track() {
            this.clips = new List<Clip>();
            this.MinHeight = 21;
            this.MaxHeight = 200;
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
        }

        public static void SetTimeline(Track track, Timeline timeline) {
            Timeline oldTimeline = track.Timeline;
            if (!ReferenceEquals(oldTimeline, timeline)) {
                track.OnTimelineChanging(timeline);
                track.Timeline = timeline;
                foreach (Clip clip in track.Clips) {
                    Clip.OnTrackTimelineChanged(clip, oldTimeline, timeline);
                }

                track.OnTimelineChanged(oldTimeline);
            }
        }

        /// <summary>
        /// Called when this track is about to be moved to a new timeline. <see cref="Timeline"/> is
        /// the previous timeline, and <see cref="newTimeline"/> is the new one
        /// </summary>
        /// <param name="newTimeline">The new timeline. May be null, meaning this track is being removed</param>
        protected virtual void OnTimelineChanging(Timeline newTimeline) {

        }

        /// <summary>
        /// Called when this track is moved from one timeline to another
        /// </summary>
        /// <param name="oldTimeline">The previous timeline. May be null, meaning this track was added to a timeline</param>
        public virtual void OnTimelineChanged(Timeline oldTimeline) {

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
            List<Clip> src = this.clips;
            int i = 0, c = src.Count;
            while (i < c) {
                Clip clip = src[i++];
                long begin = clip.FrameBegin;
                if (frame >= begin && frame < begin + clip.FrameDuration)
                    return clip;
            }

            return null;

            // cannot use binary search until Clips is ordered
            //List<Clip> src = this.Clips;
            //int a = 0, b = src.Count - 1;
            //while (a <= b) {
            //    int mid = (a + b) / 2;
            //    Clip clip = src[mid];
            //    if (clip.IntersectsFrameAt(frame)) {
            //        return clip;
            //    }
            //    else if (frame < clip.FrameBegin) {
            //        b = mid - 1;
            //    }
            //    else {
            //        a = mid + 1;
            //    }
            //}
            //return null;
        }

        public void GetClipIndicesAt(long frame, ICollection<int> indices) {
            List<Clip> list = this.clips;
            for (int i = 0, count = list.Count; i < count; i++) {
                if (list[i].IntersectsFrameAt(frame)) {
                    indices.Add(i);
                }
            }
        }

        public void AddClip(Clip clip) {
            this.InsertClip(this.clips.Count, clip);
        }

        public void InsertClip(int index, Clip clip) {
            this.clips.Insert(index, clip);
            Clip.SetTrack(clip, this);
        }

        public bool RemoveClip(Clip clip) {
            int index = this.clips.IndexOf(clip);
            if (index < 0) {
                return false;
            }

            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.clips[index];
            if (!ReferenceEquals(this, clip.Track))
                throw new Exception("Expected clip's track to equal this instance");
            Clip.SetTrack(clip, null);
            this.clips.RemoveAt(index);
        }

        public void MoveClipIndex(int oldIndex, int newIndex) {
            this.clips.MoveItem(oldIndex, newIndex);
        }

        public abstract Track CloneCore();

        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetDouble(nameof(this.MinHeight), this.MinHeight);
            data.SetDouble(nameof(this.MaxHeight), this.MaxHeight);
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
            this.MinHeight = data.GetDouble(nameof(this.MinHeight), 40);
            this.MaxHeight = data.GetDouble(nameof(this.MaxHeight), 200);
            this.Height = data.GetDouble(nameof(this.Height), 60);
            this.TrackColour = data.TryGetString(nameof(this.TrackColour), out string colour) ? colour : TrackColours.GetRandomColour();
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            this.AutomationData.UpdateBackingStorage();
            foreach (RBEBase entry in data.GetList(nameof(this.Clips)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                Clip clip = Clip.ReadSerialisedWithId(dictionary);
                this.AddClip(clip);
            }
        }

        public abstract bool IsClipTypeAcceptable(Clip clip);

        /// <summary>
        /// Returns the maximum (exclusive) frame that any clip exists at
        /// <para>
        /// This is the same as doing Clips.Max(x => x.FrameEndIndex)
        /// </para>
        /// </summary>
        public long GetMaxDuration() {
            long max = 0L;
            for (int i = this.clips.Count - 1; i >= 0; i--)
                max = Math.Max(max, this.clips[i].FrameEndIndex);
            return max;
        }

        public bool GetUsedFrameSpan(out FrameSpan span) {
            return FrameSpan.TryUnionAll(this.clips.Select(x => x.FrameSpan), out span);
        }

        public void GetUsedFrameSpan(ref long begin, ref long endIndex) {
            foreach (Clip clip in this.clips) {
                FrameSpan span = clip.FrameSpan;
                begin = Math.Min(begin, span.Begin);
                endIndex = Math.Max(endIndex, span.EndIndex);
            }
        }

        /// <summary>
        /// Clears all clips in this track
        /// </summary>
        public void Clear() {
            for (int i = this.clips.Count - 1; i >= 0; i--) {
                this.RemoveClipAt(i);
            }
        }
    }
}