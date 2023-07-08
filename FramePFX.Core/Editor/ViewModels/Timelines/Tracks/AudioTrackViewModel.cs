using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timelines.Tracks;
using FramePFX.Core.History;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Tracks {
    public class AudioTrackViewModel : TrackViewModel {
        public const string VolumeHistoryKey = "audio-track.Volume";
        private HistoryAudioTrackVolume volumeHistory;

        public new AudioTrack Model => (AudioTrack) base.Model;

        public float Volume {
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

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, AudioTrack.VolumeKey)) {
                    this.AutomationData[AudioTrack.VolumeKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame).SetFloatValue(value);
                }
                else {
                    this.AutomationData[AudioTrack.VolumeKey].GetOverride().SetFloatValue(value);
                    this.AutomationData[AudioTrack.VolumeKey].RaiseOverrideValueChanged();
                }
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

                TimelineViewModel timeline = this.Timeline;
                if (TimelineUtilCore.CanAddKeyFrame(timeline, this, AudioTrack.IsMutedKey)) {
                    this.AutomationData[AudioTrack.IsMutedKey].GetActiveKeyFrameOrCreateNew(timeline.PlayHeadFrame).SetBooleanValue(value);
                }
                else {
                    this.AutomationData[AudioTrack.IsMutedKey].GetOverride().SetBooleanValue(value);
                    this.AutomationData[AudioTrack.IsMutedKey].RaiseOverrideValueChanged();
                }
            }
        }

        public AudioTrackViewModel(TimelineViewModel timeline, AudioTrack model) : base(timeline, model) {
            this.AutomationData.AssignRefreshHandler(AudioTrack.VolumeKey, (s, e) => this.OnAutomationPropertyUpdated(nameof(this.Volume), in e));
            this.AutomationData.AssignRefreshHandler(AudioTrack.IsMutedKey, (s, e) => this.OnAutomationPropertyUpdated(nameof(this.IsMuted), in e));
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
            public Transaction<float> Volume { get; }

            public HistoryAudioTrackVolume(AudioTrackViewModel holder, float newValue) : base(holder) {
                this.Volume = new Transaction<float>(holder.Volume, newValue);
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