using System;
using System.Threading;
using System.Windows.Input;
using System.Windows.Threading;
using FramePFX.Core;
using FramePFX.Core.ResourceManaging;
using FramePFX.Render;
using FramePFX.Timeline;
using FramePFX.Utils;

namespace FramePFX {
    public class MainViewModel : BaseViewModel, IEditor {
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

        private readonly Thread playbackThread;
        private readonly DispatcherTimer playbackTimer;
        private long nextPlaybackTick;
        private long lastPlaybackTick;
        public volatile bool isPlaybackThreadRunning;

        public MainViewModel() {
            this.ResourceManager = new ResourceManagerViewModel();
            this.Timeline = new TimelineViewModel(this);
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
            this.playbackThread = new Thread(this.OnTickPlayback);
            this.playbackThread.Start();
        }

        public void OnTickPlayback() {
            this.lastPlaybackTick = 0;
            this.nextPlaybackTick = 0;
            while (this.isPlaybackThreadRunning) {
                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick || (time + 1) >= this.nextPlaybackTick) {
                        this.PlaybackAverageIntervalMS.PushValue(time - this.lastPlaybackTick);
                        this.lastPlaybackTick = time;
                        this.nextPlaybackTick = time + 33L; // 33ms = 30fps
                        this.Timeline.StepFrame();
                    }

                    // TODO: Directly render clips here, instead of waiting for the GL thread to do it (and also remove the GL thread ticking stuff)
                    if (this.Timeline.IsRenderDirty) {
                        IOGLViewPort mainViewPort = IoC.Instance.Provide<IOGLViewPort>();
                        if (mainViewPort is IRenderHandler handler) {
                            mainViewPort.Context.UseContext(() => {
                                handler.RenderGLThread();
                                TimelineViewModel.Instance.IsRenderDirty = false;
                                handler.Tick(0d);
                            }, true);
                        }
                    }

                    Thread.Yield();
                }
                else if (this.Timeline.isFramePropertyChangeScheduled) {
                    this.Timeline.RaisePropertyChanged(nameof(this.Timeline.PlayHeadFrame));
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
