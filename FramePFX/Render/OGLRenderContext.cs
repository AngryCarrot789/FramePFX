using System;
using System.Drawing;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.RenderV2 {
    public class OGLRenderContext : IRenderContext {
        private readonly UsageCounter contextUsage;
        private volatile bool isReady;
        private CASLockType lastUsageLockType;

        public GameWindow Window { get; }

        public CASLock ContextLock { get; }

        public DispatchThread OwningThread { get; }

        public bool IsReady {
            get => this.isReady;
            set => this.isReady = value;
        }

        public OGLRenderContext(DispatchThread owningThread, GameWindow window) {
            this.OwningThread = owningThread;
            this.Window = window;
            this.ContextLock = new CASLock();
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
            GameWindow window = new GameWindow(width, height, GraphicsMode.Default, "OpenTK Hidden Render Window", GameWindowFlags.Default, DisplayDevice.Default, 1, 0, GraphicsContextFlags.Offscreen, null, true) {
                VSync = VSyncMode.Off
            };

            window.MakeCurrent();
            window.Size = new System.Drawing.Size(width, height);
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
            this.ContextLock.Lock(out var lockType);
            this.Use(true);

            // Must be called on the thread that created the window!
            // Otherwise, the application will freeze and will never recover (AFAIk)
            this.Window.Size = new Size(width, height);
            GL.Viewport(0, 0, width, height);
            OGLUtils.SetOrthoMatrix(width, height);
            this.Use(false);
            this.ContextLock.Unlock(lockType);
            this.isReady = true;
        }

        public void Use(bool use) {
            this.UseInternal(use, true);
        }

        private bool UseInternal(bool use, bool forceUseOrUnuse) {
            if (use) {
                if (this.contextUsage.Use()) {
                    if (forceUseOrUnuse || !this.Window.Context.IsCurrent) {
                        this.Window.MakeCurrent();
                        return true;
                    }
                    else {
                        return false;
                    }
                }
            }
            else if (this.contextUsage.Free()) {
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
            if (!this.isReady) {
                return false;
            }

            CASLockType type;
            if (force) {
                this.ContextLock.Lock(out type);
            }
            else if (!this.ContextLock.TryLock(out type)) {
                return false;
            }

            this.UseInternal(true, false);
            action();
            this.UseInternal(false, false);
            this.ContextLock.Unlock(type);
            return true;
        }

        public bool BeginUse(bool force = true) {
            if (!this.isReady) {
                return false;
            }

            if (force) {
                this.ContextLock.Lock(out this.lastUsageLockType);
            }
            else if (!this.ContextLock.TryLock(out this.lastUsageLockType)) {
                return false;
            }

            this.UseInternal(true, false);
            return true;
        }

        public void EndUse() {
            this.UseInternal(false, false);
            this.ContextLock.Unlock(this.lastUsageLockType);
        }

        public void Dispose() {
            this.OwningThread.InvokeAsync(this.DisposeInternal).Wait();
        }

        private void DisposeInternal() {
            this.Use(true);
            this.Window.Dispose();
        }
    }
}