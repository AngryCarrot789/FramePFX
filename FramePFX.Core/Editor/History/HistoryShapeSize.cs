using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryShapeSize : IHistoryAction {
        public ShapeClipViewModel Clip { get; }
        public Transaction<float> Width { get; }
        public Transaction<float> Height { get; }

        public HistoryShapeSize(ShapeClipViewModel clip) {
            this.Clip = clip;
            this.Width = Transactions.ImmutableType(clip.Width);
            this.Height = Transactions.ImmutableType(clip.Height);
        }

        public async Task UndoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Width = this.Width.Original;
                this.Clip.Height = this.Height.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Width = this.Width.Current;
                this.Clip.Height = this.Height.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}