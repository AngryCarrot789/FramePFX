using FramePFX.Core.PropertyPages;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips.Pages {
    public class BaseClipPropertyPageViewModel : PropertyPageViewModel<ClipViewModel> {
        public string PageName { get; }

        public BaseClipPropertyPageViewModel(ClipViewModel target, string pageName) : base(target) {
            this.PageName = pageName;
        }
    }
}