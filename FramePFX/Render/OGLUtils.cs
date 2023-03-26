using System;
using System.Threading;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.RenderV2 {
    public static class OGLUtils {
        public static DispatchThread OGLThread { get; private set; }

        public static OGLRenderContext GlobalContext { get; private set; }

        public delegate void GLThreadTickEventArgs();

        public static event GLThreadTickEventArgs OnGLThreadTick;

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
            public OGLThreadImpl() {

            }

            protected override void OnThreadStart() {
                GlobalContext = OGLRenderContext.Create(this, 1, 1);
            }

            protected override void OnThreadTick() {
                OnGLThreadTick?.Invoke();
                Thread.Sleep(100);
            }

            protected override void OnThreadStop() {
                if (GlobalContext != null) {
                    GlobalContext.Dispose();
                    GlobalContext = null;
                }
            }
        }
    }
}