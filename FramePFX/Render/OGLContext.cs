using System;
using System.Drawing;
using FramePFX.Core.Render;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Render {
    public class OGLContext : IDisposable, IOGLContext {
        private readonly UsageCounter contextUsageCounter = new UsageCounter();
        private volatile FrameBuffer framebuffer;
        private volatile bool isReady;
        private CASLockType lastRenderLockType;

        public GameWindow Window { get; }
        public IGraphicsContext Context { get; }

        public FrameBuffer Framebuffer {
            get => this.framebuffer;
        }

        public bool IsReady {
            get => this.isReady;
        }

        public CASLock ContextLock { get; }

        public OGLContext(GameWindow window, FrameBuffer framebuffer) {
            this.Window = window;
            this.Context = window.Context;
            this.framebuffer = framebuffer;
            this.ContextLock = new CASLock();
        }

        /// <summary>
        /// Creates a new hidden OpenTK window with the given width and height
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static OGLContext Create(int width, int height) {
            GameWindow window = new GameWindow(width, height, GraphicsMode.Default, "OpenTK Hidden Render Window", GameWindowFlags.Default, DisplayDevice.Default, 1, 0, GraphicsContextFlags.Offscreen, null, true) {
                VSync = VSyncMode.Off
            };

            window.MakeCurrent();
            window.Size = new Size(width, height);
            FrameBuffer buffer = FrameBuffer.Create(width, height);

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1d, 1d);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Fix OpenGL flipped image
            // GL.Rotate(180, 0, 0, 1);
            // GL.Scale(-1f, 1f, 1f);
            // GL.Ortho(0, ViewportScaleX, ViewportScaleY, 0, -1d, 1d);

            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);
            window.Context.MakeCurrent(null);
            return new OGLContext(window, buffer);
        }

        public void UpdateViewportSize(int width, int height) {
            this.ContextLock.Lock(out var lockType);
            this.MakeCurrent(true);
            if (this.framebuffer != null && !this.framebuffer.IsDisposed) {
                this.framebuffer.Dispose();
            }

            this.Window.Size = new Size(width, height);
            this.framebuffer = FrameBuffer.Create(width, height);

            GL.Viewport(0, 0, width, height);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1d, 1d);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            this.MakeCurrent(false);
            this.ContextLock.Unlock(lockType);
        }

        public void BeginRender() {
            this.ContextLock.Lock(out this.lastRenderLockType);
            this.MakeCurrent(true);
            this.framebuffer.Use();
        }

        public void EndRender() {
            this.MakeCurrent(false);
            this.ContextLock.Unlock(this.lastRenderLockType);
        }

        public bool UseContext(Action action, bool force = false) {
            CASLockType type;
            if (force) {
                this.ContextLock.Lock(out type);
            }
            else if (!this.ContextLock.TryLock(out type)) {
                return false;
            }

            this.MakeCurrent(true);
            action();
            this.MakeCurrent(false);
            this.ContextLock.Unlock(type);
            return true;
        }

        public void MakeCurrent(bool valid) {
            if (valid) {
                if (this.contextUsageCounter.Use()) {
                    this.Window.MakeCurrent();
                }
            }
            else if (this.contextUsageCounter.Free()) {
                this.Window.Context.MakeCurrent(null);
            }
        }

        public bool DrawViewportIntoBitmap(IntPtr bitmap, int w, int h, bool force = false) {
            return this.UseContext(() => {
                GL.ReadBuffer(ReadBufferMode.Back);
                GL.ReadPixels(0, 0, w, h, PixelFormat.Rgb, PixelType.UnsignedByte, bitmap);
            }, force);
        }

        public void Dispose() {
            this.ContextLock.Lock(out CASLockType type);
            this.MakeCurrent(true);
            this.framebuffer?.Dispose();
            this.Window?.Dispose();
            this.ContextLock.Unlock(type);
        }
    }
}