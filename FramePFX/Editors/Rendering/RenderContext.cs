using System.Numerics;
using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    /// <summary>
    /// The rendering context used to render a clip, and may also be used by effects to access the back buffer
    /// </summary>
    public class RenderContext {
        /// <summary>
        /// The image info associated with our <see cref="Surface"/>
        /// </summary>
        public SKImageInfo ImageInfo { get; }

        /// <summary>
        /// The bitmap that stores the pixels
        /// </summary>
        public SKBitmap Bitmap { get; }

        /// <summary>
        /// A pixmap, used by our <see cref="Surface"/>, which references the <see cref="Bitmap"/>
        /// </summary>
        public SKPixmap Pixmap { get; }

        /// <summary>
        /// The surface used to draw things
        /// </summary>
        public SKSurface Surface { get; }

        /// <summary>
        /// Our <see cref="Surface"/>'s canvas
        /// </summary>
        public SKCanvas Canvas { get; }

        /// <summary>
        /// A vector2 containing our <see cref="ImageInfo"/>'s width and height
        /// </summary>
        public Vector2 FrameSize => new Vector2(this.ImageInfo.Width, this.ImageInfo.Height);

        /// <summary>
        /// Returns a rect that contains the effective rendering area, based on the canvas'
        /// <see cref="SKCanvas.TotalMatrix"/> and the size of our <see cref="ImageInfo"/>.
        /// <para>
        /// Pixels could exceed this area of course, however they really shouldn't since they
        /// would basically be drawing offscreen
        /// </para>
        /// </summary>
        public SKRect RenderArea => this.TranslateRect(new SKRect(0, 0, this.ImageInfo.Width, this.ImageInfo.Height));

        public EnumRenderQuality Quality { get; }

        public SKFilterQuality FilterQuality { get; }

        public RenderContext(SKImageInfo imageInfo, SKSurface surface, SKBitmap bitmap, SKPixmap pixmap, EnumRenderQuality quality = EnumRenderQuality.UnspecifiedQuality) {
            this.ImageInfo = imageInfo;
            this.Surface = surface;
            this.Canvas = surface.Canvas;
            this.Bitmap = bitmap;
            this.Pixmap = pixmap;
            this.Quality = quality;
            this.FilterQuality = quality.ToFilterQuality();
        }

        /// <summary>
        /// Translates the given rectangle based on our canvas' <see cref="SKCanvas.TotalMatrix"/>
        /// </summary>
        /// <param name="inputRect">The input rect to be translated</param>
        /// <returns>The final rect which represents the effective drawing area</returns>
        public SKRect TranslateRect(SKRect inputRect) {
            return this.Canvas.TotalMatrix.MapRect(inputRect);
        }
    }
}