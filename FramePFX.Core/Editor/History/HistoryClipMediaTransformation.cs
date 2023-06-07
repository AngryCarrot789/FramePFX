using System.Numerics;
using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryClipMediaTransformation : IHistoryAction {
        public VideoClipViewModel Clip { get; }
        public Transaction<Vector2> MediaPosition { get; }
        public Transaction<Vector2> MediaScale { get; }
        public Transaction<Vector2> MediaScaleOrigin { get; }

        public HistoryClipMediaTransformation(VideoClipViewModel clip) {
            this.Clip = clip;
            this.MediaPosition = Transactions.Immutable(clip.MediaPosition);
            this.MediaScale = Transactions.Immutable(clip.MediaScale);
            this.MediaScaleOrigin = Transactions.Immutable(clip.MediaScaleOrigin);
        }

        public async Task UndoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.MediaPosition = this.MediaPosition.Original;
                this.Clip.MediaScale = this.MediaScale.Original;
                this.Clip.MediaScaleOrigin = this.MediaScaleOrigin.Original;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            this.Clip.IsHistoryChanging = true;
            try {
                this.Clip.MediaPosition = this.MediaPosition.Current;
                this.Clip.MediaScale = this.MediaScale.Current;
                this.Clip.MediaScaleOrigin = this.MediaScaleOrigin.Current;
            }
            finally {
                this.Clip.IsHistoryChanging = false;
            }
        }
    }
}