using FramePFX.Core.Editor;
using FramePFX.Core.Rendering;
using SkiaSharp;
using SkiaSharp.Views.Desktop;

namespace FramePFX {
    public static class SKRenderUtils {
        public static void RenderFrame(ProjectModel project, SKPaintSurfaceEventArgs e) {
            if (project.IsSaving) {
                return;
            }

            RenderContext context = new SkiaSharpRenderContext(e.Surface, e.Surface.Canvas, e.RawInfo);
            context.Canvas.Clear(SKColors.Black);
            project.Timeline.Render(context);
        }
    }
}