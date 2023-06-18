using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline.Layers;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Layers {
    public class AudioLayerViewModel : LayerViewModel {
        public new AudioLayerModel Model => (AudioLayerModel) base.Model;

        public float Volume {
            get => this.Model.Volume;
            set {
                this.Model.Volume = value;
                this.RaisePropertyChanged();
                this.Timeline.DoRender(true);
            }
        }

        public bool IsMuted {
            get => this.Model.IsMuted;
            set {
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
    }
}