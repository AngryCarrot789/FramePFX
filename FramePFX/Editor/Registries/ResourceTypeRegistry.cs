using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Editor.Registries {
    /// <summary>
    /// The registry for resource items (including the resource folder)
    /// </summary>
    public class ResourceTypeRegistry : ModelRegistry<BaseResourceObject, BaseResourceViewModel> {
        public static ResourceTypeRegistry Instance { get; } = new ResourceTypeRegistry();

        private ResourceTypeRegistry() {
            base.Register<ResourceFolder, ResourceFolderViewModel>("r_group");
            base.Register<ResourceColour, ResourceColourViewModel>("r_argb");
            base.Register<ResourceImage, ResourceImageViewModel>("r_img");
            base.Register<ResourceAVMedia, ResourceAVMediaViewModel>("r_av_media");
            base.Register<ResourceMpegMedia, ResourceMpegMediaViewModel>("r_media");
            base.Register<ResourceTextStyle, ResourceTextStyleViewModel>("r_txt");
            base.Register<ResourceTextFile, ResourceTextFileViewModel>("r_txtfile");
            base.Register<ResourceCompositionSeq, ResourceCompositionViewModel>("r_comp");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseResourceObject where TViewModel : BaseResourceViewModel {
            base.Register<TModel, TViewModel>(id);
        }

        public new BaseResourceObject CreateModel(string id) => base.CreateModel(id);

        public new BaseResourceViewModel CreateViewModel(string id) => base.CreateViewModel(id);

        public new BaseResourceViewModel CreateViewModelFromModel(BaseResourceObject item) => base.CreateViewModelFromModel(item);
    }
}