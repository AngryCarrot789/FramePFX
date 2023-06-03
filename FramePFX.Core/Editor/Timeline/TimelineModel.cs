using System.Collections.Generic;
using System.Linq;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Rendering;

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

        public void Render(RenderContext render) {
            this.Render(render, this.PlayHead);
        }

        public void Render(RenderContext render, long playhead) {
            List<ClipModel> clips = this.GetClipsAtFrame(playhead).ToList();
            foreach (ClipModel layer in clips) {
                if (layer is VideoClipModel vc) {
                    vc.Render(render, playhead);
                }
            }
        }

        public IEnumerable<ClipModel> GetClipsAtFrame(long frame) {
            return Enumerable.Reverse(this.Layers).SelectMany(layer => layer.GetClipsAtFrame(frame));
        }
    }
}