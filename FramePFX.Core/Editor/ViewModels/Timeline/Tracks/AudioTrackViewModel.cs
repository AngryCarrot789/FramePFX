using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline.Tracks;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Tracks {
    public class AudioTrackViewModel : TrackViewModel {
        public const string VolumeHistoryKey = "audio-track.Volume";
        private HistoryAudioTrackVolume volumeHistory;

        public new AudioTrackModel Model => (AudioTrackModel) base.Model;

        public double Volume {
            get => this.Model.Volume;
            set {
                if (!this.IsHistoryChanging) {
                    if (FrontEndHistoryHelper.ActiveDragId == VolumeHistoryKey) {
                        if (this.volumeHistory == null)
                            this.volumeHistory = new HistoryAudioTrackVolume(this, value);
                        FrontEndHistoryHelper.OnDragEnd = FrontEndHistoryHelper.OnDragEnd ?? ((s, cancel) => {
                            if (cancel) {
                                this.IsHistoryChanging = true;
                                this.Volume = this.volumeHistory.Volume.Original;
                                this.IsHistoryChanging = false;
                            }
                            else {
                                this.HistoryManager.AddAction(this.volumeHistory, "Edit volume");
                            }

                            this.volumeHistory = null;
                        });
                    }
                    else {
                        this.HistoryManager.AddAction(new HistoryAudioTrackVolume(this, value), "Edit volume");
                    }
                }

                this.AutomationData[AudioTrackModel.OpacityKey].GetOverride().SetDoubleValue(value);
                this.Model.Volume = value;
                this.RaisePropertyChanged();
                this.Timeline.DoRender(true);
            }
        }

        public bool IsMuted {
            get => this.Model.IsMuted;
            set {
                if (this.IsMuted == value) {
                    return;
                }

                if (this.IsAutomationRefreshInProgress) {
                    Debugger.Break();
                    return;
                }

                if (!this.IsHistoryChanging) {
                    this.HistoryManager.AddAction(new HistoryAudioTrackIsMuted(this, value), "Switch IsMuted");
                }

                this.AutomationData[AudioTrackModel.IsMutedKey].GetOverride().SetBooleanValue(value);
                this.Model.IsMuted = value;
                this.RaisePropertyChanged();
                this.Timeline.DoRender(true);
            }
        }

        public AudioTrackViewModel(TimelineViewModel timeline, AudioTrackModel model) : base(timeline, model) {

        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return false;
        }

        public override async Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin) {
            await IoC.MessageDialogs.ShowMessageAsync("Audio unsupported", "Cannot drop audio yet");
        }

        private class HistoryAudioTrackIsMuted : BaseHistoryHolderAction<AudioTrackViewModel> {
            public Transaction<bool> IsMuted { get; }

            public HistoryAudioTrackIsMuted(AudioTrackViewModel holder, bool newValue) : base(holder) {
                this.IsMuted = new Transaction<bool>(holder.IsMuted, newValue);
            }

            protected override Task UndoAsyncCore() {
                this.Holder.IsMuted = this.IsMuted.Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore() {
                this.Holder.IsMuted = this.IsMuted.Current;
                return Task.CompletedTask;
            }
        }

        private class HistoryAudioTrackVolume : BaseHistoryHolderAction<AudioTrackViewModel> {
            public Transaction<double> Volume { get; }

            public HistoryAudioTrackVolume(AudioTrackViewModel holder, double newValue) : base(holder) {
                this.Volume = new Transaction<double>(holder.Volume, newValue);
            }

            protected override Task UndoAsyncCore() {
                this.Holder.Volume = this.Volume.Original;
                return Task.CompletedTask;
            }

            protected override Task RedoAsyncCore() {
                this.Holder.Volume = this.Volume.Current;
                return Task.CompletedTask;
            }
        }
    }
}