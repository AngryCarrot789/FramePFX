using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class VideoClipViewModel : ClipViewModel {
        public new VideoClipModel Model => (VideoClipModel) base.Model;

        public VideoClipViewModel(VideoClipModel model) : base(model) {

        }

        public virtual void OnInvalidateVisual() {

        }
    }
}