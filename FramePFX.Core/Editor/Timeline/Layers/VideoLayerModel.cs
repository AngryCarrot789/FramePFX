using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Editor.Timeline.VideoClips;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.Timeline.Layers {
    public class VideoLayerModel : LayerModel {
        public static readonly AutomationKey OpacityKey = AutomationKey.RegisterDouble(nameof(VideoLayerModel), nameof(Opacity), new KeyDescriptorDouble(1d, 0d, 1d));
        public static readonly AutomationKey IsVisibleKey = AutomationKey.RegisterBool(nameof(VideoLayerModel), nameof(IsVisible), new KeyDescriptorBoolean(true));
        public const double MinimumVisibleOpacity = 0.0001d;

        /// <summary>
        /// The opacity of the layer, from 0d to 1d. When the value dips below <see cref="MinimumVisibleOpacity"/>, it is effectively invisible and won't be rendered
        /// </summary>
        public double Opacity { get; set; }

        /// <summary>
        /// A visibility state, user switchable
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Returns when <see cref="IsVisible"/> is true and <see cref="Opacity"/> is greater than <see cref="MinimumVisibleOpacity"/>
        /// </summary>
        public bool IsActuallyVisible => this.IsVisible && this.Opacity > MinimumVisibleOpacity;

        public VideoLayerModel(TimelineModel timeline) : base(timeline) {
            this.Opacity = 1d;
            this.IsVisible = true;
            this.AutomationData.AssignKey(OpacityKey, (s, f) => this.Opacity = s.GetDoubleValue(f));
            this.AutomationData.AssignKey(IsVisibleKey, (s, f) => this.IsVisible = s.GetBooleanValue(f));
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

        public override bool CanUpdateAutomation() {
            return base.CanUpdateAutomation() && this.IsActuallyVisible;
        }
    }
}