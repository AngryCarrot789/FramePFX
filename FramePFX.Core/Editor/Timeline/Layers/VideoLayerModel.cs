using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class VideoLayerModel : LayerModel {
        public float Opacity { get; set; }

        public VideoLayerModel(TimelineModel timeline) : base(timeline) {
            this.Opacity = 1f;
        }

        public override LayerModel CloneCore() {
            VideoLayerModel layer = new VideoLayerModel(this.Timeline) {
                Opacity = this.Opacity,
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                LayerColour = this.LayerColour,
                Name = TextIncrement.GetNextNumber(this.Name)
            };

            foreach (ClipModel clip in this.Clips) {
                layer.AddClip(clip.Clone());
            }

            return layer;
        }
    }
}