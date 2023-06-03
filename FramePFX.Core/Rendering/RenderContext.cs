using FramePFX.Core.Editor;
using SkiaSharp;

namespace FramePFX.Core.Rendering {
    public class RenderContext {
        /// <summary>
        /// The video editor that is involved in the render process
        /// </summary>
        public VideoEditorModel Editor { get; }

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

        public RenderContext(VideoEditorModel editor, SKSurface surface, SKCanvas canvas, SKImageInfo frameInfo) {
            this.Editor = editor;
            this.Surface = surface;
            this.Canvas = canvas;
            this.FrameInfo = frameInfo;
        }
    }
}