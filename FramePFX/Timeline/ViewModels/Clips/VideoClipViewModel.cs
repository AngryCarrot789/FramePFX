using FramePFX.Render;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline.ViewModels.Clips {
    public abstract class VideoClipViewModel : ClipViewModel, IVideoClip {
        protected VideoClipViewModel() {

        }

        public abstract void Render(IViewPort vp, long frame);
    }
}
