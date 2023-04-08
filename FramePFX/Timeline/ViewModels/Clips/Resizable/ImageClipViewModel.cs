using FramePFX.Render;
using FramePFX.ResourceManaging.VideoResources;
using OpenTK.Graphics.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace FramePFX.Timeline.ViewModels.Clips.Resizable {
    public class ImageClipViewModel : PositionableClipViewModel {
        private ImageResourceViewModel resource;
        public ImageResourceViewModel Resource {
            get => this.resource;
            set => RaisePropertyChanged(ref this.resource, value);
        }

        private FrameBuffer buffer;

        public override void RenderCore(IViewPort vp, long frame) {
            if (this.Resource == null || this.Resource.ImageData == null) {
                return;
            }

            if (this.buffer == null) {
                this.CreateFrameBuffer();
            }

            this.buffer.Use();
            GL.texture
            this.buffer.Unuse();
        }

        public void CreateFrameBuffer() {
            Image<Bgra32> image = this.resource.ImageData;
            this.buffer = FrameBuffer.Create(image.Width, image.Height);
        }
    }
}