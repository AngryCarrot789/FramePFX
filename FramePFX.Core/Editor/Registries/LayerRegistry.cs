using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for layers; audio, video, etc
    /// </summary>
    public class LayerRegistry : ModelRegistry<LayerModel, LayerViewModel> {
        public static LayerRegistry Instance { get; } = new LayerRegistry();

        private LayerRegistry() {
            this.Register<VideoLayerModel, VideoLayerViewModel>("video_layer");
            this.Register<AudioLayerModel, AudioLayerViewModel>("audio_layer");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : LayerModel where TViewModel : LayerViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public LayerModel CreateLayerModel(TimelineModel timeline, string id) {
            return (LayerModel) Activator.CreateInstance(base.GetModelType(id), timeline);
        }

        public LayerViewModel CreateLayerViewModel(TimelineViewModel timeline, string id) {
            return (LayerViewModel) Activator.CreateInstance(base.GetViewModelType(id), timeline);
        }

        public LayerViewModel CreateViewModelFromModel(TimelineViewModel timeline, LayerModel model) {
            if (!ReferenceEquals(timeline.Model, model.Timeline)) {
                throw new ArgumentException("Timeline models do not match");
            }

            return (LayerViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), timeline, model);
        }
    }
}