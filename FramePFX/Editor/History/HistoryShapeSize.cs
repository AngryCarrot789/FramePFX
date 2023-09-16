using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines.VideoClips;
using FramePFX.History;

namespace FramePFX.Editor.History {
    public class HistoryShapeSize : HistoryAction {
        public ShapeClipViewModel Clip { get; }
        public Transaction<float> Width { get; }
        public Transaction<float> Height { get; }

        public HistoryShapeSize(ShapeClipViewModel clip) {
            this.Clip = clip;
            this.Width = Transactions.ImmutableType(clip.Width);
            this.Height = Transactions.ImmutableType(clip.Height);
        }

        protected override async Task UndoAsyncCore() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Width = this.Width.Original;
                this.Clip.Height = this.Height.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        protected override async Task RedoAsyncCore() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Width = this.Width.Current;
                this.Clip.Height = this.Height.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public override void OnRemoved() {
        }
    }
}