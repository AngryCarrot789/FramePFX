using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.PropertyPages;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Pages {
    public class ResourcePageFactory : PropertyPageFactory<BaseResourceObjectViewModel, BaseResourcePropertyPageViewModel> {
        public static ResourcePageFactory Instance { get; } = new ResourcePageFactory();

        private ResourcePageFactory() {
            this.RegisterPage<BaseResourceObjectViewModel, BaseResourcePageViewModel>();
            this.RegisterPage<ResourceItemViewModel, ResourceItemPageViewModel>();
            this.RegisterPage<ResourceColourViewModel, ColourResourcePageViewModel>();
        }
    }
}