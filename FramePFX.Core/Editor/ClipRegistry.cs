using System;
using FramePFX.Core.Editor.Timeline;
using FramePFX.Core.Editor.Timeline.Layers;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;

namespace FramePFX.Core.Editor {
    public class LayerRegistry : ModelRegistry<TimelineLayerModel, TimelineLayerViewModel> {
        public static LayerRegistry Instance { get; } = new LayerRegistry();

        private LayerRegistry() {
            this.Register<VideoLayerModel, VideoLayerViewModel>("video_layer");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : TimelineLayerModel where TViewModel : TimelineLayerViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public TimelineLayerModel CreateLayerModel(TimelineModel timeline, string id) {
            return (TimelineLayerModel) Activator.CreateInstance(base.GetModelType(id), timeline);
        }

        public TimelineLayerViewModel CreateLayerViewModel(TimelineViewModel timeline, string id) {
            return (TimelineLayerViewModel) Activator.CreateInstance(base.GetViewModelType(id), timeline);
        }

        public TimelineLayerViewModel CreateViewModelFromModel(TimelineViewModel timeline, TimelineLayerModel model) {
            if (!ReferenceEquals(timeline.Model, model.Timeline)) {
                throw new ArgumentException("Timeline models do not match");
            }

            return (TimelineLayerViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(model), timeline);
        }
    }
}