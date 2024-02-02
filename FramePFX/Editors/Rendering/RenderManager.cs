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
    /// A class that manages the rendering capabilities of a project
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

        private SKBitmap bitmap;
        private SKPixmap pixmap;
        public SKSurface surface;

        public Task ScheduledRenderTask => this.scheduledRenderTask;

        public double AverageRenderTimeMillis => this.averageRenderTimeMillis;

        public event FrameRenderedEventHandler FrameRendered;

        public RenderManager(Project project) {
            this.Project = project;
        }

        public void Dispose() {
            if (this.IsRendering)
                throw new InvalidOperationException("Cannot dispose while rendering");
            this.bitmap?.Dispose();
            this.pixmap?.Dispose();
            this.surface?.Dispose();
            this.isDisposed = true;
        }

        public void UpdateFrameInfo(SKImageInfo info) {
            if (this.ImageInfo == info) {
                return;
            }

            if (this.IsRendering) {
                throw new InvalidOperationException("Cannot change frame info while rendering");
            }

            this.ImageInfo = info;
            this.bitmap?.Dispose();
            this.pixmap?.Dispose();
            this.bitmap = new SKBitmap(info);

            IntPtr ptr = this.bitmap.GetAddress(0, 0);
            this.pixmap = new SKPixmap(info, ptr, info.RowBytes);
            this.surface = SKSurface.Create(this.pixmap);
        }

        /// <summary>
        /// Renders the timeline at the given frame, based on the current state of the project. This is an async method;
        /// the preparation phase will have been completed by the time this method returns but the returned task will be
        /// completed when the render is completed
        /// </summary>
        /// <param name="frame">The frame to render</param>
        public async Task RenderTimelineAsync(Timeline timeline, long frame, EnumRenderQuality quality = EnumRenderQuality.UnspecifiedQuality) {
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

            long beginRender = Time.GetSystemTicks();
            SKImageInfo imageInfo = this.ImageInfo;
            List<VideoTrack> tracks = new List<VideoTrack>();

            // render bottom to top, as most video editors do
            for (int i = timeline.Tracks.Count - 1; i >= 0; i--) {
                Track track = timeline.Tracks[i];
                if (!(track is VideoTrack videoTrack) || !videoTrack.Visible) {
                    continue;
                }

                if (videoTrack.PrepareRenderFrame(imageInfo, frame, quality)) {
                    tracks.Add(videoTrack);
                }
            }

            // This seems way too simple... or maybe it really is this simple but other
            // open source video editors just design their rendering system completely differently?
            // Either way, this works and it works well... for now when there are no composition clips
            await Task.Run(async () => {
                Task[] tasks = new Task[tracks.Count];
                for (int i = 0; i < tracks.Count; i++) {
                    VideoTrack track = tracks[i];
                    tasks[i] = Task.Run(() => track.RenderFrame(imageInfo, quality));
                }

                this.surface.Canvas.Clear(SKColors.Black);
                for (int i = 0; i < tracks.Count; i++) {
                    if (!tasks[i].IsCompleted)
                        await tasks[i];
                    tracks[i].DrawFrameIntoSurface(this.surface);
                }
            });

            this.averageRenderTimeMillis = (Time.GetSystemTicks() - beginRender) / Time.TICK_PER_MILLIS_D;
            this.isRendering = 0;
            this.FrameRendered?.Invoke(this);
        }

        // SaveLayer requires a temporary drawing bitmap, which can slightly
        // decrease performance, so only SaveLayer when absolutely necessary
        public static int SaveLayerForOpacity(SKCanvas canvas, double opacity, ref SKPaint transparency) {
            return canvas.SaveLayer(transparency ?? (transparency = new SKPaint {
                Color = new SKColor(255, 255, 255, RenderUtils.DoubleToByte255(opacity))
            }));
        }

        public static int BeginClipOpacityLayer(SKCanvas canvas, VideoClip clip, ref SKPaint paint) {
            if (clip.UsesCustomOpacityCalculation || clip.Opacity >= 1.0) { // check greater than just in case...
                return canvas.Save();
            }
            else {
                return SaveLayerForOpacity(canvas, clip.Opacity, ref paint);
            }
        }

        public static int BeginTrackOpacityLayer(SKCanvas canvas, VideoTrack track, ref SKPaint paint) {
            // TODO: optimise this, because it adds about 3ms of extra lag per layer with an opacity less than 1 (due to bitmap allocation obviously)
            return track.Opacity >= 1.0 ? canvas.Save() : SaveLayerForOpacity(canvas, track.Opacity, ref paint);
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
                    await this.RenderTimelineAsync(this.Project.MainTimeline, this.Project.MainTimeline.PlayHeadPosition, EnumRenderQuality.Low);
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