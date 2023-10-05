using System;

namespace FramePFX.WPF {
    /// <summary>
    /// Contains information about a draw event. Drawing is always using Pbgra32 (pre-multiplied BGRA, 8 bits per channel), meaning
    /// the RBG components are multiplied by the alpha before being written to the <see cref="BackBuffer"/>
    /// </summary>
    public readonly struct DrawEventArgs {
        public readonly IntPtr BackBuffer;
        public readonly int Width;
        public readonly int Height;
        public readonly double ScaleX;
        public readonly double ScaleY;

        public int Stride => this.Width * 4;

        public DrawEventArgs(IntPtr backBuffer, int width, int height, double scaleX, double scaleY) {
            this.BackBuffer = backBuffer;
            this.Width = width;
            this.Height = height;
            this.ScaleX = scaleX;
            this.ScaleY = scaleY;
        }
    }
}