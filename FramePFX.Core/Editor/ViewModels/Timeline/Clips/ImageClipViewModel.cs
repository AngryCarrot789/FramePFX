using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.ResourceManaging.Resources;
using FramePFX.Core.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : BaseResourceVideoClipViewModel<ResourceImage, ResourceImageViewModel> {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        public ImageClipViewModel(ImageClipModel model) : base(model) {
            model.ResourceStateChanged += this.OnResourceStateChanged;
        }

        private void OnResourceStateChanged(object clip) { // ImageClipModel clip
            this.RaisePropertyChanged(nameof(this.IsOffline));
        }
    }
}