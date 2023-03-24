using System.Threading;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Project;
using FramePFX.Render;
using FramePFX.ResourceManaging;
using FramePFX.Timeline;
using FramePFX.Utils;

namespace FramePFX {
    public class ViewportViewModel : BaseViewModel, IEditor {
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
        /// A handle to the actual view port
        /// </summary>
        public IAutoRenderTarget ViewportHandle { get; set; }

        private readonly Thread playbackThread;
        private long nextPlaybackTick;
        private long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;
        public readonly NumberAverager playbackAverageIntervalMS = new NumberAverager(10);

        public ViewportViewModel() {
            IoC.Editor = this;
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

        public void RenderViewPort() {
            IAutoRenderTarget view = this.ViewportHandle;
            if (view != null && view.IsReadyForRender) {
                view.Context.BeginRender();
                view.Render();
                view.Context.EndRender();
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
                        this.RenderViewPort();
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
