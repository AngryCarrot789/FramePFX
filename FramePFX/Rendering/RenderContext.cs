using System.Numerics;
using SkiaSharp;

namespace FramePFX.Rendering {
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

        /// <summary>
        /// The size of the rendering canvas, e.g. 1920,1080
        /// </summary>
        public Vector2 FrameSize { get; }

        public RenderContext(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) {
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = frameInfo;
            this.FrameSize = new Vector2(frameInfo.Width, frameInfo.Height);
        }

        /// <summary>
        /// Clears the context's drawing canvas
        /// </summary>
        public void ClearContext() {
            this.Canvas.Clear(SKColors.Black);
        }
    }
}