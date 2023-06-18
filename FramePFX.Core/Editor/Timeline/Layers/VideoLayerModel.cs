using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class VideoLayerModel : LayerModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoLayerModel), nameof(Opacity), new KeyDescriptorDouble(1d));

        public float Opacity { get; set; }

        public bool IsVisible { get; set; }

        public VideoLayerModel(TimelineModel timeline) : base(timeline) {
            this.Opacity = 1f;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey);
        }

        public override LayerModel CloneCore() {
            VideoLayerModel layer = new VideoLayerModel(this.Timeline) {
                Opacity = this.Opacity,
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                LayerColour = this.LayerColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            foreach (ClipModel clip in this.Clips) {
                // assert clip is VideoClipModel
                // assert CanAccept(clip)
                layer.AddClip(clip.Clone());
            }

            return layer;
        }

        public override bool CanAccept(ClipModel clip) {
            return clip is VideoClipModel;
        }
    }
}