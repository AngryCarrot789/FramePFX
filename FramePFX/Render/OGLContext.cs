namespace FramePFX.Render {
    public interface OGLViewPort {
        int Width { get; set; }

        /// <summary>
        /// The actual width of this view port's height
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// Whether OpenGL is ready to be rendered. When false, rendering may result in a crash
        /// </summary>
        bool IsReady { get; }

        void UpdateSize(int w, int h);
    }
}