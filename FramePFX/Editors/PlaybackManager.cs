using System;
using System.Threading;
using System.Windows;
using FramePFX.Editors.Timelines;
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

        public void SetFrameRate(double frameRate) {
            this.intervalTicks = (long) Math.Round(1000.0 / frameRate * Time.TICK_PER_MILLIS);
        }

        public void Play() {
            if (!(this.Editor.Project is Project project) || this.PlayState == PlayState.Play) {
                return;
            }

            this.PlayState = PlayState.Play;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, project.MainTimeline.PlayHeadPosition);
            this.thread_IsPlaying = true;
        }

        public void Play(long frame) {
            if (!(this.Editor.Project is Project project)) {
                return;
            }

            if (this.PlayState == PlayState.Play && project.MainTimeline.PlayHeadPosition == frame) {
                return;
            }

            this.PlayState = PlayState.Play;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, frame);
            this.thread_IsPlaying = true;
        }

        public void Pause() {
            if (this.PlayState != PlayState.Play) {
                return;
            }

            if (!(this.Editor.Project is Project project)) {
                return;
            }

            this.thread_IsPlaying = false;
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

            this.PlayState = PlayState.Stop;
            this.PlaybackStateChanged?.Invoke(this, this.PlayState, project.MainTimeline.StopHeadPosition);
            project.MainTimeline.PlayHeadPosition = project.MainTimeline.StopHeadPosition;
            this.thread_IsPlaying = false;
        }

        private void OnTimerFrame() {
            Application.Current.Dispatcher.Invoke(() => {
                if (this.thread_IsPlaying && this.Editor.Project is Project project) {
                    Timeline timeline = project.MainTimeline;

                    // Increment or wrap to beginning
                    if (timeline.PlayHeadPosition == (timeline.MaxDuration - 1)) {
                        timeline.PlayHeadPosition = 0;
                    }
                    else {
                        // This is not how I indent to cause a re-render, but for now, changing the play head triggers a render
                        timeline.PlayHeadPosition++;
                    }
                }
            });
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