using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.History;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.History {
    public class HistoryVideoClipPosition : IHistoryAction {
        public VideoClipViewModel Clip { get; }
        public Transaction<ClipSpan> Span { get; }
        public Transaction<long> MediaFrameOffset { get; }

        public HistoryVideoClipPosition(VideoClipViewModel clip) {
            this.Clip = clip;
            this.Span = Transactions.Immutable(clip.Span);
            this.MediaFrameOffset = Transactions.Immutable(clip.MediaFrameOffset);
        }

        public async Task UndoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Span = this.Span.Original;
                this.Clip.MediaFrameOffset = this.MediaFrameOffset.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.Span = this.Span.Current;
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