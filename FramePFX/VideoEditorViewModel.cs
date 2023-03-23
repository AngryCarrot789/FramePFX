using System.Threading;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Core.Render;
using FramePFX.Core.ResourceManaging;
using FramePFX.Core.Timeline;
using FramePFX.Utils;

namespace FramePFX {
    public class VideoEditorViewModel : BaseViewModel, IEditor {
        public TimelineViewModel Timeline { get; }

        public ResourceManagerViewModel ResourceManager { get; set; }

        private volatile bool isPlaying;

        public bool IsPlaying {
            get => this.isPlaying;
            set {
                this.isPlaying = value;
                this.RaisePropertyChanged();
            }
        }

        public readonly NumberAverager PlaybackAverageIntervalMS = new NumberAverager(10);

        public ICommand PlayPauseCommand { get; set; }

        public RelayCommand PlayCommand { get; set; }

        public RelayCommand PauseCommand { get; set; }

        public IRenderTarget MainViewPort { get; set; }

        private readonly Thread playbackThread;
        private long nextPlaybackTick;
        private long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;

        public VideoEditorViewModel() {
            this.ResourceManager = new ResourceManagerViewModel();
            IoC.ResourceManager = this.ResourceManager;

            this.Timeline = new TimelineViewModel();
            IoC.Timeline = this.Timeline;

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
            IRenderTarget view = this.MainViewPort;
            if (view != null) {
                view.Context.BeginRender();
                view.Render();
                view.Context.EndRender();
            }
        }

        public void PlaybackThreadMain() {
            this.lastPlaybackTick = 0;
            this.nextPlaybackTick = 0;
            while (this.isPlaybackThreadRunning) {
                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick || (time + 1) >= this.nextPlaybackTick) { // time + 1 for ahead of time playback... just in case
                        this.PlaybackAverageIntervalMS.PushValue(time - this.lastPlaybackTick);
                        this.lastPlaybackTick = time;
                        this.nextPlaybackTick = time + 33L; // 33ms = 30fps
                        this.Timeline.StepFrame();
                    }

                    // Directly render clips here, instead of waiting for the GL thread to do it (and also
                    // the GL thread needs to be removed because it just chews CPU for no reason)
                    if (this.Timeline.IsRenderDirty) {
                        this.RenderViewPort();
                    }

                    // yield results in a generally higher CPU usage due to the fact that
                    // the thread time-splice duration may be in the order of 1s of millis
                    // meaning this function will generally have very high precision and will
                    // absolutely nail the FPS with pinpoint precision
                    Thread.Yield();
                }
                else if (this.Timeline.isFramePropertyChangeScheduled) {
                    this.Timeline.RaisePropertyChanged(nameof(this.Timeline.PlayHeadFrame));
                    this.Timeline.isFramePropertyChangeScheduled = false;
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

            this.IsPlaying = true;
            this.PlayCommand.RaiseCanExecuteChanged();
            this.PauseCommand.RaiseCanExecuteChanged();
            this.nextPlaybackTick = Time.GetSystemMillis();
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
