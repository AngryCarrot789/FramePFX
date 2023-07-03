using System;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.Registries {
    /// <summary>
    /// The registry for resource items (including the resource group)
    /// </summary>
    public class ResourceTypeRegistry : ModelRegistry<BaseResourceObject, BaseResourceObjectViewModel> {
        public static ResourceTypeRegistry Instance { get; } = new ResourceTypeRegistry();

        private ResourceTypeRegistry() {
            base.Register<ResourceGroup, ResourceGroupViewModel>("r_group");
            base.Register<ResourceColour, ResourceColourViewModel>("r_argb");
            base.Register<ResourceImage, ResourceImageViewModel>("r_img");
            base.Register<ResourceOldMedia, ResourceOldMediaViewModel>("r_media_old");
            base.Register<ResourceMpegMedia, ResourceMpegMediaViewModel>("r_media");
            base.Register<ResourceText, ResourceTextViewModel>("r_txt");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseResourceObject where TViewModel : BaseResourceObjectViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public BaseResourceObject CreateResourceItemModel(string id) {
            return (BaseResourceObject) Activator.CreateInstance(base.GetModelType(id));
        }

        public BaseResourceObjectViewModel CreateResourceItemViewModel(string id) {
            return (BaseResourceObjectViewModel) Activator.CreateInstance(base.GetViewModelType(id), this.CreateResourceItemModel(id));
        }

        public BaseResourceObjectViewModel CreateItemViewModelFromModel(BaseResourceObject item) {
            return (BaseResourceObjectViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(item), item);
        }
    }
}