using System;
using System.Threading;
using System.Windows.Input;
using FramePFX.Core;
using FramePFX.Timeline;
using FramePFX.Utils;

namespace FramePFX {
    public class MainViewModel : BaseViewModel {
        public TimelineViewModel Timeline { get; }

        private volatile bool isPlaying;
        public bool IsPlaying {
            get => this.isPlaying;
            set => this.RaisePropertyChanged(ref this.isPlaying, value);
        }

        public ICommand PlayPauseCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }

        private readonly Thread playbackThread;
        private long nextPlaybackTick;
        public volatile bool isPlaybackThreadRunning;

        public MainViewModel() {
            this.Timeline = new TimelineViewModel();
            this.playbackThread = new Thread(this.OnTickPlayback);
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
            this.playbackThread.Start();
        }

        public void OnTickPlayback() {
            while (this.isPlaybackThreadRunning) {
                if (this.IsPlaying) {
                    long time = Time.GetSystemMillis();
                    if (time >= this.nextPlaybackTick || (time + 1) >= this.nextPlaybackTick) {
                        this.nextPlaybackTick = Time.GetSystemMillis() + 33L; // 33ms = 30fps
                        if (this.Timeline.PlayHeadFrame == (this.Timeline.MaxDuration - 1)) {
                            this.Timeline.PlayHeadFrame = 0;
                        }
                        else {
                            this.Timeline.PlayHeadFrame++;
                        }
                    }

                    Thread.Sleep(1);
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
