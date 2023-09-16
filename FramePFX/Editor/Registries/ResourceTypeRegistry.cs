using System;
using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Editor.Registries {
    /// <summary>
    /// The registry for resource items (including the resource group)
    /// </summary>
    public class ResourceTypeRegistry : ModelRegistry<BaseResourceObject, BaseResourceObjectViewModel> {
        public static ResourceTypeRegistry Instance { get; } = new ResourceTypeRegistry();

        private ResourceTypeRegistry() {
            base.Register<ResourceGroup, ResourceGroupViewModel>("r_group");
            base.Register<ResourceColour, ResourceColourViewModel>("r_argb");
            base.Register<ResourceImage, ResourceImageViewModel>("r_img");
            base.Register<ResourceAVMedia, ResourceAVMediaViewModel>("r_av_media");
            base.Register<ResourceMpegMedia, ResourceMpegMediaViewModel>("r_media");
            base.Register<ResourceText, ResourceTextViewModel>("r_txt");
            base.Register<ResourceTextFile, ResourceTextFileViewModel>("r_txtfile");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseResourceObject where TViewModel : BaseResourceObjectViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public BaseResourceObject CreateModel(string id) {
            return (BaseResourceObject) Activator.CreateInstance(base.GetModelType(id));
        }

        public BaseResourceObjectViewModel CreateViewModel(string id) {
            return (BaseResourceObjectViewModel) Activator.CreateInstance(base.GetViewModelType(id), this.CreateModel(id));
        }

        public BaseResourceObjectViewModel CreateViewModelFromModel(BaseResourceObject item) {
            return (BaseResourceObjectViewModel) Activator.CreateInstance(base.GetViewModelTypeFromModel(item), item);
        }
    }
}