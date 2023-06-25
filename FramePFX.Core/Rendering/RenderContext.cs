using FramePFX.Core.Editor;
using SkiaSharp;

namespace FramePFX.Core.Rendering {
    public sealed class RenderContext {
        /// <summary>
        /// The target render surface
        /// </summary>
        public SKSurface Surface { get; }

        /// <summary>
        /// The surface's canvas
        /// </summary>
        public SKCanvas Canvas { get; }

        /// <summary>
        /// The image info about the surface
        /// </summary>
        public SKImageInfo FrameInfo { get; }

        public RenderContext(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) {
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = frameInfo;
        }
    }
}