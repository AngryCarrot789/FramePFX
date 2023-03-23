using System;

namespace FramePFX.Core.Render {
    /// <summary>
    /// An implementation for an OpenGL context. Usually, there's only 1 of these
    /// </summary>
    public interface IOGLContext : IDisposable {
        /// <summary>
        /// Attempts to use the context. This can be called from any thread
        /// <para>
        /// When force is true, this function will spin-wait until the OpenGL Context is free to use, otherwise if the context is already in use, then false is returned
        /// </para>
        /// </summary>
        /// <param name="action">The action to invoke using this OGL context</param>
        /// <param name="force">Whether to force-take the context lock</param>
        /// <returns>True if forced or the context was not in use, otherwise false if the context was already in use and force was false</returns>
        bool UseContext(Action action, bool force = false);

        void MakeCurrent(bool valid);

        bool DrawViewportIntoBitmap(IntPtr bitmap, int w, int h, bool force = false);

        void UpdateViewportSize(int width, int height);

        /// <summary>
        /// Marks the beginning of a new render
        /// </summary>
        void BeginRender();

        /// <summary>
        /// Marks the end of a render
        /// </summary>
        void EndRender();
    }
}