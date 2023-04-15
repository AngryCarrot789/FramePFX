using FramePFX.Core.Utils;

namespace FramePFX.Timeline.ViewModels.Layer {
    public class VideoTimelineLayer : TimelineLayer {
        private float opacity;

        /// <summary>
        /// The opacity of this layer. Between 0f and 1f (not yet implemented properly)
        /// </summary>
        public float Opacity {
            get => this.opacity;
            set => this.RaisePropertyChanged(ref this.opacity, Maths.Clamp(value, 0f, 1f), () => this.Timeline.MarkRenderDirty());
        }

        public VideoTimelineLayer(EditorTimeline timeline) : base(timeline) {
            this.Opacity = 1f;
        }
    }
}