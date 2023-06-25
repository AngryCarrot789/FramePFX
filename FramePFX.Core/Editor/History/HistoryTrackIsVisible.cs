using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Tracks;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryTrackIsVisible : BaseHistoryHolderAction<VideoTrackViewModel> {
        public Transaction<bool> IsVisible { get; }

        public HistoryTrackIsVisible(VideoTrackViewModel track, bool newValue) : base(track) {
            this.IsVisible = new Transaction<bool>(track.IsVisible, newValue);
        }

        protected override Task UndoAsyncCore() {
            this.Holder.IsVisible = this.IsVisible.Original;
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore() {
            this.Holder.IsVisible = this.IsVisible.Current;
            return Task.CompletedTask;
        }
    }
}