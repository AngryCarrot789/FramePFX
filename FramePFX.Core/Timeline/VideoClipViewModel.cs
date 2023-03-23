using FramePFX.Core.Timeline;
using FramePFX.Render;

namespace FramePFX.Timeline.Layer.Clips {
    public abstract class VideoClipViewModel : ClipViewModel {
        protected VideoClipViewModel() {

        }

        public abstract void Render(IOGLViewPort ogl, long frame);
    }
}
