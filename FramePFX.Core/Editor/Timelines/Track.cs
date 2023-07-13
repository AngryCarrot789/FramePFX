using System;
using System.Collections.Generic;
using System.Diagnostics;
using FramePFX.Core.Automation;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timelines {
    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class Track : IAutomatable, IRBESerialisable {
        /// <summary>
        /// The timeline that created this track. Will never be null
        /// </summary>
        public Timeline Timeline { get; private set; }

        /// <summary>
        /// This track's clips (unordered)
        /// </summary>
        public List<Clip> Clips { get; }

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

        public AutomationEngine AutomationEngine => this.Timeline.AutomationEngine;

        public bool IsAutomationChangeInProgress { get; set; }

        protected Track() {
            this.Clips = new List<Clip>();
            this.MinHeight = 21;
            this.MaxHeight = 200;
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
        }

        public static void SetTimeline(Track track, Timeline timeline) {
            Timeline oldTimeline = track.Timeline;
            if (ReferenceEquals(oldTimeline, timeline)) {
                Debug.WriteLine("Attempted to set the timeline to the same value");
            }
            else {
                track.Timeline = timeline;
                track.OnTimelineChanged(oldTimeline, timeline);
            }
        }

        public virtual void OnTimelineChanged(Timeline oldTimeline, Timeline timeline) {
            foreach (Clip clip in this.Clips) {
                clip.OnTrackTimelineChanged(oldTimeline, timeline);
            }
        }

        public void GetClipsAtFrame(long frame, List<Clip> list) {
            List<Clip> src = this.Clips;
            int count = src.Count, i = 0;
            while (i < count) {
                Clip clip = src[i++];
                if (clip.IntersectsFrameAt(frame)) {
                    list.Add(clip);
                }
            }
        }

        public Clip GetClipAtFrame(long frame) {
            List<Clip> src = this.Clips;
            int i = 0, c = src.Count;
            while (i < c) {
                Clip clip = src[i++];
                long begin = clip.FrameBegin;
                long duration = clip.FrameDuration;
                if (frame >= begin && frame < (begin + duration))
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
            List<Clip> list = this.Clips;
            for (int i = 0, count = list.Count; i < count; i++) {
                if (list[i].IntersectsFrameAt(frame)) {
                    indices.Add(i);
                }
            }
        }

        public void AddClip(Clip model) {
            this.InsertClip(this.Clips.Count, model);
        }

        public void InsertClip(int index, Clip model) {
            this.Clips.Insert(index, model);
            Clip.SetTrack(model, this);
        }

        public bool RemoveClip(Clip model) {
            int index = this.Clips.IndexOf(model);
            if (index < 0) {
                return false;
            }

            this.RemoveClipAt(index);
            return true;
        }

        public void RemoveClipAt(int index) {
            Clip clip = this.Clips[index];
            if (!ReferenceEquals(this, clip.Track)) {
                throw new Exception("Expected model (to remove)'s track to equal this instance");
            }

            Clip.SetTrack(clip, null);
            this.Clips.RemoveAt(index);
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
            foreach (Clip clip in this.Clips) {
                if (!(clip.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + clip.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(Clip.FactoryId), id);
                clip.WriteToRBE(dictionary.CreateDictionary("Data"));
            }
        }

        public virtual void ReadFromRBE(RBEDictionary data) {
            this.DisplayName = data.GetString(nameof(this.DisplayName), null);
            this.MinHeight = data.GetDouble(nameof(this.MinHeight), 40);
            this.MaxHeight = data.GetDouble(nameof(this.MaxHeight), 200);
            this.Height = data.GetDouble(nameof(this.Height), 60);
            this.TrackColour = data.TryGetString(nameof(this.TrackColour), out string colour) ? colour : TrackColours.GetRandomColour();
            this.AutomationData.ReadFromRBE(data.GetDictionary(nameof(this.AutomationData)));
            foreach (RBEBase entry in data.GetList(nameof(this.Clips)).List) {
                if (!(entry is RBEDictionary dictionary))
                    throw new Exception($"Resource dictionary contained a non dictionary child: {entry.Type}");
                string id = dictionary.GetString(nameof(Clip.FactoryId));
                Clip clip = ClipRegistry.Instance.CreateModel(id);
                clip.ReadFromRBE(dictionary.GetDictionary("Data"));
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
            List<Clip> clips = this.Clips;
            for (int i = 0, c = clips.Count; i < c; i++)
                max = Math.Max(max, clips[i].FrameEndIndex);
            return max;
        }

        public bool GetUsedFrameSpan(out FrameSpan span) {
            using (var enumerator = this.Clips.GetEnumerator()) {
                if (enumerator.MoveNext()) {
                    span = enumerator.Current.FrameSpan;
                    while (enumerator.MoveNext())
                        span = span.MinMax(enumerator.Current.FrameSpan);
                    return true;
                }
            }

            span = default;
            return false;
        }

        public void GetUsedFrameSpan(ref long begin, ref long endIndex) {
            foreach (Clip clip in this.Clips) {
                FrameSpan span = clip.FrameSpan;
                begin = Math.Min(begin, span.Begin);
                endIndex = Math.Max(endIndex, span.EndIndex);
            }
        }
    }
}