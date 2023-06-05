using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline {
    public class TimelineModel {
        public ProjectModel Project { get; }

        public List<TimelineLayerModel> Layers { get; }

        public long PlayHead { get; set; }

        public long MaxDuration { get; set; }

        public TimelineModel(ProjectModel project) {
            this.Project = project;
            this.Layers = new List<TimelineLayerModel>();
        }

        public void AddLayer(TimelineLayerModel layer) {
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            this.Layers.Add(layer);
        }

        public bool RemoveLayer(TimelineLayerModel layer) {
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            int index = this.Layers.IndexOf(layer);
            if (index < 0) {
                return false;
            }

            this.Layers.RemoveAt(index);
            return true;
        }

        public void RemoveLayer(int index) {
            TimelineLayerModel layer = this.Layers[index];
            Validate.Exception(ReferenceEquals(layer.Timeline, this), "Expected layer's timeline and the current timeline instance to be equal");
            this.Layers.RemoveAt(index);
        }

        public void ClearLayers() {
            this.Layers.Clear();
        }

        public void Render(RenderContext render) {
            this.Render(render, this.PlayHead);
        }

        public void Render(RenderContext render, long frame) {
            List<ClipModel> clips = this.GetClipsAtFrame(frame).ToList();
            foreach (ClipModel layer in clips) {
                if (layer is VideoClipModel vc) {
                    vc.Render(render, frame);
                }
            }
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return Enumerable.Reverse(this.Layers).SelectMany(layer => layer.GetClipsAtFrame(frame));
        }
    }
}