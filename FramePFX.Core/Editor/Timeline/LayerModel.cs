using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FFmpeg.AutoGen;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    public class LayerModel {
        public TimelineModel Timeline { get; }

        public List<ClipModel> Clips { get; }

        public float Opacity { get; set; }
        public string Name { get; set; }
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public double Height { get; set; }
        public string LayerColour { get; set; }

        public LayerModel(TimelineModel timeline) {
            this.Timeline = timeline;
            this.Clips = new List<ClipModel>();
            this.MinHeight = 40;
            this.MaxHeight = 200;
            this.Height = 60;
            this.LayerColour = LayerColours.GetRandomColour();
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return this.Clips.Where(clip => clip.IntersectsFrameAt(frame));
        }

        public void AddClip(ClipModel model, bool fireLayerChangedEvent = true) {
            this.InsertClip(this.Clips.Count, model, fireLayerChangedEvent);
        }

        public void InsertClip(int index, ClipModel model, bool fireLayerChangedEvent = true) {
            this.Clips.Insert(index, model);
            ClipModel.SetLayer(model, this, fireLayerChangedEvent);
        }

        public bool RemoveClip(ClipModel model, bool fireLayerChangedEvent = true) {
            Validate.Exception(ReferenceEquals(this, model.Layer), "Expected model (to remove)'s layer to equal this instance");
            int index = this.Clips.IndexOf(model);
            if (index < 0) {
                return false;
            }

            this.RemoveClipAt(index, fireLayerChangedEvent);
            return true;
        }

        public void RemoveClipAt(int index, bool fireLayerChangedEvent = true) {
            ClipModel clip = this.Clips[index];
            Validate.Exception(ReferenceEquals(this, clip.Layer), "Expected model (to remove)'s layer to equal this instance");
            this.Clips.RemoveAt(index);
            ClipModel.SetLayer(clip, null, fireLayerChangedEvent);
        }
    }
}