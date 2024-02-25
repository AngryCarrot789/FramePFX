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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines;
using FramePFX.Logger;
using FramePFX.Natives;
using FramePFX.Utils;

namespace FramePFX.Editors {
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
        private Thread thread;

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
        public Timeline Timeline { get; private set; }

        /// <summary>
        /// An event fired when the play, pause or stop methods are called, if the current playback state does not match the matching function
        /// </summary>
        public event PlaybackStateEventHandler PlaybackStateChanged;

        private PFXNative.NativeAudioEngineData streamData;

        public unsafe delegate int ManagedAudioEngineCallback(void* output, ulong framesPerBuffer, IntPtr timeInfo, ulong statusFlags);

        private readonly ManagedAudioEngineCallback engineCallbackDelgate;
        private readonly IntPtr engineCallbackDelgatePtr;
        private double phase;

        public PlaybackManager(VideoEditor editor) {
            this.Editor = editor;
            unsafe {
                this.engineCallbackDelgate = this.AudioEngineCallback;
                this.engineCallbackDelgatePtr = Marshal.GetFunctionPointerForDelegate(this.engineCallbackDelgate);
                this.streamData = new PFXNative.NativeAudioEngineData() {
                    ManagedAudioEngineCallback = this.engineCallbackDelgatePtr
                };
            }
        }

        public unsafe int AudioEngineCallback(void* output, ulong framesPerBuffer, IntPtr timeInfo, ulong statusFlags) {
            float* outputBytes = (float*) output;
            AudioRingBuffer buffer = this.Timeline.RenderManager.audioRingBuffer;
            if (buffer != null) {
                lock (buffer) {
                    int readCount = buffer.ReadFromRingBuffer((byte*) output, (int) (framesPerBuffer * sizeof(float)));
                    for (ulong i = readCount < 1 ? 0 : (ulong) (readCount / sizeof(float)); i < framesPerBuffer; i++) {
                        *outputBytes++ = 0;
                    }
                }
            }

            return 0;

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
            //     *outputBytes++ = sample;
            //     *outputBytes++ = sample;
            // }
            // // 0 means never finished
            // return 0;
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
                IsBackground = true,
                Name = "Timer Thread"
            };

            this.thread_IsTimerRunning = true;
            this.thread.Start();
        }

        public void StopTimer() {
            this.thread_IsTimerRunning = false;
            this.thread?.Join();
        }

        public void SetFrameRate(Rational frameRate) {
            double fps = frameRate.AsDouble;
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
            if (this.Timeline == null) {
                return;
            }

            if (this.PlayState == PlayState.Play && this.Timeline.PlayHeadPosition == frame) {
                return;
            }

            this.PlayInternal(frame);
        }

        private void PlayInternal(long targetFrame) {
            this.PlayState = PlayState.Play;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, targetFrame);
            this.lastRenderTime = DateTime.Now;
            // this.waveOut.Play();
            this.thread_IsPlaying = true;

            unsafe {
                fixed (PFXNative.NativeAudioEngineData* engineData = &this.streamData) {
                    this.isAudioPlaying = PFXNative.PFXAE_BeginAudioPlayback(engineData) == 0;
                }
            }
        }

        private void OnAboutToStopPlaying() {
            // this.waveOut.Stop();
            this.thread_IsPlaying = false;
        }

        private void OnStoppedPlaying() {
            if (this.isAudioPlaying) {
                unsafe {
                    fixed (PFXNative.NativeAudioEngineData* engineData = &this.streamData) {
                        PFXNative.PFXAE_EndAudioPlayback(engineData);
                    }
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

            this.OnAboutToStopPlaying();
            this.PlayState = PlayState.Stop;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, this.Timeline.StopHeadPosition);
            this.OnStoppedPlaying();
            this.Timeline.PlayHeadPosition = this.Timeline.StopHeadPosition;
        }

        private void OnTimerFrame() {
            if (!this.thread_IsPlaying) {
                return;
            }

            if (this.Timeline == null || this.Timeline.Project == null) {
                return;
            }

            using (this.Timeline.RenderManager.SuspendRenderInvalidation()) {
                Task renderTask = IoC.Dispatcher.Invoke(() => {
                    Timeline timeline = this.Timeline;
                    Project project;
                    if (timeline == null || (project = timeline.Project) == null || !timeline.IsActive) {
                        return Task.CompletedTask;
                    }

                    if (project.Editor == null || !this.thread_IsPlaying) {
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
                    double fps = project.Settings.FrameRate.AsDouble;
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

                    long newPlayHead = Periodic.Add(timeline.PlayHeadPosition, incr, 0, timeline.MaxDuration - 1);
                    timeline.PlayHeadPosition = newPlayHead;
                    if ((timeline.RenderManager.LastRenderTask?.IsCompleted ?? true)) {
                        return RenderTimeline(timeline.RenderManager, timeline.PlayHeadPosition, CancellationToken.None);
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
        }

        private static async Task RenderTimeline(RenderManager renderManager, long frame, CancellationToken cancellationToken) {
            // await (renderManager.ScheduledRenderTask ?? Task.CompletedTask);
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();
            await (renderManager.LastRenderTask = renderManager.RenderTimelineAsync(frame, cancellationToken));
            renderManager.OnFrameCompleted();
        }

        private void TimerMain() {
            do {
                while (!this.thread_IsPlaying) {
                    Thread.Sleep(50);
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
                if (this.thread_IsPlaying) {
                    this.OnTimerFrame();
                }
            } while (this.thread_IsTimerRunning);

            // long frameEndTicks = Time.GetSystemTicks();
            // while (this.thread_IsTimerRunning) {
            //     while (!this.thread_IsPlaying) {
            //         Thread.Sleep(50);
            //     }
            //     do {
            //         long ticksA = Time.GetSystemTicks();
            //         long interval = ticksA - frameEndTicks;
            //         if (interval >= this.intervalTicks)
            //             break;
            //         Thread.Sleep(1);
            //     } while (true);
            //     if (this.thread_IsPlaying) {
            //         try {
            //             this.OnTimerFrame();
            //         }
            //         catch (Exception e) {
            //             // Don't crash the timer thread in release, just ignore it (or break in release)
            //             #if DEBUG
            //             System.Diagnostics.Debugger.Break();
            //             #endif
            //         }
            //     }
            //     frameEndTicks = Time.GetSystemTicks();
            // }
        }

        internal static void InternalOnActiveTimelineChanged(PlaybackManager playback, Timeline oldTimeline, Timeline newTimeline) {
            playback.Stop();
            playback.Timeline = newTimeline;
        }
    }
}