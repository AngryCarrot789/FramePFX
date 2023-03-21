using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using FramePFX.Utils;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FramePFX.Render {
    public class TKRenderThread : IDisposable {
        public const double TARGET_FPS = 30d;
        public const double TARGET_FPS_MS = 1000d / TARGET_FPS;       // 60FPS = 16.666666666666
        public const double TARGET_FPS_TICKS = 10000000 / TARGET_FPS; // 60FPS = 16.666666666666
        public const double TARGET_FPS_DELTA = 1 / TARGET_FPS;        // 60FPS = 0.01666666666

        private readonly ThreadTimer thread;
        private readonly NumberAverager averager;
        private long lastTickTime;

        private GameWindow window;
        private IGraphicsContext context;
        private Framebuffer framebuffer;
        private readonly CASLock contextLock;

        private volatile bool isGLReady;
        public bool IsGLEnabled => this.isGLReady;

        private readonly CASLock actionLock;
        private readonly CASLock taskLock;
        private readonly List<Action> actions;
        private readonly List<Task> tasks;

        public volatile bool isPaused;

        public int Width { get; set; } = 1;

        public int Height { get; set; } = 1;

        public IRenderHandler RenderHandler { get; set; }

        public double AverageDelta => this.averager.GetAverage();

        // public LockedObject<IntPtr> Bitmap { get; set; }
        // public Action BitmapWriteCallback { get; set; }
        public CASLock BitmapLock { get; }

        public ThreadTimer Thread => this.thread;

        public TKRenderThread() {
            this.thread = new ThreadTimer(TimeSpan.FromMilliseconds(TARGET_FPS_MS)) {
                StartedAction = this.OnThreadStarted,
                StoppedAction = this.OnThreadStopped,
                TickAction = this.OnGLThreadTick,
                ThreadName = "GL I/O Element Render Thread"
            };

            this.averager = new NumberAverager(10);
            this.actions = new List<Action>();
            this.tasks = new List<Task>();
            this.actionLock = new CASLock();
            this.taskLock = new CASLock();
            this.contextLock = new CASLock();
            this.BitmapLock = new CASLock();
        }

        public void Start() {
            this.thread.Start();
            this.isPaused = false;
        }

        public void Pause() {
            this.isPaused = true;
        }

        public void StopAndDispose() {
            this.isPaused = true;
            this.thread.Stop();
        }

        public void Invoke(Action action) {
            // The tick thread may call Invoke() or InvokeAsync(), so
            // it's still a good idea to take into account the lock type
            this.actionLock.Lock(out CASLockType type);
            this.actions.Add(action);
            this.actionLock.Unlock(type);
        }

        public Task InvokeAsync(Action action) {
            Task task = new Task(action);
            this.taskLock.Lock(out CASLockType type);
            this.tasks.Add(task);
            this.taskLock.Unlock(type);
            return task;
        }

        public void UpdateViewportSie(int width, int height) {
            if (width <= 0 || height <= 0 || this.window == null)
                return;

            this.isGLReady = false;
            this.contextLock.Lock(out CASLockType type);
            this.MakeContextCurrent(true);
            if (this.framebuffer != null && !this.framebuffer.IsDisposed) {
                this.framebuffer.Dispose();
            }

            this.window.Size = new Size(width, height);
            this.framebuffer = Framebuffer.Create(width, height);
            GL.Viewport(0, 0, width, height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, width, 0, height, -1d, 1d);
            // Fix OpenGL flipped image
            // GL.Rotate(180, 0, 0, 1);
            // GL.Scale(-1f, 1f, 1f);
            // GL.Ortho(0, ViewportScaleX, ViewportScaleY, 0, -1d, 1d);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            this.MakeContextCurrent(false);
            this.contextLock.Unlock(type);
            this.isGLReady = true;
        }

        private void OnThreadStarted() {
            this.isGLReady = false;
            this.window = new GameWindow(this.Width, this.Height, GraphicsMode.Default, "OpenTK Hidden Render Window", GameWindowFlags.Default, DisplayDevice.Default, 1, 0, GraphicsContextFlags.Offscreen, null, true) {
                VSync = VSyncMode.Off
            };

            this.context = this.window.Context;
            this.contextLock.Lock(out CASLockType type);
            this.MakeContextCurrent(true);

            // GL.BlendFunc(BlendingFactor.DstColor, BlendingFactor.OneMinusSrcAlpha);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthFunc(DepthFunction.Less);
            GL.DepthMask(true);

            if (this.framebuffer != null && !this.framebuffer.IsDisposed) {
                this.framebuffer.Dispose();
            }

            this.window.Size = new Size(this.Width, this.Height);
            this.framebuffer = Framebuffer.Create(this.Width, this.Height);
            GL.Viewport(0, 0, this.Width, this.Height);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, this.Width, 0, this.Height, -1d, 1d);
            // Fix OpenGL flipped image
            // GL.Rotate(180, 0, 0, 1);
            // GL.Scale(-1f, 1f, 1f);
            // GL.Ortho(0, ViewportScaleX, ViewportScaleY, 0, -1d, 1d);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            this.MakeContextCurrent(false);
            this.contextLock.Unlock(type);

            this.RenderHandler.Setup();
            this.lastTickTime = GetCurrentTime();
            this.isGLReady = true;
        }

        private void OnGLThreadTick() {
            if (this.isPaused) {
                return;
            }

            // calc interval time
            long time = Time.GetSystemTicks();
            double delta = (double) (time - this.lastTickTime) / Time.TICK_PER_SECOND;
            this.averager.PushValue(delta);
            this.lastTickTime = time;

            this.HandleCallbacks();

            this.contextLock.Lock(out _);
            this.MakeContextCurrent(true);
            this.framebuffer.Use();

            this.RenderHandler.RenderGLThread();
            this.RenderHandler.Tick(delta);

            // IntPtr ptr;
            // if (this.Bitmap != null && (ptr = this.Bitmap.Value) != IntPtr.Zero) {
            //     this.Bitmap.Lock(out CASLockType lockType);
            //     GL.ReadBuffer(ReadBufferMode.Back);
            //     GL.ReadPixels(0, 0, this.framebuffer.width, this.framebuffer.height, PixelFormat.Rgb, PixelType.UnsignedByte, ptr);
            //     this.Bitmap.Unlock(lockType);
            //     this.BitmapWriteCallback?.Invoke();
            // }

            this.MakeContextCurrent(false);
            this.contextLock.Unlock();
        }

        private void OnThreadStopped() {
            this.contextLock.Lock(out CASLockType type);
            this.isGLReady = false;
            this.MakeContextCurrent(true);

            if (this.framebuffer != null && !this.framebuffer.IsDisposed)
                this.framebuffer.Dispose();

            this.MakeContextCurrent(false);
            this.contextLock.Unlock(type);
        }

        private void HandleCallbacks() {
            if (this.actionLock.TryLock(out CASLockType actionLockType)) {
                foreach (Action action in this.actions)
                    action();
                this.actions.Clear();
                this.actionLock.Unlock(actionLockType);
            }

            if (this.taskLock.TryLock(out CASLockType taskLockType)) {
                foreach (Task action in this.tasks)
                    action.RunSynchronously();
                this.tasks.Clear();
                this.taskLock.Unlock(taskLockType);
            }
        }

        private static long GetCurrentTime() {
            return Time.GetSystemTicks();
        }

        private void MakeContextCurrent(bool valid) {
            if (valid) {
                this.window.MakeCurrent();
            }
            else {
                this.context.MakeCurrent(null);
            }
        }

        public bool DrawViewportIntoBitmap(IntPtr bitmap, int w, int h) {
            if (this.contextLock.TryLock(out CASLockType ctxLockType)) {
                this.MakeContextCurrent(true);
                GL.ReadBuffer(ReadBufferMode.Back);
                GL.ReadPixels(0, 0, w, h, PixelFormat.Rgb, PixelType.UnsignedByte, bitmap);
                if (ctxLockType != CASLockType.Thread) {
                    this.MakeContextCurrent(false);
                    this.contextLock.Unlock(ctxLockType);
                }

                return true;
            }

            return false;
        }

        public void Dispose() {
            if (this.thread.IsRunning) {
                this.thread.Stop();
            }

            this.framebuffer?.Dispose();
            this.window?.Dispose();
        }
    }
}