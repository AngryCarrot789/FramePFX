namespace FramePFX.Core.Render {
    /// <summary>
    /// A viewport that can be rendered into via OpenGL, e.g. main view port, clip renderers, etc
    /// </summary>
    public interface IOGLViewPort {
        /// <summary>
        /// The actual width of the view port
        /// </summary>
        int ViewportWidth { get; set; }

        /// <summary>
        /// The actual height of the view port
        /// </summary>
        int ViewportHeight { get; set; }

        /// <summary>
        /// Whether OpenGL is ready to be rendered. When false, rendering may result in a crash
        /// </summary>
        bool IsReadyForRender { get; }

        /// <summary>
        /// The context that this view port uses
        /// </summary>
        IOGLContext Context { get; }

        /// <summary>
        /// Sets the width and height of the OpenGL view port. This may result in <see cref="IsReadyForRender"/> being false until the WPF thread sets up the view port
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        void UpdateViewportSize(int w, int h);
    }
}