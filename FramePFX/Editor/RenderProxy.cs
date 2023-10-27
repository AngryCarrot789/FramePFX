using FramePFX.Editor.Rendering;
using SkiaSharp;

namespace FramePFX.Editor {
    public class RenderProxy {
        protected long frame;
        protected RenderContext context;

        public RenderProxy() {
        }

        /// <summary>
        /// Called on the main thread, before passing a render task to a scheduler
        /// </summary>
        /// <param name="frame">The frame being rendered</param>
        public virtual void OnPreUpdate(long frame, RenderContext context) {
            this.frame = frame;
            this.context = context;
        }

        /// <summary>
        /// Called on a task scheduler to process a frame
        /// </summary>
        public virtual void OnUpdate(RenderContext ctx) {

        }
        
        public virtual void OnPostUpdate() {

        }
    }
}