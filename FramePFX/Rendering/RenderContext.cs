using System.Collections.Generic;
using System.Numerics;
using FramePFX.Editor.Timelines.VideoClips;
using SkiaSharp;

namespace FramePFX.Rendering
{
    public sealed class RenderContext
    {
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

        private List<(VideoClip, SKRect)> clipBoxes;

        public List<(VideoClip, SKRect)> ClipBoundingBoxes => this.clipBoxes ?? (this.clipBoxes = new List<(VideoClip, SKRect)>());

        /// <summary>
        /// Gets or sets a value that states if the render process should provide clip bounding box information (added to <see cref="ClipBoundingBoxes"/>)
        /// </summary>
        public bool ShouldProvideClipBounds;

        public RenderContext(SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo)
        {
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = frameInfo;
            this.FrameSize = new Vector2(frameInfo.Width, frameInfo.Height);
        }

        /// <summary>
        /// Clears the context's drawing canvas
        /// </summary>
        public void ClearPixels()
        {
            this.Canvas.Clear(SKColors.Black);
        }
    }
}