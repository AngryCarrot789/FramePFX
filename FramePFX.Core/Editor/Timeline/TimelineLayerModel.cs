using System.Collections.Generic;
using System.Linq;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineLayerModel {
        public TimelineModel Timeline { get; }

        public List<ClipModel> Clips { get; }

        public float Opacity { get; set; }
        public string Name { get; set; }
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
        public double Height { get; set; }
        public string LayerColour { get; set; }

        public TimelineLayerModel(TimelineModel timeline) {
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

        public void AddClip(ClipModel model) {
            this.Clips.Add(model);
            model.Layer = this;
        }

        public void RemoveClip(ClipModel model) {
            this.Clips.Remove(model);
            model.Layer = null;
        }

        public void RemoveClipAt(int index) {
            ClipModel clip = this.Clips[index];
            this.Clips.RemoveAt(index);
            clip.Layer = null;
        }
    }
}