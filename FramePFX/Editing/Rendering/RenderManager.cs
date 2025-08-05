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

using System.Diagnostics;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Clips.Video;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.Utils;
using PFXToolKitUI;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;
using PFXToolKitUI.Utils.RDA;
using SkiaSharp;

namespace FramePFX.Editing.Rendering;

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
    private volatile Task? lastRenderTask;
    private bool isDisposed;
    internal volatile int suspendRenderCount;
    private volatile int isRenderCancelled;
    internal volatile int useSlowRenderDispatchCount;
    private readonly RateLimitedDispatchAction fastRapidRenderDispatch;
    private readonly RateLimitedDispatchAction slowRapidRenderDispatch;
    private readonly RapidDispatchActionEx rapidRenderDispatch;

    private volatile bool isSkiaValid;
    private SKBitmap? bitmap;

    private SKPixmap? pixmap;

    // public but unsafe access to the underlying surface, used by view port. Must not be replaced externally
    public SKSurface? surface;

    public Task? LastRenderTask {
        get => this.lastRenderTask;
        set => this.lastRenderTask = value;
    }

    /// <summary>
    /// Gets the rect containing the bounds of the pixels that were modified during the last render
    /// </summary>
    public SKRect LastRenderRect;

    public AudioRingBuffer? audioRingBuffer;
    public int totalRenderCount;
    private volatile StackTrace stackTrace;

    public double AverageVideoRenderTimeMillis => this.averageVideoRenderTimeMillis;
    public double AverageAudioRenderTimeMillis => this.averageAudioRenderTimeMillis;

    public event FrameRenderedEventHandler? FrameRendered;
    public event EventHandler? BitmapsDisposed;

    public RenderManager(Timeline timeline) {
        this.Timeline = timeline;

        // These RLDAs are for spontaneous render requests.
        // They try to prevent the timeline being rendered say 500
        // times a second when the user is dragging around a clip,
        // but to speed up when for example using a number dragger on opacity maybe
        TimeSpan FastRenderInterval = TimeSpan.FromMilliseconds(1000.0 / 80.0); // 80 FPS = 12ms 
        this.fastRapidRenderDispatch = RateLimitedDispatchActionBase.ForDispatcherAsync(this.ScheduleRenderFromRLDA, FastRenderInterval, DispatchPriority.Send);

        TimeSpan SlowRenderInterval = TimeSpan.FromMilliseconds(1000.0 / 30.0); // 30 FPS = 33.33333ms 
        this.slowRapidRenderDispatch = RateLimitedDispatchActionBase.ForDispatcherAsync(this.ScheduleRenderFromRLDA, SlowRenderInterval, DispatchPriority.Send);

        this.rapidRenderDispatch = RapidDispatchActionEx.ForAsync(this.ScheduleRenderFromRapidDispatch, ApplicationPFX.Instance.Dispatcher, DispatchPriority.Normal);
        // this.renderThread = new Thread(this.RenderThreadMain);
    }

    private Task ScheduleRenderFromRLDA() {
        this.rapidRenderDispatch.InvokeAsync();
        return Task.CompletedTask;
    }

    private async Task ScheduleRenderFromRapidDispatch() {
        Task? task = this.lastRenderTask;
        if (task != null) {
            await task;
        }

        await (this.lastRenderTask = this.DoScheduledRender());
    }

    public SuspendRenderToken CancelRenderAndWaitForCompletion() {
        SuspendRenderToken suspension = this.SuspendRenderInvalidation();
        this.isRenderCancelled = 1;
        while (this.isRendering != 0)
            Thread.Sleep(1);
        return suspension;
    }

    public static void InternalOnTimelineProjectChanged(RenderManager manager, Project? oldProject, Project? newProject) {
        if (oldProject != null) {
            oldProject.Settings.ResolutionChanged -= manager.SettingsOnResolutionChanged;
            manager.DisposeCanvas();
            manager.audioRingBuffer?.Dispose();
        }

        if (newProject != null) {
            newProject.Settings.ResolutionChanged += manager.SettingsOnResolutionChanged;
            manager.SettingsOnResolutionChanged(newProject.Settings);
            manager.audioRingBuffer?.Dispose();
            manager.audioRingBuffer = new AudioRingBuffer(8192);
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
        this.BitmapsDisposed?.Invoke(this, EventArgs.Empty);
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
#if DEBUG
        StackTrace old = Interlocked.Exchange(ref this.stackTrace, new StackTrace(true));
#endif
        if (!ApplicationPFX.Instance.Dispatcher.CheckAccess())
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
        Project? project = this.Timeline.Project;
        if (project == null) {
            return Task.CompletedTask;
        }

        this.BeginRender(frame);
        long beginRender;
        int samplesToProcess, effectiveSamples;
        List<VideoTrack> videoTrackList;
        List<AudioTrack> audioTrackList;
        SKImageInfo imgInfo;

        try {
            videoTrackList = new List<VideoTrack>();
            audioTrackList = new List<AudioTrack>();
            imgInfo = this.ImageInfo;
            beginRender = Time.GetSystemTicks();

            double fps = project.Settings.FrameRateDouble;
            double sampleDouble = Math.Ceiling(44100.0 / fps) + this.accumulatedSamples;
            // Ensure value is even. 44100/60fps == 735, meaning that last sample for the
            // right channel wouldn't get generated, and the next render would write the first
            // left sample into the previous render's right channel :P
            effectiveSamples = (int) Maths.Ceil(sampleDouble, 2);
            this.accumulatedSamples = (sampleDouble - effectiveSamples);

            // samplesToProcess = 1024;
            samplesToProcess = effectiveSamples;

            // samples = project.Settings.BufferSize;

            // render bottom to top, as most video editors do
            for (int i = this.Timeline.Tracks.Count - 1; i >= 0; i--) {
                Track track = this.Timeline.Tracks[i];
                if (track is VideoTrack videoTrack && VideoTrack.IsEnabledParameter.GetCurrentValue(videoTrack)) {
                    if (videoTrack.PrepareRenderFrame(imgInfo, frame, quality)) {
                        videoTrackList.Add(videoTrack);
                    }
                }

                if (track is AudioTrack audioTrack && !AudioTrack.IsMutedParameter.GetCurrentValue(audioTrack)) {
                    if (audioTrack.PrepareRenderFrame(frame, samplesToProcess, quality)) {
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
            this.surface!.Canvas.Clear(SKColors.Transparent);
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

        // probably not a good idea to render audio like this...?
        Task renderAudio = Task.Run(async () => {
            this.CheckRenderCancelled();

            Task[] tasks = new Task[audioTrackList.Count];
            for (int i = 0; i < tasks.Length; i++) {
                AudioTrack track = audioTrackList[i];
                tasks[i] = Task.Run(() => track.RenderAudioFrame(samplesToProcess, quality), token);
            }

            this.CheckRenderCancelled();

            for (int i = 0; i < tasks.Length; i++) {
                if (!tasks[i].IsCompleted)
                    await tasks[i];
            }

            this.SumTrackSamples(audioTrackList, samplesToProcess, effectiveSamples);
            this.averageAudioRenderTimeMillis = (Time.GetSystemTicks() - beginRender) / Time.TICK_PER_MILLIS_D;
        }, token);

        return Task.WhenAll(renderVideo, renderAudio).ContinueWith(t => {
            this.LastRenderRect = totalRenderArea;
            this.isRendering = 0;
        }, TaskContinuationOptions.ExecuteSynchronously);
    }

    private unsafe void SumTrackSamples(List<AudioTrack> tracks, int sampleFrames, int effectiveSamples) {
        lock (this.audioRingBuffer!) {
            int samples = sampleFrames * 2;
            float* final = stackalloc float[samples];
            foreach (AudioTrack track in tracks) {
                float* tsamples = track.renderedSamples;
                for (int i = 0; i < samples; i++) {
                    *(final + i) = *(final + i) + tsamples[i];
                }
            }

            // if (effectiveSamples < sampleFrames) {
            //     this.audioRingBuffer.OffsetWrite(-(sampleFrames - effectiveSamples));
            // }

            this.audioRingBuffer.WriteToRingBuffer(final, samples);
        }
    }

    private void CheckRenderCancelled() {
        if (this.isRenderCancelled != 0 || !this.isSkiaValid)
            throw new TaskCanceledException();
    }

    // SaveLayer requires a temporary drawing bitmap, which can slightly
    // decrease performance, so only SaveLayer when absolutely necessary

    public static int BeginClipOpacityLayer(SKCanvas canvas, VideoClip clip, ref SKPaint? paint) {
        if (clip.UsesCustomOpacityCalculation || clip.RenderOpacity >= 1.0) {
            // check greater than just in case...
            return canvas.Save();
        }
        else {
            return canvas.SaveLayer(paint ??= new SKPaint {
                Color = new SKColor(255, 255, 255, clip.RenderOpacityByte)
            });
        }
    }

    public static void EndOpacityLayer(SKCanvas canvas, int count, ref SKPaint? paint) {
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
            // most likely suspendRenderCount > 0, which is true when
            // playback is active. PlaybackManager uses suspension to prevent
            // excessive rendering requests during automation updates
            return;
        }

        if (!this.Timeline.IsActive) {
            // if (this.Timeline is CompositionTimeline composition)
            //     composition.TryNotifyParentTimelineRenderInvalidated();
            return;
        }

        // don't invalidate when not in an editor, or while playing video
        VideoEditor? editor = this.Timeline.Project?.Editor;
        if (editor == null || editor.Playback.PlayState == PlayState.Play) {
            return;
        }

        RateLimitedDispatchAction dispatch = this.useSlowRenderDispatchCount > 0 ? this.slowRapidRenderDispatch : this.fastRapidRenderDispatch;
        dispatch.InvokeAsync();
    }

    /// <summary>
    /// Raises the <see cref="FrameRendered"/> event
    /// </summary>
    public void OnFrameCompleted() => this.FrameRendered?.Invoke(this);

    private async Task DoScheduledRender() {
        VideoEditor? editor;
        if (this.suspendRenderCount < 1 && (editor = this.Timeline.Project?.Editor) != null) {
            try {
                await this.RenderTimelineAsync(this.Timeline.PlayHeadPosition, CancellationToken.None, EnumRenderQuality.Low);
            }
            catch (TaskCanceledException) {
            }
            catch (OperationCanceledException) {
            }
            catch (Exception e) {
                if (editor.Playback.PlayState == PlayState.Play) {
                    editor.Playback.Pause();
                }

                await IMessageDialogService.Instance.ShowMessage("Render Error", "An exception occurred while rendering", e.GetToString());
                Debug.WriteLine(e.GetToString());
            }

            this.OnFrameCompleted();
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

        this.surface!.Flush();

        // this is the same code that VideoTrack uses for efficient final frame assembly
        SKRect frameRect = this.ImageInfo.ToRect();
        if (usedArea == frameRect) {
            this.surface.Draw(target.Canvas, 0, 0, paint);
        }
        else {
            using (SKImage img = SKImage.FromPixels(this.ImageInfo, this.bitmap!.GetPixels())) {
                target.Canvas.DrawImage(img, usedArea, usedArea, paint);
            }
        }
    }

    public SuspendRenderToken SuspendRenderInvalidation() {
        Interlocked.Increment(ref this.suspendRenderCount);
        return new SuspendRenderToken(this);
    }

    public UseSlowRenderToken UseSlowRenderDispatch() {
        Interlocked.Increment(ref this.useSlowRenderDispatchCount);
        return new UseSlowRenderToken(this);
    }
}

public struct SuspendRenderToken : IDisposable {
    internal RenderManager manager;

    internal SuspendRenderToken(RenderManager manager) {
        this.manager = manager;
    }

    public void Dispose() {
        if (this.manager != null)
            Interlocked.Decrement(ref this.manager.suspendRenderCount);
        this.manager = null;
    }
}

public struct UseSlowRenderToken : IDisposable {
    internal RenderManager manager;

    internal UseSlowRenderToken(RenderManager manager) {
        this.manager = manager;
    }

    public void Dispose() {
        if (this.manager != null)
            Interlocked.Decrement(ref this.manager.useSlowRenderDispatchCount);
        this.manager = null;
    }
}