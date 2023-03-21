using System;
using System.Windows.Media.Imaging;

namespace FramePFX.Render {
    public interface OGLContext {
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
        bool IsOGLReady { get; }

        WriteableBitmap CurrentFrame { get; }

        IntPtr CurrentFramePtr { get; }

        /// <summary>
        /// Sets the width and height of the OpenGL view port. This may result in <see cref="IsOGLReady"/> being false until the WPF thread sets up the view port
        /// </summary>
        /// <param name="w"></param>
        /// <param name="h"></param>
        void UpdateViewport(int w, int h);
    }
}