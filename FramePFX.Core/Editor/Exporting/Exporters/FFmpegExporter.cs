using System;
using System.Threading.Tasks;
using FramePFX.Core.Rendering;
using FramePFX.Core.Utils;
using SkiaSharp;

namespace FramePFX.Core.Editor.Exporting.Exporters {
    public class FFmpegExporter : IExportService {
        public void Export(ProjectModel project, IExportProgress progress, ExportProperties properties) {
            FrameSpan duration = properties.Span;
            Resolution resolution = project.Settings.Resolution;
            Rational frameRate = project.Settings.FrameRate;
            // platform color type is typically SKColorType.Rgba8888... i think?
            SKImageInfo frameInfo = new SKImageInfo(resolution.Width, resolution.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            
            IntPtr backBuffer = IntPtr.Zero; // TODO: back buffer... somehow?
            using (SKSurface surface = SKSurface.Create(frameInfo, backBuffer, frameInfo.RowBytes)) {
                if (surface == null) {
                    throw new Exception("Failed to create SKSurface");
                }

                RenderContext context = new RenderContext(surface, surface.Canvas, frameInfo);
                for (long frame = duration.Begin, end = duration.EndIndex; frame < end; frame++) {
                    context.Canvas.Clear(SKColors.Black);
                    project.AutomationEngine.TickProjectAtFrame(frame);
                    project.Timeline.Render(context, frame);
                    progress.OnFrameCompleted(frame);
                    surface.Flush();
                    // pass pixels to FFmpeg and encode
                }
                // this.OnPaintSurface(new SKPaintSurfaceEventArgs(surface, skImageInfo.WithSize(size2), skImageInfo));
            }

            // flush and close FFmpeg streams, export complete... i guess?
        }
    }
}