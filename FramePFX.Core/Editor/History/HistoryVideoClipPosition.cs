using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.History;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.History {
    public class HistoryVideoClipPosition : IHistoryAction {
        public ClipViewModel Clip { get; }
        public Transaction<FrameSpan> Span { get; }
        public Transaction<long> MediaFrameOffset { get; }

        public HistoryVideoClipPosition(ClipViewModel clip) {
            this.Clip = clip;
            this.Span = Transactions.ImmutableType(clip.FrameSpan);
            this.MediaFrameOffset = Transactions.ImmutableType(clip.MediaFrameOffset);
        }

        public void Undo() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.FrameSpan = this.Span.Original;
                this.Clip.MediaFrameOffset = this.MediaFrameOffset.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task UndoAsync() {
            this.Undo();
        }

        public async Task RedoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.FrameSpan = this.Span.Current;
                this.Clip.MediaFrameOffset = this.MediaFrameOffset.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {
        }
    }
}