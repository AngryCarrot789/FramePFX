using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines;
using FramePFX.History;

namespace FramePFX.Editor.History {
    public class HistoryTrackDisplayName : HistoryAction {
        public TrackViewModel Track { get; }
        public Transaction<string> DisplayName { get; }

        public HistoryTrackDisplayName(TrackViewModel track) {
            this.Track = track;
            this.DisplayName = Transactions.ImmutableType(track.DisplayName);
        }

        protected override Task UndoAsyncCore() {
            try {
                this.Track.IsHistoryChanging = true;
                this.Track.DisplayName = this.DisplayName.Original;
            }
            finally {
                this.Track.IsHistoryChanging = false;
            }

            return Task.CompletedTask;
        }

        protected override Task RedoAsyncCore() {
            try {
                this.Track.IsHistoryChanging = true;
                this.Track.DisplayName = this.DisplayName.Current;
            }
            finally {
                this.Track.IsHistoryChanging = false;
            }

            return Task.CompletedTask;
        }

        public override void OnRemoved() {
        }
    }
}