using FramePFX.Render;

namespace FramePFX.Timeline {
    public abstract class VideoClipViewModel : ClipViewModel {
        protected VideoClipViewModel() {

        }

        public abstract void Render(IOGLViewPort ogl, long frame);
    }
}
