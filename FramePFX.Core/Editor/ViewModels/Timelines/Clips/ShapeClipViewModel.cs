using System.Threading.Tasks;
using FramePFX.Core.Editor.History;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ResourceManaging.ViewModels.Resources;
using FramePFX.Core.Editor.Timelines.VideoClips;
using FramePFX.Core.History.Tasks;
using FramePFX.Core.History.ViewModels;

namespace FramePFX.Core.Editor.ViewModels.Timelines.Clips {
    public class ShapeClipViewModel : VideoClipViewModel, IAcceptResourceDrop {
        private readonly HistoryBuffer<HistoryShapeSize> sizeHistory = new HistoryBuffer<HistoryShapeSize>();

        public new ShapeVideoClip Model => (ShapeVideoClip) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                if (!this.IsHistoryChanging && this.Track != null && this.GetHistoryManager(out HistoryManagerViewModel m)) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(m, action = new HistoryShapeSize(this), "Edit shape size");
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
                if (!this.IsHistoryChanging && this.Track != null && this.GetHistoryManager(out HistoryManagerViewModel m)) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(m, action = new HistoryShapeSize(this), "Edit shape size");
                    action.Height.SetCurrent(value);
                }

                this.Model.Height = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public ShapeClipViewModel(ShapeVideoClip model) : base(model) {

        }

        public override bool CanDropResource(BaseResourceObjectViewModel resource) {
            return resource is ResourceColourViewModel;
        }

        public override async Task OnDropResource(BaseResourceObjectViewModel resource) {
            this.Model.SetTargetResourceId(((ResourceColourViewModel) resource).UniqueId);
            this.Model.InvalidateRender();
        }
    }
}