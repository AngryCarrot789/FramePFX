using System.Drawing.Design;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Timeline.Layer.Clips {
    public class VideoClipViewModel : ClipViewModel {
        public VideoClipViewModel(LayerViewModel layer) : base(layer) {

        }

        public void RenderCore() {
            this.Render();
        }

        public virtual void Render() {

        }
    }
}
