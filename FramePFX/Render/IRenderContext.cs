using System;

namespace FramePFX.Render {
    /// <summary>
    /// A context for one or more viewports. This defines behaviour for using the context
    /// </summary>
    public interface IRenderContext {
        /// <summary>
        /// Whether this context is valid and ready to be used
        /// </summary>
        bool IsReady { get; }

        /// <summary>
        /// The thread that created this context
        /// </summary>
        DispatchThread OwningThread { get; }

        /// <summary>
        /// Updates this context for the new viewport size
        /// </summary>
        /// <param name="width">The new width</param>
        /// <param name="height">The new height</param>
        void UpdateViewport(int width, int height);

        /// <summary>
        /// Makes this context the global active context for the rendering engine. This must be called with care, because
        /// another thread may already be using this context. See <see cref="BeginUse"/> and <see cref="EndUse"/> or <see cref="UseContext"/>
        /// for a safer option
        /// </summary>
        /// <param name="use">Whether to use this context, or set the global context to none</param>
        void Use(bool use);

        /// <summary>
        /// Attempts to use this context. Returns false if not ready or already in use on another thread
        /// </summary>
        /// <param name="action">An action to call if the context was used</param>
        /// <param name="force">Whether to force use the context. Can result in a deadlock when true if used incorrectly</param>
        /// <returns></returns>
        bool UseContext(Action action, bool force = false);

        /// <summary>
        /// Attempts to use this context. Returns false if not ready or already in use on another thread
        /// </summary>
        /// <param name="force">Whether to force use the context. Can result in a deadlock when true if used incorrectly</param>
        /// <returns></returns>
        bool BeginUse(bool force);

        /// <summary>
        /// Ends the context usage, allowing it to be used by other threads
        /// </summary>
        void EndUse();
    }
}