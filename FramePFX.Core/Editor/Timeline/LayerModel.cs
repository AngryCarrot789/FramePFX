using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    /// <summary>
    /// A model that represents a timeline layer, which can contain clips
    /// </summary>
    public abstract class LayerModel {
        public TimelineModel Timeline { get; }

        public List<ClipModel> Clips { get; }

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

        public void AddClip(ClipModel model, bool setLayer = true) {
            this.InsertClip(this.Clips.Count, model, setLayer);
        }

        public void InsertClip(int index, ClipModel model, bool setLayer = true) {
            this.Clips.Insert(index, model);
            if (setLayer) {
                ClipModel.SetLayer(model, this);
            }
        }

        public bool RemoveClip(ClipModel model, bool clearLayer = true) {
            Validate.Exception(ReferenceEquals(this, model.Layer), "Expected model (to remove)'s layer to equal this instance");
            int index = this.Clips.IndexOf(model);
            if (index < 0) {
                return false;
            }

            this.RemoveClipAt(index, clearLayer);
            return true;
        }

        public void RemoveClipAt(int index, bool clearLayer = true) {
            ClipModel clip = this.Clips[index];
            Validate.Exception(ReferenceEquals(this, clip.Layer), "Expected model (to remove)'s layer to equal this instance");
            this.Clips.RemoveAt(index);
            if (clearLayer) {
                ClipModel.SetLayer(clip, null);
            }
        }

        public abstract LayerModel CloneCore();
    }
}