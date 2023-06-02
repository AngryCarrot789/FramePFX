using FramePFX.Core.Editor.Timeline.Layers;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class VideoLayerViewModel : TimelineLayerViewModel {
        public new VideoLayerModel Model => (VideoLayerModel) base.Model;

        public VideoLayerViewModel(TimelineViewModel timeline, VideoLayerModel model) : base(timeline, model) {

        }
    }
}