using FramePFX.Render;

namespace FramePFX.Timeline.Layer.Clips {
    public abstract class VideoClipViewModel : ClipViewModel, IClipRenderTarget {
        protected VideoClipViewModel() {

        }

        public abstract void Render(IViewPort ogl, long frame);
    }
}
