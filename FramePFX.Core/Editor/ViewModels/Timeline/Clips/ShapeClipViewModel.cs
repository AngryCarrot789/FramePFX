using System.Net;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class SquareClipViewModel : VideoClipViewModel, IDropClipResource {
        public new SquareClipModel Model => (SquareClipModel) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                this.Model.Width = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public float Height {
            get => this.Model.Height;
            set {
                this.Model.Height = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public SquareClipViewModel(SquareClipModel model) : base(model) {

        }

        public override bool CanDropResource(ResourceItemViewModel resource) {
            return resource is ResourceColourViewModel;
        }

        public override async Task OnDropResource(ResourceItemViewModel resource) {
            if (!(resource is ResourceColourViewModel colour)) {
                await IoC.MessageDialogs.ShowMessageAsync("Incompatible resource", $"Square clips cannot accept {resource.GetType().Name}");
                return;
            }

            this.Model.SetTargetResourceId(colour.UniqueId);
            this.OnInvalidateRender();
        }
    }
}