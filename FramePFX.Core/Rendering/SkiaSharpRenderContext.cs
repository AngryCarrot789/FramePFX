using SkiaSharp;

namespace FramePFX.Core.Rendering {
    public class SkiaSharpRenderContext : RenderContext {
        public SkiaSharpRenderContext(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) : base(surface, canvas, frameInfo) {
            
        }
    }
}