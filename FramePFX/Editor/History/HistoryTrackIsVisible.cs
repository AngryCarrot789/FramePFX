using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.History;

namespace FramePFX.Editor.History {
    public class HistoryTrackIsVisible : BaseHistoryHolderAction<VideoTrackViewModel> {
        public Transaction<bool> IsVisible { get; }

        public HistoryTrackIsVisible(VideoTrackViewModel track, bool newValue) : base(track) {
            this.IsVisible = new Transaction<bool>(track.IsVisible, newValue);
        }

        protected override Task UndoAsyncForHolder() {
            this.Holder.IsVisible = this.IsVisible.Original;
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncForHolder() {
            this.Holder.IsVisible = this.IsVisible.Current;
            return Task.CompletedTask;
        }
    }
}