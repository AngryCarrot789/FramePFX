using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Pages {
    public class BaseResourcePageViewModel : BaseResourcePropertyPageViewModel {
        public BaseResourcePageViewModel(BaseResourceObjectViewModel target) : base(target, "Resource Data") {
        }
    }

    public class ResourceItemPageViewModel : BaseResourcePropertyPageViewModel {
        public ResourceItemPageViewModel(ResourceItemViewModel target) : base(target, "Resource Item Data") {
        }
    }

    public class ColourResourcePageViewModel : BaseResourcePropertyPageViewModel {
        public new ResourceColourViewModel Target => (ResourceColourViewModel) base.Target;
        public ColourResourcePageViewModel(ResourceColourViewModel target) : base(target, "Colour Data") {

        }
    }
}