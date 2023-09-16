using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;
using FramePFX.Utils;

namespace FramePFX.Editor.History {
    public class HistoryVideoClipPosition : HistoryAction {
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

        protected override async Task UndoAsyncCore() {
            this.Undo();
        }

        protected override async Task RedoAsyncCore() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.FrameSpan = this.Span.Current;
                this.Clip.MediaFrameOffset = this.MediaFrameOffset.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public override void OnRemoved() {
        }
    }
}