using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryTrackOpacity : BaseHistoryHolderAction<VideoTrackViewModel> {
        public Transaction<double> Opacity { get; }

        public HistoryTrackOpacity(VideoTrackViewModel track) : base(track) {
            this.Opacity = Transactions.ImmutableType(track.Opacity);
        }

        protected override Task UndoAsyncCore() {
            this.Holder.Opacity = this.Opacity.Original;
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore() {
            this.Holder.Opacity = this.Opacity.Current;
            return Task.CompletedTask;
        }
    }
}