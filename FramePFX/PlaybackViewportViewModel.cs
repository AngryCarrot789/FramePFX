using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Project;
using FramePFX.Render;
using FramePFX.Timeline;
using FramePFX.Timeline.Layer.Clips;
using FramePFX.Utils;
using OpenTK.Graphics.OpenGL;

namespace FramePFX {
    public class PlaybackViewportViewModel : BaseViewModel {
        private volatile bool isPlaying;
        public bool IsPlaying {
            get => this.isPlaying;
            set {
                this.isPlaying = value;
                this.RaisePropertyChanged();
            }
        }

        public ICommand PlayPauseCommand { get; set; }

        public RelayCommand PlayCommand { get; set; }

        public RelayCommand PauseCommand { get; set; }

        /// <summary>
        /// A handle to the main view port
        /// </summary>
        public IViewPort ViewPortHandle { get; set; }

        private readonly Thread playbackThread;
        private long nextPlaybackTick;
        private long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;
        public readonly NumberAverager playbackAverageIntervalMS = new NumberAverager(10);

        public PlaybackViewportViewModel() {
            this.PlayPauseCommand = new RelayCommand(() => {
                if (this.IsPlaying) {
                    this.PauseAction();
                }
                else {
                    this.PlayAction();
                }
            });

            this.PlayCommand = new RelayCommand(this.PlayAction, () => !this.IsPlaying);
            this.PauseCommand = new RelayCommand(this.PauseAction, () => this.IsPlaying);
            this.isPlaybackThreadRunning = true;
            // using a DispatcherTimer instead of a Thread will not make anything better
            this.playbackThread = new Thread(this.PlaybackThreadMain);
            this.playbackThread.Start();
        }

        public bool IsReadyForRender() {
            return this.ViewPortHandle != null && this.ViewPortHandle.IsReady && this.ViewPortHandle.Context.IsReady;
        }

        public void RenderTimeline(TimelineViewModel timeline) {
            if (this.IsReadyForRender()) {
                // Render main view port
                long playHead = timeline.PlayHeadFrame;
                IViewPort view = this.ViewPortHandle;
                if (!view.BeginRender(true)) {
                    return;
                }

                List<ClipContainerViewModel> clips = timeline.GetClipsOnPlayHead().ToList();
                GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
                // TODO: change this to support layer opacity. And also move to shaders because this glVertex3f old stuff it no good
                foreach (ClipContainerViewModel clip in clips) {
                    if (clip.ClipContent is IClipRenderTarget target) {
                        target.Render(view, playHead);
                    }

                    // TODO: add audio... somehow. I have no idea how to do audio lololol
                    // else if (handle.ClipHandle is IAudioRenderTarget) {
                    //
                    // }
                }

                view.FlushFrame();
                view.EndRender();
            }
        }

        // TODO: Maybe move this into a non-viewmodel, so that it's more MVVM-ey?
        private void PlaybackThreadMain() {
            this.lastPlaybackTick = 0;
            this.nextPlaybackTick = 0;
            while (this.isPlaybackThreadRunning) {
                ProjectViewModel project;
                TimelineViewModel timeline;
                if ((project = IoC.ActiveProject) == null || (timeline = project.Timeline) == null) {
                    Thread.Sleep(100);
                    continue;
                }

                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick) { //  || (time + 1) >= this.nextPlaybackTick // time + 1 for ahead of time playback... just in case
                        this.playbackAverageIntervalMS.PushValue(time - this.lastPlaybackTick);
                        this.lastPlaybackTick = time;
                        this.nextPlaybackTick = time + 33L; // 33ms = 30fps
                        timeline.StepFrame();
                    }

                    // Directly render clips here, instead of waiting for the GL thread to do it (and also
                    // the GL thread needs to be removed because it just chews CPU for no reason)
                    if (timeline.IsRenderDirty) {
                        this.RenderTimeline(timeline);
                    }

                    // yield results in a generally higher CPU usage due to the fact that
                    // the thread time-splice duration may be in the order of 1s of millis
                    // meaning this function will generally have very high precision and will
                    // absolutely nail the FPS with pinpoint precision
                    Thread.Yield();
                }
                else if (timeline.isFramePropertyChangeScheduled) {
                    timeline.RaisePropertyChanged(nameof(timeline.PlayHeadFrame));
                    timeline.isFramePropertyChangeScheduled = false;
                }
                else {
                    Thread.Sleep(20);
                }
            }
        }

        public void PlayAction() {
            if (this.IsPlaying) {
                return;
            }

            this.nextPlaybackTick = Time.GetSystemMillis();
            this.IsPlaying = true;
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
        }

        public void PauseAction() {
            if (!this.IsPlaying) {
                return;
            }

            this.IsPlaying = false;
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
        }
    }
}
