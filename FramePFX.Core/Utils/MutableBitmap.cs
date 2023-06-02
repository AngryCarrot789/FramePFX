using System;

namespace FramePFX.Core.Utils {
    public interface MutableBitmap {
        int Width { get; }
        int Height { get; }
        int Stride { get; }
        int BitsPerPixel { get; }

        /// <summary>
        /// Locks the back buffer, allowing you to edit it
        /// </summary>
        /// <returns></returns>
        IntPtr GetBackBuffer();

        /// <summary>
        /// Releases the back buffer
        /// </summary>
        void ReleaseBackBuffer();
    }
}