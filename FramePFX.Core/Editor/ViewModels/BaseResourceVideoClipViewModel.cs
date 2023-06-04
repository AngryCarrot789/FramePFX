using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;

namespace FramePFX.Core.Editor.ViewModels {
    public class BaseResourceVideoClipViewModel<TModel, TViewModel> : VideoClipViewModel where TModel : ResourceItem where TViewModel : ResourceItemViewModel {
        public new BaseResourceVideoClip<TModel> Model => (BaseResourceVideoClip<TModel>) ((ClipViewModel) this).Model;

        public string ResourceId {
            get => this.Model.ResourceId;
            set {
                this.Model.ResourceId = value;
                this.RaisePropertyChanged();
            }
        }

        public bool? IsOnline {
            get => this.Model.IsResourceOnline;
            set => this.Model.IsResourceOnline = value;
        }

        public BaseResourceVideoClipViewModel(BaseResourceVideoClip<TModel> model) : base(model) {
            model.ResourceOnlineChanged += this.OnResourceOnlineChanged;
            model.ResourceRenamed += this.OnResourceRenamed;
            model.DataModified += this.OnResourceModified;
            model.ResourceRemoved += this.OnResourceRemoved;
        }

        private void OnResourceOnlineChanged(object clip) {
            this.RaisePropertyChanged(nameof(this.IsOnline));
        }

        protected virtual void OnResourceRenamed(VideoClipModel clip, string oldid, string newid) {

        }

        protected virtual void OnResourceModified(VideoClipModel clip, TModel resource, string property) {

        }

        protected virtual void OnResourceRemoved(VideoClipModel clip, TModel resource) {

        }
    }
}