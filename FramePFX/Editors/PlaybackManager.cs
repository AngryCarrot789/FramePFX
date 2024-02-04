using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FramePFX.Editors.Rendering;
using FramePFX.Editors.Timelines;
using FramePFX.Logger;
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
        private volatile bool thread_IsTimerRunning;
        private long intervalTicks;
        private long nextTickTime;
        private Thread thread;

        public PlayState PlayState { get; private set; } = PlayState.Stop;

        // the time at which the playback began. Used to skip frames future frames that could not
        // be rendered in time while still trying maintaining an accurate frame rate
        private DateTime lastRenderTime;
        private double accumulatedRateMillis;

        /// <summary>
        /// The editor which owns this playback manager object. This does not change
        /// </summary>
        public VideoEditor Editor { get; }

        /// <summary>
        /// An event fired when the play, pause or stop methods are called, if the current playback state does not match the matching function
        /// </summary>
        public event PlaybackStateEventHandler PlaybackStateChanged;

        public PlaybackManager(VideoEditor editor) {
            this.Editor = editor;
        }

        public bool CanSetPlayStateTo(PlayState newState) {
            if (this.Editor.Project == null) {
                return false;
            }

            switch (newState) {
                case PlayState.Play: return this.PlayState != PlayState.Play;
                case PlayState.Pause:
                case PlayState.Stop: return this.PlayState == PlayState.Play;
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
            this.intervalTicks = (long) Math.Round(1000.0 / frameRate.AsDouble * Time.TICK_PER_MILLIS);
        }

        public void Play() {
            if (!(this.Editor.Project is Project project) || this.PlayState == PlayState.Play) {
                return;
            }

            this.PlayInternal(project.MainTimeline.PlayHeadPosition);
        }

        public void Play(long frame) {
            if (!(this.Editor.Project is Project project)) {
                return;
            }

            if (this.PlayState == PlayState.Play && project.MainTimeline.PlayHeadPosition == frame) {
                return;
            }

            this.PlayInternal(frame);
        }

        private void PlayInternal(long targetFrame) {
            this.PlayState = PlayState.Play;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, targetFrame);
            this.lastRenderTime = DateTime.Now;
            this.thread_IsPlaying = true;
        }

        private void OnAboutToStopPlaying() {
            this.thread_IsPlaying = false;
        }

        public void Pause() {
            if (this.PlayState != PlayState.Play) {
                return;
            }

            if (!(this.Editor.Project is Project project)) {
                return;
            }

            this.OnAboutToStopPlaying();
            project.MainTimeline.StopHeadPosition = project.MainTimeline.PlayHeadPosition;
            this.PlayState = PlayState.Pause;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, project.MainTimeline.StopHeadPosition);
        }

        public void Stop() {
            if (this.PlayState != PlayState.Play) {
                return;
            }

            if (!(this.Editor.Project is Project project)) {
                return;
            }

            this.OnAboutToStopPlaying();
            this.PlayState = PlayState.Stop;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, project.MainTimeline.StopHeadPosition);
            project.MainTimeline.PlayHeadPosition = project.MainTimeline.StopHeadPosition;
        }

        private void OnTimerFrame() {
            if (!this.thread_IsPlaying) {
                return;
            }

            Project project = this.Editor.Project;
            if (project == null) {
                return;
            }

            using (project.RenderManager.SuspendRenderInvalidation()) {
                Task renderTask = Application.Current.Dispatcher.Invoke(() => {
                    if (project.Editor == null || !this.thread_IsPlaying) {
                        return Task.CompletedTask;
                    }

                    // A lot of this extra code is to maintain the frame rate even if the render is stalling.
                    // IF for example there are a bunch of high quality videos being rendered and the render
                    // is waiting for them to decode, there could be a say 100ms render interval (10fps),
                    // and since we don't want to playback at 10 fps, we try to catch up by skipping a few frames

                    // However... this has a possible runaway effect specifically for things like decoder seeking;
                    // If we skip frames, the render will take even longer, meaning we skip more frames, and so on,
                    // So, frame skipping is limited to 3 frames just to be safe. Still need to work out a better
                    // solution but for now, 3 frames should be generally safe
                    Timeline timeline = project.MainTimeline;
                    double fps = project.Settings.FrameRate.AsDouble;
                    double expectedInterval = Time.TICK_PER_SECOND_D / fps;
                    double actualInterval = DateTime.Now.Ticks - this.lastRenderTime.Ticks;
                    this.lastRenderTime = DateTime.Now;

                    long incr = 1;
                    if (actualInterval > expectedInterval) {
                        double diffMillis = (actualInterval - expectedInterval) / Time.TICK_PER_MILLIS_D;
                        double incrDouble = (diffMillis / (1000.0 / fps)) + this.accumulatedRateMillis;
                        long extra = (long) Math.Floor(incrDouble);
                        this.accumulatedRateMillis = (incrDouble - extra);
                        incr += extra;
                    }

                    // Don't allow jumps of more than 3 frames, otherwise that runaway thing might occur
                    incr = Math.Min(incr, 3);

                    long newPlayHead = Periodic.Add(timeline.PlayHeadPosition, incr, 0, timeline.MaxDuration - 1);
                    timeline.PlayHeadPosition = newPlayHead;
                    if ((project.RenderManager.ScheduledRenderTask?.IsCompleted ?? true)) {
                        return RenderTimeline(project.RenderManager, timeline, timeline.PlayHeadPosition, CancellationToken.None);
                    }

                    return Task.CompletedTask;
                });

                try {
                    renderTask.Wait();
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

        private static async Task RenderTimeline(RenderManager renderManager, Timeline timeline, long frame, CancellationToken cancellationToken) {
            // await (renderManager.ScheduledRenderTask ?? Task.CompletedTask);
            if (cancellationToken.IsCancellationRequested)
                throw new TaskCanceledException();
            await renderManager.RenderTimelineAsync(timeline, frame, cancellationToken);
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
    }
}