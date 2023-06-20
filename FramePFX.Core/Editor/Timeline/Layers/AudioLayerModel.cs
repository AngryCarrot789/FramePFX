using FramePFX.Core.Editor.Timeline.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class AudioLayerModel : LayerModel {
        public float Volume { get; set; }

        public bool IsMuted { get; set; }

        public AudioLayerModel(TimelineModel timeline) : base(timeline) {
            this.Volume = 1f;
            this.IsMuted = false;
        }

        public override LayerModel CloneCore() {
            AudioLayerModel layer = new AudioLayerModel(this.Timeline) {
                Volume = this.Volume,
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                LayerColour = this.LayerColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            foreach (ClipModel clip in this.Clips) {
                // assert clip is AudioClipModel
                // assert CanAccept(clip)
                layer.AddClip(clip.Clone());
            }

            return layer;
        }

        public override bool CanAccept(ClipModel clip) {
            return clip is AudioClipModel;
        }
    }
}