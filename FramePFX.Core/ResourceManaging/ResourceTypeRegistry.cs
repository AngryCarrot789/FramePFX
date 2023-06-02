using System;
using FramePFX.Core.ResourceManaging.ViewModels;

namespace FramePFX.Core.ResourceManaging {
    public class ResourceTypeRegistry : ModelRegistry<ResourceItem, ResourceItemViewModel> {
        public static ResourceTypeRegistry Instance { get; } = new ResourceTypeRegistry();

        private ResourceTypeRegistry() {
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : ResourceItem where TViewModel : ResourceItemViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public ResourceItem CreateLayerModel(string id) {
            return (ResourceItem) Activator.CreateInstance(base.GetModelType(id));
        }

        public ResourceItemViewModel CreateLayerViewModel(string id) {
            return (ResourceItemViewModel) Activator.CreateInstance(base.GetViewModelType(id));
        }

        public ResourceItemViewModel CreateViewModelFromModel(ResourceItem item) {
            return (ResourceItemViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(item), item);
        }
    }
}