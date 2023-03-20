namespace FramePFX.Render {
    public interface OGLContext {
        /// <summary>
        /// The actual width of the view port
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// The actual height of the view port
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Whether OpenGL is ready to be rendered. When false, rendering may result in a crash
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// Sets the width and height of the OpenGL view port. This may result in <see cref="IsReady"/> being false until the WPF thread sets up the view port
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        void UpdateSize(int w, int h);
    }
}