using System;
using System.Threading;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Render {
    public static class OGLUtils {
        public static DispatchThread OGLThread { get; private set; }

        public static OGLRenderContext GlobalContext { get; private set; }

        public delegate void GLThreadTickEventArgs();

        public static event GLThreadTickEventArgs OnGLThreadTick;

        private static volatile bool isInTickMode;

        public static bool IsGLThreadInTickMode {
            get => isInTickMode;
            set => isInTickMode = value;
        }

        public static void SetupOGLThread() {
            OGLThread = new OGLThreadImpl();
            OGLThread.Start();
        }

        public static void ShutdownMainThread() {
            OGLThread?.Stop(true);
        }

        public static void WaitForContextCompletion() {
            int count = 0;
            while (GlobalContext == null) {
                Thread.Sleep(50);
                if ((count += 50) >= 3000) {
                    throw new Exception("GL context was not created within 3 seconds");
                }
            }
        }

        public static void SetOrthoMatrix(int width, int height) {
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1d, 1d);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        private class OGLThreadImpl : DispatchThread {
            private readonly EventWaitHandle handle;

            public OGLThreadImpl() {
                this.handle = new AutoResetEvent(true);
            }

            public void WakeThread() {
                this.handle.Set();
            }

            protected override void OnThreadStart() {
                GlobalContext = OGLRenderContext.Create(this, 1, 1);
            }

            protected override void OnThreadTick() {
                if (isInTickMode) {
                    OnGLThreadTick?.Invoke();
                    Thread.Sleep(10);
                }
                else {
                    Thread.Sleep(100);
                    // this.handle.WaitOne();
                }
            }

            protected override void OnThreadStop() {
                if (GlobalContext != null) {
                    GlobalContext.Dispose();
                    GlobalContext = null;
                }
            }

            protected override void OnActionEnqueued(Action action, bool invokeLater) {
                this.WakeThread();
            }

            protected override void OnAsyncActionEnqueued(Action action, bool invokeLater) {
                this.WakeThread();
            }
        }
    }
}