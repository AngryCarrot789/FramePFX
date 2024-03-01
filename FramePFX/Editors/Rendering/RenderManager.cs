//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Editors.Utils;
using FramePFX.Utils;
using SkiaSharp;

namespace FramePFX.Editors.Rendering {
    public delegate void FrameRenderedEventHandler(RenderManager manager);

    /// <summary>
    /// A class that manages the audio-visual rendering for a timeline
    /// </summary>
    public class RenderManager {
        public Timeline Timeline { get; }

        public SKImageInfo ImageInfo { get; private set; }

        public bool IsRendering => this.isRendering != 0;

        private double averageVideoRenderTimeMillis;
        private double averageAudioRenderTimeMillis;
        private double accumulatedSamples;
        private volatile int isRendering;
        private volatile int isRenderScheduled;
        private volatile Task lastRenderTask;
        private bool isDisposed;
        internal volatile int suspendRenderCount;
        private volatile int isRenderCancelled;

        private volatile bool isSkiaValid;
        private SKBitmap bitmap;

        private SKPixmap pixmap;

        // public but unsafe access to the underlying surface, used by view port. Must not be replaced externally
        public SKSurface surface;

        public Task LastRenderTask {
            get => this.lastRenderTask;
            set => this.lastRenderTask = value;
        }

        /// <summary>
        /// Gets the rect containing the bounds of the pixels that were modified during the last render
        /// </summary>
        public SKRect LastRenderRect;

        public AudioRingBuffer audioRingBuffer;

        public double AverageVideoRenderTimeMillis => this.averageVideoRenderTimeMillis;
        public double AverageAudioRenderTimeMillis => this.averageAudioRenderTimeMillis;

        public event FrameRenderedEventHandler FrameRendered;

        public RenderManager(Timeline timeline) {
            this.Timeline = timeline;
            // this.renderThread = new Thread(this.RenderThreadMain);
        }

        public SuspendRender CancelRenderAndWaitForCompletion() {
            SuspendRender suspension = this.SuspendRenderInvalidation();
            this.isRenderCancelled = 1;
            while (this.isRendering != 0)
                Thread.Sleep(1);
            return suspension;
        }

        public static void InternalOnTimelineProjectChanged(RenderManager manager, Project oldProject, Project newProject) {
            if (oldProject != null) {
                oldProject.Settings.ResolutionChanged -= manager.SettingsOnResolutionChanged;
                manager.DisposeCanvas();
                manager.audioRingBuffer?.Dispose();
            }

            if (newProject != null) {
                newProject.Settings.ResolutionChanged += manager.SettingsOnResolutionChanged;
                manager.SettingsOnResolutionChanged(newProject.Settings);
                manager.audioRingBuffer?.Dispose();
                manager.audioRingBuffer = new AudioRingBuffer(4096);
            }
        }

        private void SettingsOnResolutionChanged(ProjectSettings settings) {
            this.UpdateFrameInfo(settings);
            this.InvalidateRender();
        }

        public void Dispose() {
            this.DisposeCanvas();
            this.isDisposed = true;
        }

        private void DisposeCanvas() {
            using (this.CancelRenderAndWaitForCompletion()) {
                this.DisposeSkiaObjects();
            }
        }

        private void DisposeSkiaObjects() {
            this.isSkiaValid = false;
            this.bitmap?.Dispose();
            this.pixmap?.Dispose();
            this.surface?.Dispose();
        }

        public void UpdateFrameInfo(ProjectSettings settings) {
            using (this.CancelRenderAndWaitForCompletion()) {
                SKImageInfo info = new SKImageInfo(settings.Width, settings.Height, SKColorType.Bgra8888);
                if (this.ImageInfo == info && this.isSkiaValid) {
                    return;
                }

                this.DisposeSkiaObjects();
                this.ImageInfo = info;
                this.bitmap = new SKBitmap(info);

                IntPtr ptr = this.bitmap.GetAddress(0, 0);
                this.pixmap = new SKPixmap(info, ptr, info.RowBytes);
                this.surface = SKSurface.Create(this.pixmap);
                this.isSkiaValid = true;
            }
        }

        private void BeginRender(long frame) {
            if (!IoC.Dispatcher.IsOnOwnerThread)
                throw new InvalidOperationException("Cannot start rendering while not on the main thread");
            if (frame < 0 || frame >= this.Timeline.MaxDuration)
                throw new ArgumentOutOfRangeException(nameof(frame), "Frame is not within the bounds of the timeline");
            if (this.ImageInfo.Width < 1 || this.ImageInfo.Height < 1)
                throw new InvalidOperationException("The current frame info is invalid");
            if (Interlocked.CompareExchange(ref this.isRendering, 1, 0) != 0)
                throw new InvalidOperationException("Render already in progress");
            // possible race condition... maybe guard lock swapping isRendering with 1, instead of using CAS?
            this.isRenderCancelled = 0;
        }

        /// <summary>
        /// Renders the timeline at the given frame, based on the current state of the project. This is an async method;
        /// the preparation phase will have been completed by the time this method returns but the returned task will be
        /// completed when the render is completed
        /// </summary>
        /// <param name="frame">The frame to render</param>
        public Task RenderTimelineAsync(long frame, CancellationToken token, EnumRenderQuality quality = EnumRenderQuality.UnspecifiedQuality) {
            Project project = this.Timeline.Project;
            if (project == null) {
                return Task.CompletedTask;
            }

            this.BeginRender(frame);
            long beginRender;
            long samples;
            List<VideoTrack> videoTrackList;
            List<AudioTrack> audioTrackList;
            SKImageInfo imgInfo;

            try {
                videoTrackList = new List<VideoTrack>();
                audioTrackList = new List<AudioTrack>();
                imgInfo = this.ImageInfo;
                beginRender = Time.GetSystemTicks();

                double fps = project.Settings.FrameRate.AsDouble;
                double sampleDouble = Math.Ceiling(44100.0 / fps) + this.accumulatedSamples;
                // ensure value is even.
                // 44100/60fps == 735, meaning that last sample for the right channel
                // wouldn't get generated, and the next render would write the first
                // left sample into the previous render's right channel :P
                samples = (long) Maths.Ceil(sampleDouble, 2);
                this.accumulatedSamples = (sampleDouble - samples);

                // samples = project.Settings.BufferSize;

                // render bottom to top, as most video editors do
                for (int i = this.Timeline.Tracks.Count - 1; i >= 0; i--) {
                    Track track = this.Timeline.Tracks[i];
                    if (track is VideoTrack videoTrack && VideoTrack.VisibleParameter.GetCurrentValue(videoTrack)) {
                        if (videoTrack.PrepareRenderFrame(imgInfo, frame, quality)) {
                            videoTrackList.Add(videoTrack);
                        }
                    }

                    if (track is AudioTrack audioTrack) {
                        if (audioTrack.PrepareRenderFrame(frame, samples, quality)) {
                            audioTrackList.Add(audioTrack);
                        }
                    }
                }
            }
            catch {
                this.isRendering = 0;
                throw;
            }

            bool isFirstRenderRect = true;
            SKRect totalRenderArea = default;
            Task renderVideo = Task.Run(async () => {
                this.CheckRenderCancelled();
                Task[] tasks = new Task[videoTrackList.Count];
                for (int i = 0; i < tasks.Length; i++) {
                    VideoTrack track = videoTrackList[i];
                    tasks[i] = Task.Run(() => track.RenderVideoFrame(imgInfo, quality), token);
                }

                this.CheckRenderCancelled();
                this.surface.Canvas.Clear(SKColors.Black);
                for (int i = 0; i < tasks.Length; i++) {
                    if (!tasks[i].IsCompleted)
                        await tasks[i];
                    videoTrackList[i].DrawFrameIntoSurface(this.surface, out SKRect usedRenderingArea);
                    if (isFirstRenderRect) {
                        totalRenderArea = usedRenderingArea;
                        isFirstRenderRect = false;
                    }
                    else {
                        totalRenderArea = SKRect.Union(totalRenderArea, usedRenderingArea);
                    }
                }

                this.averageVideoRenderTimeMillis = (Time.GetSystemTicks() - beginRender) / Time.TICK_PER_MILLIS_D;
            }, token);

            Task renderAudio = Task.Run(async () => {
                this.CheckRenderCancelled();

                Task[] tasks = new Task[audioTrackList.Count];
                for (int i = 0; i < tasks.Length; i++) {
                    AudioTrack track = audioTrackList[i];
                    tasks[i] = Task.Run(() => track.RenderAudioFrame(samples, quality), token);
                }

                this.CheckRenderCancelled();

                for (int i = 0; i < tasks.Length; i++) {
                    if (!tasks[i].IsCompleted)
                        await tasks[i];
                    lock (this.audioRingBuffer) {
                        audioTrackList[i].WriteSamples(this.audioRingBuffer, samples * 2);
                    }
                }

                this.averageAudioRenderTimeMillis = (Time.GetSystemTicks() - beginRender) / Time.TICK_PER_MILLIS_D;
            }, token);

            return Task.WhenAll(renderVideo, renderAudio).ContinueWith(t => {
                this.LastRenderRect = totalRenderArea;
                this.isRendering = 0;
            }, TaskContinuationOptions.ExecuteSynchronously);
        }

        private void CheckRenderCancelled() {
            if (this.isRenderCancelled != 0 || !this.isSkiaValid)
                throw new TaskCanceledException();
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
            if (this.isDisposed || this.suspendRenderCount > 0) {
                return;
            }

            if (!this.Timeline.IsActive) {
                // if (this.Timeline is CompositionTimeline composition)
                //     composition.TryNotifyParentTimelineRenderInvalidated();
                return;
            }

            if (Interlocked.CompareExchange(ref this.isRenderScheduled, 1, 0) != 0) {
                return;
            }

            IoC.Dispatcher.InvokeAsync(() => {
                this.lastRenderTask = this.DoScheduledRender();
            }, DispatcherPriority.Send);
        }


        /// <summary>
        /// Raises the <see cref="FrameRendered"/> event
        /// </summary>
        public void OnFrameCompleted() => this.FrameRendered?.Invoke(this);

        private async Task DoScheduledRender() {
            if (this.suspendRenderCount < 1) {
                try {
                    await this.RenderTimelineAsync(this.Timeline.PlayHeadPosition, CancellationToken.None, EnumRenderQuality.Low);
                }
                catch (TaskCanceledException) {
                }
                catch (OperationCanceledException) {
                }
                catch (Exception e) {
                    if (this.Timeline?.Project?.Editor is VideoEditor editor && editor.Playback.PlayState == PlayState.Play) {
                        editor.Playback.Pause();
                    }

                    Debug.WriteLine(e.GetToString());
                }
                finally {
                    this.isRenderScheduled = 0;
                }

                this.OnFrameCompleted();
            }
            else {
                this.isRenderScheduled = 0;
            }
        }

        /// <summary>
        /// Draws the currently rendered frame into the given surface
        /// </summary>
        /// <param name="target">The surface in which our rendered frame is drawn into</param>
        /// <param name="paint">The paint used for drawing, e.g., for adding opacity</param>
        public void Draw(SKSurface target, SKPaint paint = null) {
            SKRect usedArea = this.LastRenderRect;
            if (!(usedArea.Width > 0) || !(usedArea.Height > 0)) {
                return;
            }

            this.surface.Flush();

            // this is the same code that VideoTrack uses for efficient final frame assembly
            SKRect frameRect = this.ImageInfo.ToRect();
            if (usedArea == frameRect) {
                this.surface.Draw(target.Canvas, 0, 0, paint);
            }
            else {
                using (SKImage img = SKImage.FromPixels(this.ImageInfo, this.bitmap.GetPixels())) {
                    target.Canvas.DrawImage(img, usedArea, usedArea, paint);
                }
            }
        }

        public SuspendRender SuspendRenderInvalidation() {
            Interlocked.Increment(ref this.suspendRenderCount);
            return new SuspendRender(this);
        }
    }

    public struct SuspendRender : IDisposable {
        internal RenderManager manager;

        internal SuspendRender(RenderManager manager) {
            this.manager = manager;
        }

        public void Dispose() {
            if (this.manager != null)
                Interlocked.Decrement(ref this.manager.suspendRenderCount);
            this.manager = null;
        }
    }
}