using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class VideoLayerModel : LayerModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoLayerModel), nameof(Opacity), new KeyDescriptorDouble(1d, 0d, 1d));

        public double Opacity { get; set; }

        public bool IsVisible { get; set; }

        public VideoLayerModel(TimelineModel timeline) : base(timeline) {
            this.Opacity = 1d;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey);
        }

        public override LayerModel CloneCore() {
            VideoLayerModel layer = new VideoLayerModel(this.Timeline) {
                MaxHeight = this.MaxHeight,
                MinHeight = this.MinHeight,
                Height = this.Height,
                LayerColour = this.LayerColour,
                DisplayName = TextIncrement.GetNextText(this.DisplayName)
            };

            this.AutomationData.LoadDataIntoClone(layer.AutomationData);
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

        public override void UpdateAutomationValues(long frame) {
            if (this.AutomationData[OpacityKey].IsAutomationInUse) {
                this.Opacity = this.AutomationData[OpacityKey].GetDoubleValue(frame);
            }

            base.UpdateAutomationValues(frame);
        }
    }
}