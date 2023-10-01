using System;
using System.Runtime.CompilerServices;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.WPF
{
    /// <summary>
    /// A class that wraps a hidden offscreen <see cref="GameWindow"/> that can be drawn into using OpenGL
    /// </summary>
    public class OGLContextWrapper : IDisposable
    {
        private readonly GameWindow window;

        public OGLContextWrapper()
        {
            this.window = new GameWindow(1, 1,
                // 32 BBP, 8 FSAA samples (multisampling)
                new GraphicsMode(new ColorFormat(4, 4, 4, 4), 24, 0, 8, ColorFormat.Empty),
                "FramePFX Hidden Render Window",
                GameWindowFlags.FixedWindow, DisplayDevice.Default,
                1, 0, // major and minor OpenGL version
                GraphicsContextFlags.Offscreen | GraphicsContextFlags.Debug,
                null, // shared context
                true)
            {
                VSync = VSyncMode.Off,
                WindowBorder = WindowBorder.Hidden
            };
        }

        public void MakeCurrent(bool current)
        {
            if (current)
            {
                this.window.MakeCurrent();
            }
            else
            {
                this.window.Context.MakeCurrent(null);
            }
        }

        public void UpdateSize(int width, int height)
        {
            bool changed = false;
            if (this.window.Width != width)
            {
                this.window.Width = width;
                changed = true;
            }

            if (this.window.Height != height)
            {
                this.window.Height = height;
                changed = true;
            }

            if (changed)
            {
                GL.Viewport(0, 0, width, height);
            }
        }

        public void Dispose()
        {
            this.window.Dispose();
        }
    }
}