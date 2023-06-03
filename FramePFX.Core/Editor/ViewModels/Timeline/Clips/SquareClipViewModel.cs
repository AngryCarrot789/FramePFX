using FramePFX.Core.Editor.Timeline.Clip;

namespace FramePFX.Core.Editor.ViewModels.Timeline.Clips {
    public class SquareClipViewModel : VideoClipViewModel {
        public new SquareClipModel Model => (SquareClipModel) base.Model;

        public float Width {
            get => this.Model.Width;
            set {
                this.Model.Width = value;
                this.RaisePropertyChanged();
                this.OnInvalidateRender();
            }
        }

        public float Height {
            get => this.Model.Height;
            set {
                this.Model.Height = value;
                this.RaisePropertyChanged();
                this.OnInvalidateRender();
            }
        }

        public SquareClipViewModel(SquareClipModel model) : base(model) {

        }
    }
}