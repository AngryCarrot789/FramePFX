using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryClipDisplayName : IHistoryAction {
        public ClipViewModel Clip { get; }
        public Transaction<string> DisplayName { get; }

        public HistoryClipDisplayName(ClipViewModel clip) {
            this.Clip = clip;
            this.DisplayName = Transactions.ImmutableType(clip.DisplayName);
        }

        public async Task UndoAsync() {
            try {
                this.Clip.IsHistoryChanging = true;
                this.Clip.DisplayName = this.DisplayName.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            try {
                this.Clip.IsHistoryChanging = true;
                this.Clip.DisplayName = this.DisplayName.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}