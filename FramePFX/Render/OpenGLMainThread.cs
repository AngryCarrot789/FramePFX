using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Utils;

namespace FramePFX.Render {
    // TODO: Get rid of a ticking render and instead just render directly when needed (e.g playhead moved or progressed while playing)
    public class OpenTKRenderThread : IDisposable {
        public const double TARGET_FPS = 30d;
        public const double TARGET_FPS_MS = 1000d / TARGET_FPS;       // 60FPS = 16.666666666666
        public const double TARGET_FPS_TICKS = 10000000 / TARGET_FPS; // 60FPS = 16.666666666666
        public const double TARGET_FPS_DELTA = 1 / TARGET_FPS;        // 60FPS = 0.01666666666

        private readonly ThreadTimer thread;
        private readonly NumberAverager averager;
        private long lastTickTime;
        private readonly CASLock actionLock;
        private readonly CASLock taskLock;
        private readonly List<Action> actions;
        private readonly List<Task> tasks;
        public OGLContext oglContext;

        public volatile bool isPaused;

        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;

        public double AverageDelta => this.averager.GetAverage();

        // public LockedObject<IntPtr> Bitmap { get; set; }
        // public Action BitmapWriteCallback { get; set; }
        public CASLock BitmapLock { get; }

        public ThreadTimer Thread => this.thread;
        public static OGLContext GlobalContext { get; private set; }

        public static OpenTKRenderThread Instance { get; private set; }

        public static void Setup(int width, int height) {
            Instance = new OpenTKRenderThread() {
                Width = width,
                Height = height
            };
        }

        public OpenTKRenderThread() {
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
            if (width <= 0 || height <= 0 || this.oglContext == null)
                return;

            this.oglContext.UpdateViewportSize(width, height);
        }

        private void OnThreadStarted() {
            this.oglContext = OGLContext.Create(this.Width, this.Height);
            this.lastTickTime = GetCurrentTime();
            GlobalContext = this.oglContext;
            IoC.VideoEditor?.ActiveProject?.Timeline?.ScheduleRender(false);
        }

        private void OnGLThreadTick() {
            if (this.isPaused) {
                return;
            }

            // calc interval time
            long time = Time.GetSystemTicks();
            this.averager.PushValue((double) (time - this.lastTickTime) / Time.TICK_PER_SECOND);
            this.lastTickTime = time;

            this.HandleCallbacks();

            System.Threading.Thread.Sleep(100);

            // if (!TimelineViewModel.Instance?.IsRenderDirty ?? false) {
            //     return;
            // }
            // this.oglContext.UseContext(() => {
            //     this.RenderHandler.RenderGLThread();
            //     TimelineViewModel.Instance.IsRenderDirty = false;
            //     this.RenderHandler.Tick(delta);
            // }, true);
        }

        private void OnThreadStopped() {
            this.oglContext.Dispose();
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

        public bool DrawViewportIntoBitmap(IntPtr bitmap, int w, int h, bool force = false) {
            return this.oglContext.DrawViewportIntoBitmap(bitmap, w, h, force);
        }

        public void Dispose() {
            if (this.thread.IsRunning) {
                this.thread.Stop();
            }

            this.oglContext.Dispose();
        }
    }
}