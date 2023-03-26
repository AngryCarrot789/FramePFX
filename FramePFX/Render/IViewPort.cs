namespace FramePFX.Render {
    /// <summary>
    /// An interface that defines a view port that things can be rendered into
    /// </summary>
    public interface IViewPort {
        /// <summary>
        /// The width of this viewport
        /// </summary>
        int Width { get; }

        /// <summary>
        /// The height of this viewport
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Whether this view port is ready to be rendered into or not. When false, rendering may result in a crash
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// The context that this view port uses to render
        /// </summary>
        IRenderContext Context { get; }

        /// <summary>
        /// Sets the size of this view port
        /// </summary>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        void SetSize(int width, int height);

        /// <summary>
        /// Attempts to begin a render phase. Returns false if not ready or a render could not be started
        /// </summary>
        /// <param name="force"></param>
        /// <returns></returns>
        bool BeginRender(bool force = false);

        /// <summary>
        /// Ends the current render phase
        /// </summary>
        void EndRender();

        /// <summary>
        /// Flushes the current OGL context frame to this viewport. This is not called by <see cref="EndRender"/>, so
        /// it must be called manually in order for WPF to be able to draw the view port
        /// </summary>
        void FlushFrame();
    }
}