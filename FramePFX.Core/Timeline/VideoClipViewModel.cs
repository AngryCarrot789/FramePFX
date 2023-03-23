using FramePFX.Core.Render;

namespace FramePFX.Core.Timeline {
    public abstract class VideoClipViewModel : ClipViewModel {
        protected VideoClipViewModel() {

        }

        public abstract void Render(IOGLViewPort ogl, long frame);
    }
}
