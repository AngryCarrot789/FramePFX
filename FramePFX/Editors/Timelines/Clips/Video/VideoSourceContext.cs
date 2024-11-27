using FramePFX.Editors.Rendering;
using SkiaSharp;

namespace FramePFX.Editors.Timelines.Clips.Video {
    /// <summary>
    /// The base class for a video source for a specific clip
    /// </summary>
    public abstract class VideoSourceContext {
        /// <summary>
        /// Gets our video source, which is what created this instance. This is non-null
        /// </summary>
        public VideoSource Source { get; }

        /// <summary>
        /// Gets the video clip associated with this context
        /// </summary>
        public VideoClip Clip { get; }

        protected VideoSourceContext(VideoSource source, VideoClip clip) {
            this.Source = source;
            this.Clip = clip;
        }

        /// <summary>
        /// Prepares this video source for rendering. This is called on the main thread, and allows rendering data
        /// to be cached locally so that it can be accessed safely by a render thread in <see cref="RenderFrame"/>.
        /// </summary>
        /// <param name="rc">The pre-render setup context</param>
        /// <param name="frame">The play head frame, relative to this clip. This will always be within range of our span</param>
        /// <returns>True if this clip can be rendered (meaning <see cref="RenderFrame"/> may be called after this call)</returns>
        public abstract bool PrepareRenderFrame(PreRenderContext rc, long frame);

        /// <summary>
        /// Renders this clip using the given rendering context data. This is called on a randomly
        /// assigned rendering thread, therefore, this method should not access un-synchronised clip data
        /// </summary>
        /// <param name="rc">The rendering context, containing things such as the surface and canvas to draw to</param>
        /// <param name="renderArea">
        /// Used as an optimisation to know where this clip actually drew data, and only that area will be used.
        /// This defaults to the destination surface's frame size (calculated via the render context's image info),
        /// meaning it is unoptimised by default
        /// </param>
        public abstract void RenderFrame(RenderContext rc, ref SKRect renderArea);
    }
}