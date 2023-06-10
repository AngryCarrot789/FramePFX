using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging;
using FramePFX.Core.Editor.ResourceManaging.Resources;
using FramePFX.Core.Editor.Timeline.Clip;
using FramePFX.Core.History.Tasks;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class ShapeClipViewModel : VideoClipViewModel, IDropClipResource {
        private readonly HistoryBuffer<HistoryShapeSize> sizeHistory = new HistoryBuffer<HistoryShapeSize>();

        public new ShapeClipModel Model => (ShapeClipModel) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(this.HistoryManager, action = new HistoryShapeSize(this), "Edit shape size");
                    action.Width.SetCurrent(value);
                }

                this.Model.Width = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public float Height {
            get => this.Model.Height;
            set {
                if (!this.IsHistoryChanging && this.Layer != null) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(this.HistoryManager, action = new HistoryShapeSize(this), "Edit shape size");
                    action.Height.SetCurrent(value);
                }

                this.Model.Height = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public ShapeClipViewModel(ShapeClipModel model) : base(model) {

        }

        public override bool CanDropResource(ResourceItem resource) {
            return resource is ResourceColour;
        }

        public override async Task OnDropResource(ResourceItem resource) {
            this.Model.SetTargetResourceId(((ResourceColour) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}