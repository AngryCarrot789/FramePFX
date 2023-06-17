using FramePFX.Core.Editor;
using SkiaSharp;

namespace FramePFX.Core.Rendering {
    public class SkiaSharpRenderContext : RenderContext {
        public SkiaSharpRenderContext(VideoEditorModel editor, SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) : base(editor, surface, canvas, frameInfo) {
            
        }
    }
}