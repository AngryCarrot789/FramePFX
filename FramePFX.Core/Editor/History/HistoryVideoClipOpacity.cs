using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines.Clips;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryVideoClipOpacity : IHistoryAction {
        public VideoClipViewModel Clip { get; }
        public Transaction<double> Opacity { get; }

        public HistoryVideoClipOpacity(VideoClipViewModel clip) {
            this.Clip = clip;
            this.Opacity = Transactions.ImmutableType(clip.Opacity);
        }

        public async Task UndoAsync() {
            try {
                this.Clip.IsHistoryChanging = true;
                this.Clip.Opacity = this.Opacity.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            try {
                this.Clip.IsHistoryChanging = true;
                this.Clip.Opacity = this.Opacity.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}