using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class AudioLayerViewModel : LayerViewModel {
        public const string VolumeHistoryKey = "audio-layer.Volume";
        private HistoryAudioLayerVolume volumeHistory;

        public new AudioLayerModel Model => (AudioLayerModel) base.Model;

        public double Volume {
            get => this.Model.Volume;
            set {
                if (!this.IsHistoryChanging) {
                    if (FrontEndHistoryHelper.ActiveDragId == VolumeHistoryKey) {
                        if (this.volumeHistory == null)
                            this.volumeHistory = new HistoryAudioLayerVolume(this, value);
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
                        this.HistoryManager.AddAction(new HistoryAudioLayerVolume(this, value), "Edit volume");
                    }
                }

                this.AutomationData[AudioLayerModel.OpacityKey].GetOverride().SetDoubleValue(value);
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

                Debug.Assert(this.IsAutomationChangeInProgress == false, "IsAutomationChangeInProgress should be false");
                if (!this.IsHistoryChanging) {
                    this.HistoryManager.AddAction(new HistoryAudioLayerIsMuted(this, value), "Switch IsMuted");
                }

                this.AutomationData[AudioLayerModel.IsMutedKey].GetOverride().SetBooleanValue(value);
                this.Model.IsMuted = value;
                this.RaisePropertyChanged();
                this.Timeline.DoRender(true);
            }
        }

        public AudioLayerViewModel(TimelineViewModel timeline, AudioLayerModel model) : base(timeline, model) {

        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return false;
        }

        public override async Task OnResourceDropped(ResourceItemViewModel resource, long frameBegin) {
            await IoC.MessageDialogs.ShowMessageAsync("Audio unsupported", "Cannot drop audio yet");
        }

        private class HistoryAudioLayerIsMuted : BaseHistoryHolderAction<AudioLayerViewModel> {
            public Transaction<bool> IsMuted { get; }

            public HistoryAudioLayerIsMuted(AudioLayerViewModel holder, bool newValue) : base(holder) {
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

        private class HistoryAudioLayerVolume : BaseHistoryHolderAction<AudioLayerViewModel> {
            public Transaction<double> Volume { get; }

            public HistoryAudioLayerVolume(AudioLayerViewModel holder, double newValue) : base(holder) {
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