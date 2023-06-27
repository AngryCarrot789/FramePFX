using System;
using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Automation;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Editor.Registries;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// Base class for timeline tracks. A track simply contains clips, along with a few extra
    /// properties (like opacity for video tracks or gain for audio tracks, which typically affect all clips)
    /// </summary>
    public abstract class TrackModel : IAutomatable, IRBESerialisable {
        /// <summary>
        /// The timeline that created this track. Will never be null
        /// </summary>
        public TimelineModel Timeline { get; }

        /// <summary>
        /// This track's clips (unordered)
        /// </summary>
        public List<ClipModel> Clips { get; }

        /// <summary>
        /// This track's registry ID, used to create instances dynamically through the <see cref="TrackRegistry"/>
        /// </summary>
        public string RegistryId => TrackRegistry.Instance.GetTypeIdForModel(this.GetType());

        /// <summary>
        /// This track's owning timeline's play head position
        /// </summary>
        public long TimelinePlayhead => this.Timeline.PlayHeadFrame;

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

        protected TrackModel(TimelineModel timeline) {
            this.Timeline = timeline;
            this.Clips = new List<ClipModel>();
            this.MinHeight = 21;
            this.MaxHeight = 200;
            this.Height = 60;
            this.TrackColour = TrackColours.GetRandomColour();
            this.AutomationData = new AutomationData(this);
        }

        long IAutomatable.GetRelativeFrame(long frame) => frame;

        public List<ClipModel> GetClipsAtFrame(long frame) {
            List<ClipModel> src = this.Clips;
            int count = src.Count, i = 0;
            do {
                if (i >= count)
                    return null;
                ClipModel clip = src[i++];
                if (clip.IntersectsFrameAt(frame)) {
                    List<ClipModel> outList = new List<ClipModel> {clip};
                    while (i < count) {
                        clip = src[i++];
                        if (clip.IntersectsFrameAt(frame)) {
                            outList.Add(clip);
                        }
                    }

                    return outList;
                }
            } while (true);
        }

        public ClipModel GetClipAtFrame(long frame) {
            List<ClipModel> src = this.Clips;
            int i = 0, c = src.Count;
            while (i < c) {
                ClipModel clip = src[i++];
                if (clip.IntersectsFrameAt(frame))
                    return clip;
            }
            return null;
        }

        public void AddClip(ClipModel model, bool setTrack = true) {
            this.InsertClip(this.Clips.Count, model, setTrack);
        }

        public void InsertClip(int index, ClipModel model, bool setTrack = true) {
            this.Clips.Insert(index, model);
            if (setTrack) {
                ClipModel.SetTrack(model, this);
            }
        }

        public bool RemoveClip(ClipModel model, bool clearTrack = true) {
            int index = this.Clips.IndexOf(model);
            if (index >= 0) {
                this.RemoveClipAt(index, clearTrack);
                return true;
            }

            return false;
        }

        public void RemoveClipAt(int index, bool clearTrack = true) {
            ClipModel clip = this.Clips[index];
            if (!ReferenceEquals(this, clip.Track)) {
                throw new Exception("Expected model (to remove)'s track to equal this instance");
            }

            this.Clips.RemoveAt(index);
            if (clearTrack) {
                ClipModel.SetTrack(clip, null);
            }
        }

        public abstract TrackModel CloneCore();

        public virtual void WriteToRBE(RBEDictionary data) {
            data.SetString(nameof(this.DisplayName), this.DisplayName);
            data.SetDouble(nameof(this.MinHeight), this.MinHeight);
            data.SetDouble(nameof(this.MaxHeight), this.MaxHeight);
            data.SetDouble(nameof(this.Height), this.Height);
            data.SetString(nameof(this.TrackColour), this.TrackColour);
            this.AutomationData.WriteToRBE(data.CreateDictionary(nameof(this.AutomationData)));
            RBEList list = data.CreateList(nameof(this.Clips));
            foreach (ClipModel clip in this.Clips) {
                if (!(clip.FactoryId is string id))
                    throw new Exception("Unknown clip type: " + clip.GetType());
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString(nameof(ClipModel.FactoryId), id);
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
                string id = dictionary.GetString(nameof(ClipModel.FactoryId));
                ClipModel clip = ClipRegistry.Instance.CreateModel(id);
                clip.ReadFromRBE(dictionary.GetDictionary("Data"));
                this.AddClip(clip);
            }
        }

        public abstract bool IsClipTypeAcceptable(ClipModel clip);

        public virtual bool CanUpdateAutomation() {
            return true;
        }
    }
}