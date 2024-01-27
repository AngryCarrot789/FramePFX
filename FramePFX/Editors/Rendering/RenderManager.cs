using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    public delegate void FrameRenderedEventHandler(RenderManager manager);

    /// <summary>
    /// A class that manages the rendering capabilities of a project
    /// </summary>
    public class RenderManager {
        public Project Project { get; }

        public SKImageInfo ImageInfo { get; private set; }

        private volatile bool isRendering;
        private volatile int isRenderScheduled;
        private volatile int isRenderScheduledDuringRender;
        private volatile Task renderTask;

        private SKBitmap bitmap;
        private SKPixmap pixmap;
        private SKSurface surface;

        public event FrameRenderedEventHandler FrameRendered;

        public RenderManager(Project project) {
            this.Project = project;
        }

        public void UpdateFrameInfo(SKImageInfo info) {
            if (this.ImageInfo == info) {
                return;
            }

            if (this.isRendering) {
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
        /// Renders the timeline at the given frame, based on the current state of the project.
        /// This is an async method; the returned task will be completed when the render is completed
        /// </summary>
        /// <param name="frame">The frame to render</param>
        private async Task RenderTimelineAsync(Timeline timeline, long frame) {
            if (timeline == null)
                throw new ArgumentNullException(nameof(timeline), "Cannot render a null timeline");
            if (frame < 0 || frame >= timeline.MaxDuration)
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame is not within the bounds of the timeline");
            if (this.isRendering)
                throw new InvalidOperationException("Render already in progress");
            if (this.ImageInfo.Width < 1 || this.ImageInfo.Height < 1)
                throw new InvalidOperationException("The current frame info is invalid");
            if (!Application.Current.Dispatcher.CheckAccess())
                throw new InvalidOperationException("Cannot start rendering while not on the main thread");

            this.isRendering = true;
            SKImageInfo imageInfo = this.ImageInfo;
            List<VideoTrack> tracks = new List<VideoTrack>();

            // render bottom to top, as most video editors do
            for (int i = timeline.Tracks.Count - 1; i >= 0; i--) {
                Track track = timeline.Tracks[i];
                if (!(track is VideoTrack videoTrack) || !videoTrack.Visible) {
                    continue;
                }

                if (videoTrack.PrepareRenderFrame(imageInfo, frame)) {
                    tracks.Add(videoTrack);
                }
            }

            await Task.Run(async () => {
                Task[] tasks = new Task[tracks.Count];
                for (int i = 0; i < tracks.Count; i++) {
                    VideoTrack track = tracks[i];
                    tasks[i] = Task.Run(() => track.RenderFrame(imageInfo));
                }

                this.surface.Canvas.Clear(SKColors.Transparent);
                await Task.WhenAll(tasks);

                // SKPaint paint = null;
                foreach (VideoTrack track in tracks) {
                    // int count = BeginTrackOpacityLayer(this.surface.Canvas, track, ref paint);
                    // int count = this.surface.Canvas.Save();
                    track.DrawFrameIntoSurface(this.surface);
                    // this.surface.Canvas.RestoreToCount(count);
                    // EndOpacityLayer(this.surface.Canvas, count, ref paint);
                }
            });

            this.isRendering = false;
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
            if (Interlocked.CompareExchange(ref this.isRenderScheduled, 1, 0) != 0) {
                return;
            }

            Application.Current.Dispatcher.InvokeAsync(this.DoRenderTimeline, DispatcherPriority.Loaded);
        }

        private void DoRenderTimeline() {
            this.renderTask = this.RenderTimelineAsync(this.Project.MainTimeline, this.Project.MainTimeline.PlayHeadPosition);
            this.renderTask.ContinueWith(x => {
                this.renderTask = null;
                this.isRenderScheduled = 0;
            });
        }

        public void Draw(SKSurface target) {
            this.surface.Flush();
            this.surface.Draw(target.Canvas, 0, 0, null);
        }
    }
}