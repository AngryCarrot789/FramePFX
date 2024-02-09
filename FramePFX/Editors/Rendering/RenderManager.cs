using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    public delegate void FrameRenderedEventHandler(RenderManager manager);

    /// <summary>
    /// A class that manages the audio-visual rendering for a project
    /// </summary>
    public class RenderManager {
        public Project Project { get; }

        public SKImageInfo ImageInfo { get; private set; }

        public bool IsRendering => this.isRendering != 0;

        private double averageRenderTimeMillis;
        private volatile int isRendering;
        private volatile int isRenderScheduled;
        private volatile Task scheduledRenderTask;
        private bool isDisposed;
        internal volatile int suspendRenderCount;
        private volatile int isRenderCancelled;

        private SKBitmap bitmap;
        private SKPixmap pixmap;
        // public but unsafe access to the underlying surface, used by view port. Must not be replaced externally
        public SKSurface surface;

        public Task ScheduledRenderTask => this.scheduledRenderTask;

        public double AverageRenderTimeMillis => this.averageRenderTimeMillis;

        public event FrameRenderedEventHandler FrameRendered;

        private readonly Thread renderThread;
        private volatile bool isRenderThreadRunning;

        public RenderManager(Project project) {
            this.Project = project;
            this.Project.Settings.ResolutionChanged += this.SettingsOnResolutionChanged;
            // this.renderThread = new Thread(this.RenderThreadMain);
        }

        private void SettingsOnResolutionChanged(ProjectSettings settings) {
            this.UpdateFrameInfo();
            this.InvalidateRender();
        }

        public void Dispose() {
            if (this.IsRendering)
                throw new InvalidOperationException("Cannot dispose while rendering");
            this.bitmap?.Dispose();
            this.pixmap?.Dispose();
            this.surface?.Dispose();
            this.isDisposed = true;
        }

        public void UpdateFrameInfo() {
            if (this.IsRendering) {
                throw new InvalidOperationException("Cannot change frame info while rendering");
            }

            ProjectSettings settings = this.Project.Settings;
            SKImageInfo info = new SKImageInfo(settings.Width, settings.Height, SKColorType.Bgra8888);
            if (this.ImageInfo == info) {
                return;
            }

            this.ImageInfo = info;
            this.surface?.Dispose();
            this.bitmap?.Dispose();
            this.pixmap?.Dispose();
            this.bitmap = new SKBitmap(info);

            IntPtr ptr = this.bitmap.GetAddress(0, 0);
            this.pixmap = new SKPixmap(info, ptr, info.RowBytes);
            this.surface = SKSurface.Create(this.pixmap);
        }

        private void BeginRender(Timeline timeline, long frame) {
            if (!Application.Current.Dispatcher.CheckAccess())
                throw new InvalidOperationException("Cannot start rendering while not on the main thread");
            if (timeline == null)
                throw new ArgumentNullException(nameof(timeline), "Cannot render a null timeline");
            if (frame < 0 || frame >= timeline.MaxDuration)
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame is not within the bounds of the timeline");
            if (this.ImageInfo.Width < 1 || this.ImageInfo.Height < 1)
                throw new InvalidOperationException("The current frame info is invalid");
            if (Interlocked.CompareExchange(ref this.isRendering, 1, 0) != 0)
                throw new InvalidOperationException("Render already in progress");


        }

        /// <summary>
        /// Renders the timeline at the given frame, based on the current state of the project. This is an async method;
        /// the preparation phase will have been completed by the time this method returns but the returned task will be
        /// completed when the render is completed
        /// </summary>
        /// <param name="frame">The frame to render</param>
        public async Task RenderTimelineAsync(Timeline timeline, long frame, CancellationToken token, EnumRenderQuality quality = EnumRenderQuality.UnspecifiedQuality) {
            this.BeginRender(timeline, frame);
            long beginRender;
            List<VideoTrack> trackList;
            SKImageInfo imgInfo;
            try {
                trackList = new List<VideoTrack>();
                imgInfo = this.ImageInfo;
                beginRender = Time.GetSystemTicks();

                // render bottom to top, as most video editors do
                for (int i = timeline.Tracks.Count - 1; i >= 0; i--) {
                    Track track = timeline.Tracks[i];
                    if (!(track is VideoTrack videoTrack) || !VideoTrack.VisibleParameter.GetCurrentValue(videoTrack)) {
                        continue;
                    }

                    if (videoTrack.PrepareRenderFrame(imgInfo, frame, quality)) {
                        trackList.Add(videoTrack);
                    }
                }
            }
            catch (Exception) {
                this.isRendering = 0;
                throw;
            }

            // This seems way too simple... or maybe it really is this simple but other
            // open source video editors just design their rendering system completely differently?
            // Either way, this works and it works well... for now when there are no composition clips

            await Task.Run(async () => {
                try {
                    Task[] tasks = new Task[trackList.Count];
                    for (int i = 0; i < tasks.Length; i++) {
                        VideoTrack track = trackList[i];
                        tasks[i] = Task.Run(() => track.RenderFrame(imgInfo, quality), token);
                    }

                    this.surface.Canvas.Clear(SKColors.Black);
                    for (int i = 0; i < trackList.Count; i++) {
                        if (!tasks[i].IsCompleted)
                            await tasks[i];
                        trackList[i].DrawFrameIntoSurface(this.surface);
                    }

                    this.averageRenderTimeMillis = (Time.GetSystemTicks() - beginRender) / Time.TICK_PER_MILLIS_D;
                }
                finally {
                    this.isRendering = 0;
                }

            }, token);

            this.FrameRendered?.Invoke(this);
        }

        // SaveLayer requires a temporary drawing bitmap, which can slightly
        // decrease performance, so only SaveLayer when absolutely necessary

        public static int BeginClipOpacityLayer(SKCanvas canvas, VideoClip clip, ref SKPaint paint) {
            if (clip.UsesCustomOpacityCalculation || clip.RenderOpacity >= 1.0) { // check greater than just in case...
                return canvas.Save();
            }
            else {
                return canvas.SaveLayer(paint ?? (paint = new SKPaint {
                    Color = new SKColor(255, 255, 255, clip.RenderOpacityByte)
                }));
            }
        }

        public static void EndOpacityLayer(SKCanvas canvas, int count, ref SKPaint paint) {
            canvas.RestoreToCount(count);
            if (paint != null) {
                paint.Dispose();
                paint = null;
            }
        }

        /// <summary>
        /// Schedules the timeline to be re-drawn once the application is no longer busy
        /// </summary>
        public void InvalidateRender() {
            if (this.isDisposed || this.suspendRenderCount > 0 || Interlocked.CompareExchange(ref this.isRenderScheduled, 1, 0) != 0) {
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(() => {
                this.scheduledRenderTask = this.DoScheduledRender();
            }, DispatcherPriority.Send);
        }

        private async Task DoScheduledRender() {
            if (this.suspendRenderCount < 1) {
                try {
                    await this.RenderTimelineAsync(
                        this.Project.MainTimeline,
                        this.Project.MainTimeline.PlayHeadPosition,
                        CancellationToken.None,
                        EnumRenderQuality.Low);
                }
                finally {
                    this.isRenderScheduled = 0;
                }
            }
            else {
                this.isRenderScheduled = 0;
            }
        }

        /// <summary>
        /// Draws the currently rendered frame into the given surface
        /// </summary>
        /// <param name="target">The surface in which our rendered frame is drawn into</param>
        public void Draw(SKSurface target) {
            this.surface.Flush();
            this.surface.Draw(target.Canvas, 0, 0, null);

            // SKImageInfo imgInfo = this.ImageInfo;
            // IntPtr srcPtr = this.bitmap.GetPixels();
            // IntPtr dstPtr = target.PeekPixels().GetPixels();
            // if (srcPtr != IntPtr.Zero && dstPtr != IntPtr.Zero) {
            //     unsafe {
            //         System.Runtime.CompilerServices.Unsafe.CopyBlock(dstPtr.ToPointer(), srcPtr.ToPointer(), (uint) imgInfo.BytesSize64);
            //     }
            // }

            // using (SKImage img = SKImage.FromBitmap(this.bitmap)) {
            //     target.Canvas.DrawImage(img, 0, 0, null);
            // }
        }

        private void RenderThreadMain() {
            while (this.isRenderThreadRunning) {

            }


        }

        public SuspendRender SuspendRenderInvalidation() {
            Interlocked.Increment(ref this.suspendRenderCount);
            return new SuspendRender(this);
        }
    }

    public struct SuspendRender : IDisposable {
        internal RenderManager manager;

        public SuspendRender(RenderManager manager) {
            this.manager = manager;
        }

        public void Dispose() {
            if (this.manager != null)
                Interlocked.Decrement(ref this.manager.suspendRenderCount);
            this.manager = null;
        }
    }
}