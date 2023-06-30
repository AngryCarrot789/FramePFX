using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.PropertyPages;

namespace FramePFX.Core.Editor.ResourceManaging.ViewModels.Pages {
    public class BaseResourcePropertyPageViewModel : PropertyPageViewModel<BaseResourceObjectViewModel> {
        public string PageName { get; }

        public BaseResourcePropertyPageViewModel(BaseResourceObjectViewModel target, string pageName) : base(target) {
            this.PageName = pageName;
        }
    }
}