using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.AudioClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class AudioLayerModel : LayerModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoLayerModel), nameof(Volume), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKey IsMutedKey = AutomationKey.RegisterBool(nameof(VideoLayerModel), nameof(IsMuted), new KeyDescriptorBoolean(false));

        public double Volume { get; set; }

        public bool IsMuted { get; set; }

        public AudioLayerModel(TimelineModel timeline) : base(timeline) {
            this.Volume = 1d;
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