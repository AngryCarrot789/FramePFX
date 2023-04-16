using System;
using System.Drawing;
using System.Runtime.InteropServices;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Render.OGL {
    public class OGLRenderContext : IRenderContext {
        private readonly UsageCounter contextUsage;
        private volatile bool isReady;
        private readonly CASLock ctxLock;

        public GameWindow Window { get; }

        public DispatchThread OwningThread { get; }

        public bool IsReady {
            get => this.isReady;
            set => this.isReady = value;
        }

        public OGLRenderContext(DispatchThread owningThread, GameWindow window) {
            this.OwningThread = owningThread;
            this.Window = window;
            this.ctxLock = new CASLock();
            this.contextUsage = new UsageCounter();
            this.isReady = true;
        }

        /// <summary>
        /// Creates a new OGL render context. The thread that called this method owns the hidden window, and whenever <see cref="UpdateViewport"/> is
        /// called, it MUST be called from that thread, otherwise the application will freeze
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static OGLRenderContext Create(DispatchThread thread, int width, int height) {
            GameWindow window = new GameWindow(width, height, GraphicsMode.Default, "OpenTK Hidden Render Window", GameWindowFlags.FixedWindow, DisplayDevice.Default, 1, 0, GraphicsContextFlags.Offscreen | GraphicsContextFlags.Debug, null, true) {
                VSync = VSyncMode.Off
            };

            window.WindowBorder = WindowBorder.Hidden;
            window.MakeCurrent();
            window.Size = new Size(width, height);
            GL.Viewport(0, 0, width, height);
            OGLUtils.SetOrthoMatrix(width, height);

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
            return new OGLRenderContext(thread, window);
        }

        public void UpdateViewport(int width, int height) {
            this.isReady = false;
            if (this.ctxLock.Lock(true)) {
                this.MakeCurrent(true);

                // Must be called on the thread that created the window!
                // Otherwise, the application will freeze and will never recover (AFAIk)
                this.Window.Size = new Size(width, height);
                GL.Viewport(0, 0, width, height);
                OGLUtils.SetOrthoMatrix(width, height);
                this.MakeCurrent(false);
                this.ctxLock.Unlock();
                this.isReady = true;
            }
        }

        public void MakeCurrent(bool use) {
            this.MakeCurrentInternal(use, true);
        }

        private bool MakeCurrentInternal(bool use, bool forceUseOrUnuse) {
            if (use) {
                if (this.contextUsage.Increment()) {
                    if (forceUseOrUnuse || !this.Window.Context.IsCurrent) {
                        this.Window.MakeCurrent();
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            else if (this.contextUsage.Decrement()) {
                if (forceUseOrUnuse || this.Window.Context.IsCurrent) {
                    this.Window.Context.MakeCurrent(null);
                    return true;
                }
                else {
                    return false;
                }
            }

            return false;
        }

        public bool UseContext(Action action, bool force = false) {
            if (this.BeginUse(force)) {
                try {
                    action();
                }
                finally {
                    this.EndUse();
                }

                return true;
            }

            return false;
        }

        public bool BeginUse(bool force = true) {
            if (!this.isReady) {
                return false;
            }

            if (this.ctxLock.Lock(force)) {
                this.MakeCurrentInternal(true, true);
                return true;
            }

            return false;
        }

        public void EndUse() {
            this.MakeCurrentInternal(false, true);
            this.ctxLock.Unlock();
        }

        public void Dispose() {
            this.OwningThread.InvokeAsync(this.DisposeInternal).Wait();
        }

        private void DisposeInternal() {
            this.Window.MakeCurrent();
            this.Window.Dispose();
        }
    }
}