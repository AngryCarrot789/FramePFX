using FramePFX.Editor.ResourceManaging;
using FramePFX.Editor.ResourceManaging.Resources;
using FramePFX.Editor.ResourceManaging.ViewModels;
using FramePFX.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Editor.Registries
{
    /// <summary>
    /// The registry for resource items (including the resource folder)
    /// </summary>
    public class ResourceTypeFactory : ModelFactory<BaseResource, BaseResourceViewModel>
    {
        public static ResourceTypeFactory Instance { get; } = new ResourceTypeFactory();

        private ResourceTypeFactory()
        {
            base.Register<ResourceFolder, ResourceFolderViewModel>("r_group");
            base.Register<ResourceColour, ResourceColourViewModel>("r_argb");
            base.Register<ResourceImage, ResourceImageViewModel>("r_img");
            base.Register<ResourceAVMedia, ResourceAVMediaViewModel>("r_av_media");
            base.Register<ResourceMpegMedia, ResourceMpegMediaViewModel>("r_media");
            base.Register<ResourceTextStyle, ResourceTextStyleViewModel>("r_txt");
            base.Register<ResourceTextFile, ResourceTextFileViewModel>("r_txtfile");
            base.Register<ResourceComposition, ResourceCompositionViewModel>("r_comp");
        }

        public new void Register<TModel, TViewModel>(string id) where TModel : BaseResource where TViewModel : BaseResourceViewModel
        {
            base.Register<TModel, TViewModel>(id);
        }

        public new BaseResource CreateModel(string id) => base.CreateModel(id);

        public new BaseResourceViewModel CreateViewModel(string id) => base.CreateViewModel(id);

        public new BaseResourceViewModel CreateViewModelFromModel(BaseResource item) => base.CreateViewModelFromModel(item);
    }
}