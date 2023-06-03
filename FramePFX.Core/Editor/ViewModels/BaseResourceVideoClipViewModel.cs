using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.ResourceManaging;
using FramePFX.Core.ResourceManaging.ViewModels;

namespace FramePFX.Core.Editor.ViewModels {
    public class BaseResourceVideoClipViewModel<TModel, TViewModel> : VideoClipViewModel where TModel : ResourceItem where TViewModel : ResourceItemViewModel {
        public new BaseResourceVideoClip<TModel> Model => (BaseResourceVideoClip<TModel>) ((ClipViewModel) this).Model;

        public string ImageResourceId {
            get => this.Model.ImageResourceId;
            set {
                this.Model.ImageResourceId = value;
                this.RaisePropertyChanged();
            }
        }

        public bool IsOffline {
            get => this.Model.IsResourceOffline;
            set => this.Model.IsResourceOffline = value;
        }

        public BaseResourceVideoClipViewModel(BaseResourceVideoClip<TModel> model) : base(model) {
            model.ResourceStateChanged += this.OnResourceStateChanged;
        }

        private void OnResourceStateChanged(object clip) { // ImageClipModel clip
            this.RaisePropertyChanged(nameof(this.IsOffline));
        }
    }
}