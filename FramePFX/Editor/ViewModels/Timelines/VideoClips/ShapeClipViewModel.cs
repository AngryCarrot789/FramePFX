using FramePFX.Editor.History;
using FramePFX.Editor.Timelines.VideoClips;
using FramePFX.History.Tasks;
using FramePFX.History.ViewModels;

namespace FramePFX.Editor.ViewModels.Timelines.VideoClips {
    public class ShapeClipViewModel : VideoClipViewModel {
        private readonly HistoryBuffer<HistoryShapeSize> sizeHistory = new HistoryBuffer<HistoryShapeSize>();

        public new ShapeVideoClip Model => (ShapeVideoClip) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryShapeSize(this), "Edit shape size");
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
                if (!this.IsHistoryChanging && this.Track != null) {
                    if (!this.sizeHistory.TryGetAction(out HistoryShapeSize action))
                        this.sizeHistory.PushAction(HistoryManagerViewModel.Instance, action = new HistoryShapeSize(this), "Edit shape size");
                    action.Height.SetCurrent(value);
                }

                this.Model.Height = value;
                this.RaisePropertyChanged();
                this.Model.InvalidateRender();
            }
        }

        public ShapeClipViewModel(ShapeVideoClip model) : base(model) {
        }
    }
}