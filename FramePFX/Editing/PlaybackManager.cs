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
using Fractions;
using FramePFX.Editing.Rendering;
using FramePFX.Editing.Timelines;
using FramePFX.Natives;
using PFXToolKitUI;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing;

/// <summary>
/// An event sent by a <see cref="PlaybackManager"/>
/// <param name="sender">The playback manager</param>
/// <param name="state">The new state. Play may be repeatedly sent</param>
/// <param name="frame">The starting frame when playing, the current frame when pausing, and the last play or jump frame when stopping</param>
/// </summary>
public delegate void PlaybackStateEventHandler(PlaybackManager sender, PlayState state, long frame);

/// <summary>
/// A class that manages the playback functionality of the editor, which manages the timer and play/pause/stop states
/// </summary>
public class PlaybackManager {
    private static readonly long THREAD_SPLICE_IN_TICKS = (long) (16.4d * Time.TICK_PER_MILLIS);
    private static readonly long YIELD_MILLIS_IN_TICKS = Time.TICK_PER_MILLIS / 10;

    // thread stuff
    private volatile bool thread_IsPlaying;
    private bool isAudioPlaying;
    private volatile bool thread_IsTimerRunning;
    private long intervalTicks;
    private long nextTickTime;
    private Thread? thread;

    public PlayState PlayState { get; private set; } = PlayState.Stop;

    // the time at which the playback began. Used to skip frames future frames that could not
    // be rendered in time while still trying maintaining an accurate frame rate
    private DateTime lastRenderTime;
    private double accumulatedVideoSubFrames;
    private double audioSamplesPerFrame;
    private double accumulatedAudioSubSamples;

    /// <summary>
    /// The editor which owns this playback manager object. This does not change
    /// </summary>
    public VideoEditor Editor { get; }

    /// <summary>
    /// Gets the timeline that this playback manager processes. This changes when the editor's project's <see cref="Project.ActiveTimeline"/> changes
    /// </summary>
    public Timeline? Timeline { get; private set; }

    public double AveragePlaybackIntervalMillis => this.averager.GetAverage() / Time.TICK_PER_MILLIS;

    /// <summary>
    /// An event fired when the play, pause or stop methods are called, if the current playback state does not match the matching function
    /// </summary>
    public event PlaybackStateEventHandler? PlaybackStateChanged;

    private PFXNative.NativeAudioEngineData audioEngineData;

    public unsafe delegate int ManagedAudioEngineCallback(void* output, ulong framesPerBuffer, IntPtr timeInfo, ulong statusFlags);

    private readonly ManagedAudioEngineCallback? engineCallbackDelgate;
    private readonly IntPtr engineCallbackDelgatePtr;
    private double phase;

    private readonly NumberAverager averager = new NumberAverager(5);
    private long lastTickTime;
    private volatile Action? stopCallback;

    public PlaybackManager(VideoEditor editor) {
        this.Editor = editor;
        // unsafe
        // {
        //     this.engineCallbackDelgate = this.AudioEngineCallback;
        //     this.engineCallbackDelgatePtr = Marshal.GetFunctionPointerForDelegate(this.engineCallbackDelgate);
        //     this.audioEngineData = new PFXNative.NativeAudioEngineData()
        //     {
        //         ManagedAudioEngineCallback = this.engineCallbackDelgatePtr
        //     };
        // }
    }

    public unsafe int AudioEngineCallback(void* output, ulong framesPerBuffer, IntPtr timeInfo, ulong statusFlags) {
        float* outputFloats = (float*) output;
        AudioRingBuffer buffer = this.Timeline.RenderManager.audioRingBuffer;
        // const int sampleRate = 44100;
        // const float amplitude = 0.5F;
        // const float freq = 440;
        // const float deltaPhase = (float) (2.0 * Math.PI * freq / sampleRate);
        // const float PI2 = (float) Math.PI * 2.0F;
        // for (ulong i = 0; i < framesPerBuffer; ++i) {
        //     float sample = (float) (Math.Sin(this.phase) * amplitude);
        //     this.phase += deltaPhase;
        //     if (this.phase >= PI2)
        //         this.phase -= PI2;
        //     *outputFloats++ = sample;
        //     *outputFloats++ = sample;
        // }

        if (buffer != null) {
            lock (buffer) {
                int totalsamples = (int) (framesPerBuffer * 2);
                int readCount = buffer.ReadFromRingBuffer(outputFloats, totalsamples);
                for (int i = readCount < 1 ? 0 : readCount; i < totalsamples; i++) {
                    *outputFloats++ = 0;
                }
            }
        }

        // 0 means never finished
        return 0;
    }

    public bool CanSetPlayStateTo(PlayState newState) {
        if (this.Timeline == null) {
            return false;
        }

        switch (newState) {
            case PlayState.Play: return this.PlayState != PlayState.Play;
            case PlayState.Pause:
            case PlayState.Stop:
                return this.PlayState == PlayState.Play;
            default: throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }

    public void StartTimer() {
        if (this.thread != null && this.thread.IsAlive) {
            throw new InvalidOperationException("Timer thread already running");
        }

        this.thread = new Thread(this.TimerMain) {
            IsBackground = true, // thread auto-stops when app tries to exit :D
            Name = "FramePFX Playback Thread"
        };

        this.thread_IsTimerRunning = true;
        this.thread.Start();
    }

    public void StopTimer() {
        this.Stop();
        this.thread_IsTimerRunning = false;
        // this.thread?.Join();
    }

    public void SetFrameRate(Fraction frameRate) {
        double fps = frameRate.ToDouble();
        if (double.IsNaN(fps) || double.IsInfinity(fps))
            fps = 1.0;

        this.intervalTicks = (long) Math.Round(Time.TICK_PER_SECOND_D / fps);
        this.audioSamplesPerFrame = (int) Math.Ceiling(44100.0 / fps);
    }

    public void Play() {
        if (this.Timeline == null || this.PlayState == PlayState.Play) {
            return;
        }

        this.PlayInternal(this.Timeline.PlayHeadPosition);
    }

    public void Play(long frame) {
        if (this.Timeline == null || (this.PlayState == PlayState.Play && this.Timeline.PlayHeadPosition == frame)) {
            return;
        }

        this.PlayInternal(frame);
    }

    private void PlayInternal(long targetFrame) {
        this.PlayState = PlayState.Play;
        this.PlaybackStateChanged?.Invoke(this, this.PlayState, targetFrame);
        this.lastRenderTime = DateTime.Now;
        this.lastTickTime = Time.GetSystemTicks();
        this.thread_IsPlaying = true;

        // unsafe {
        //     fixed (PFXNative.NativeAudioEngineData* engineData = &this.audioEngineData) {
        //         this.isAudioPlaying = PFXNative.PFXAE_BeginAudioPlayback(engineData) == 0;
        //     }
        // }
    }

    private void OnAboutToStopPlaying(Action? callback = null) {
        if (callback != null)
            Interlocked.Exchange(ref this.stopCallback, callback);
        this.thread_IsPlaying = false;
    }

    private void OnStoppedPlaying() {
        if (this.isAudioPlaying) {
            unsafe {
                // fixed (PFXNative.NativeAudioEngineData* engineData = &this.audioEngineData) {
                //     PFXNative.PFXAE_EndAudioPlayback(engineData);
                // }
            }

            this.isAudioPlaying = false;
        }
    }

    public void Pause() {
        if (this.PlayState != PlayState.Play || this.Timeline == null) {
            return;
        }

        this.OnAboutToStopPlaying();
        long playHead = this.Timeline.PlayHeadPosition;

        this.PlayState = PlayState.Pause;
        this.PlaybackStateChanged?.Invoke(this, this.PlayState, this.Timeline.StopHeadPosition);
        this.OnStoppedPlaying();
        this.Timeline.StopHeadPosition = playHead;
    }

    public void Stop() {
        if (this.PlayState != PlayState.Play || this.Timeline == null) {
            return;
        }

        this.OnAboutToStopPlaying(this.InvalidateVisualForStop);
        this.PlayState = PlayState.Stop;
        this.PlaybackStateChanged?.Invoke(this, this.PlayState, this.Timeline.StopHeadPosition);
        this.OnStoppedPlaying();
        this.Timeline.PlayHeadPosition = this.Timeline.StopHeadPosition;
    }

    private void InvalidateVisualForStop() {
        // There's a chance this method gets called after the app has shutdown when a user closes the main window.
        using BugTrack.EmptyToken reallyBadImplementation = BugTrack.ReallyBadImplementation(this, "Task cancelled when closing main window");

        // Checking phase does not help since the render thread is way too fast to respond to
        // the stop command, and therefore, the phase will almost always be Running
        if (Application.Instance.IsBeforePhase(ApplicationStartupPhase.Stopping)) {
            try {
                Application.Instance.Dispatcher.Invoke(() => this.Timeline?.InvalidateRender(), DispatchPriority.Background);
            }
            catch (TaskCanceledException) {
                // ignored
            }
        }
    }

    private void OnTimerFrame() {
        if (!this.thread_IsPlaying || !this.thread_IsTimerRunning) {
            this.TryInvokeStopCallback();
            return;
        }

        if (this.Timeline?.Project == null) {
            return;
        }

        using (this.Timeline.RenderManager.SuspendRenderInvalidation()) {
            Task renderTask = Application.Instance.Dispatcher.Invoke(() => {
                Timeline timeline = this.Timeline;
                Project project;
                if (timeline == null || (project = timeline.Project) == null || !timeline.IsActive) {
                    return Task.CompletedTask;
                }

                VideoEditor? editor = project.Editor;
                if (editor == null || !this.thread_IsPlaying) {
                    this.TryInvokeStopCallback();
                    return Task.CompletedTask;
                }

                // A lot of this extra code is to maintain the frame rate even if the render is stalling.
                // IF for example there are a bunch of high quality videos being rendered and the render
                // is waiting for them to decode, there could be a say 100ms render interval (10fps),
                // and since we don't want to playback at 10 fps, we try to catch up by skipping a few frames

                // However... this has a possible runaway effect specifically for things like decoder seeking;
                // If we skip frames, the decoders may have to seek frames, further stalling the render meaning
                // it will take even longer, meaning we skip more frames, and so on...
                // So, frame skipping is limited to 3 frames just to be safe. Still need to work out a better
                // solution but for now 3 frames should be generally safe
                double fps = project.Settings.FrameRateDouble;
                double expectedInterval = Time.TICK_PER_SECOND_D / fps;
                double actualInterval = DateTime.Now.Ticks - this.lastRenderTime.Ticks;
                this.lastRenderTime = DateTime.Now;

                long incr = 1;
                if (actualInterval > expectedInterval) {
                    double diffMillis = (actualInterval - expectedInterval) / Time.TICK_PER_MILLIS_D;
                    double incrDouble = (diffMillis / (1000.0 / fps)) + this.accumulatedVideoSubFrames;
                    long extra = (long) Math.Floor(incrDouble);
                    this.accumulatedVideoSubFrames = (incrDouble - extra);
                    incr += extra;
                }

                // Don't allow jumps of more than 3 frames, otherwise that runaway thing might occur
                incr = Math.Min(incr, 3);

                long newPlayHead;
                long newPlayHeadUnprocessed = timeline.PlayHeadPosition + incr;
                FrameSpan? loopRegion = timeline.IsLoopRegionEnabled ? timeline.LoopRegion : null;
                if (loopRegion is FrameSpan loop && !loop.IsEmpty && newPlayHeadUnprocessed >= loop.Begin && newPlayHeadUnprocessed <= loop.EndIndex) {
                    newPlayHead = Periodic.MethodNameHere(newPlayHeadUnprocessed, loop.Begin, loop.EndIndex);
                }
                else {
                    newPlayHead = Periodic.MethodNameHere(newPlayHeadUnprocessed, 0, timeline.MaxDuration - 1);
                }

                timeline.PlayHeadPosition = newPlayHead;
                if ((timeline.RenderManager.LastRenderTask?.IsCompleted ?? true)) {
                    return RenderTimeline(editor, timeline.RenderManager, timeline.PlayHeadPosition, CancellationToken.None);
                }

                return Task.CompletedTask;
            });

            try {
                renderTask.Wait();
            }
            catch (AggregateException) {
                if (!renderTask.IsCanceled)
                    throw;
            }
            catch (TaskCanceledException) {
            }
            catch (OperationCanceledException) {
            }
            catch (Exception e) {
                this.thread_IsPlaying = false;
                AppLogger.Instance.WriteLine("Render exception on playback thread");
                AppLogger.Instance.WriteLine(e.GetToString());
            }
        }

        long time = Time.GetSystemTicks();
        long tickInterval = time - this.lastTickTime;
        this.lastTickTime = time;
        this.averager.PushValue(tickInterval);
    }

    private static async Task RenderTimeline(VideoEditor videoEditor, RenderManager renderManager, long frame, CancellationToken cancellationToken) {
        // await (renderManager.ScheduledRenderTask ?? Task.CompletedTask);
        if (cancellationToken.IsCancellationRequested)
            throw new TaskCanceledException();
        try {
            await (renderManager.LastRenderTask = renderManager.RenderTimelineAsync(frame, cancellationToken));
        }
        catch (TaskCanceledException) {
        }
        catch (OperationCanceledException) {
        }
        catch (Exception e) {
            if (videoEditor.Playback.PlayState == PlayState.Play) {
                videoEditor.Playback.Pause();
            }

            Debug.WriteLine(e.GetToString());
        }

        renderManager.OnFrameCompleted();
    }

    private void TimerMain() {
        do {
            if (!this.thread_IsPlaying) {
                this.TryInvokeStopCallback();
                // Saves attempting to invoke stopCallback every 50ms
                do {
                    Thread.Sleep(50);
                } while (!this.thread_IsPlaying);
            }

            long target = this.nextTickTime;
            while ((target - Time.GetSystemTicks()) > THREAD_SPLICE_IN_TICKS)
                Thread.Sleep(1);
            while ((target - Time.GetSystemTicks()) > YIELD_MILLIS_IN_TICKS)
                Thread.Yield();

            // CPU intensive wait
            long time = Time.GetSystemTicks();
            while (time < target) {
                Thread.SpinWait(8);
                time = Time.GetSystemTicks();
            }

            this.nextTickTime = Time.GetSystemTicks() + this.intervalTicks;
            this.OnTimerFrame();
        } while (this.thread_IsTimerRunning);
    }

    internal static void InternalOnActiveTimelineChanged(PlaybackManager playback, Timeline oldTimeline, Timeline newTimeline) {
        playback.Stop();
        playback.Timeline = newTimeline;
    }

    private void TryInvokeStopCallback() {
        Interlocked.Exchange(ref this.stopCallback, null)?.Invoke();
    }
}