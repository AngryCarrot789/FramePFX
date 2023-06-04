using System;
using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.ResourceManaging.ViewModels;
using FramePFX.Core.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.ResourceManaging {
    public class ResourceTypeRegistry : ModelRegistry<ResourceItem, ResourceItemViewModel> {
        public static ResourceTypeRegistry Instance { get; } = new ResourceTypeRegistry();

        private ResourceTypeRegistry() {
            base.Register<ResourceARGB, ResourceARGBViewModel>("resource_argb");
            base.Register<ResourceImage, ResourceImageViewModel>("resource_image");
            base.Register<ResourceMedia, ResourceMediaViewModel>("resource_media");
            base.Register<ResourceText, ResourceTextViewModel>("resource_argb");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : ResourceItem where TViewModel : ResourceItemViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public ResourceItem CreateResourceModel(ResourceManager manager, string id) {
            return (ResourceItem) Activator.CreateInstance(base.GetModelType(id), manager);
        }

        public ResourceItemViewModel CreateResourceViewModel(ResourceManagerViewModel manager, string id) {
            return (ResourceItemViewModel) Activator.CreateInstance(base.GetViewModelType(id), manager, this.CreateResourceModel(manager.Model, id));
        }

        public ResourceItemViewModel CreateViewModelFromModel(ResourceManagerViewModel manager, ResourceItem item) {
            return (ResourceItemViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(item), manager, item);
        }
    }
}