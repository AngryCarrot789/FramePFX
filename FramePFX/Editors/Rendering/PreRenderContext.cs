using System.Numerics;
using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    public readonly struct PreRenderContext {
        /// <summary>
        /// The image info associated with the surface that will be used to do the final render
        /// </summary>
        public SKImageInfo ImageInfo { get; }

        /// <summary>
        /// A vector2 containing our <see cref="ImageInfo"/>'s width and height
        /// </summary>
        public Vector2 FrameSize => new Vector2(this.ImageInfo.Width, this.ImageInfo.Height);

        public EnumRenderQuality RenderQuality { get; }

        public SKFilterQuality FilterQuality { get; }

        public PreRenderContext(SKImageInfo imageInfo, EnumRenderQuality renderQuality) {
            this.ImageInfo = imageInfo;
            this.RenderQuality = renderQuality;
            this.FilterQuality = renderQuality.ToFilterQuality();
        }
    }
}