using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.ResourceManaging.ViewModels.Resources;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ImageClipViewModel : VideoClipViewModel {
        public new ImageClipModel Model => (ImageClipModel) ((ClipViewModel) this).Model;

        private ImageResourceViewModel resource;
        public ImageResourceViewModel Resource {
            get => this.resource;
            private set {
                this.Model.Resource = value?.Model;
                this.RaisePropertyChanged(ref this.resource, value);
            }
        }

        public ImageClipViewModel(ImageClipModel model) : base(model) {

        }

        public void SetResource(ImageResourceViewModel resource) {
            this.Resource?.Dispose();
            this.Resource = resource;
        }
    }
}