using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryTrackDisplayName : IHistoryAction {
        public TrackViewModel Track { get; }
        public Transaction<string> DisplayName { get; }

        public HistoryTrackDisplayName(TrackViewModel track) {
            this.Track = track;
            this.DisplayName = Transactions.ImmutableType(track.DisplayName);
        }

        public async Task UndoAsync() {
            try {
                this.Track.IsHistoryChanging = true;
                this.Track.DisplayName = this.DisplayName.Original;
            }
            finally {
                this.Track.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            try {
                this.Track.IsHistoryChanging = true;
                this.Track.DisplayName = this.DisplayName.Current;
            }
            finally {
                this.Track.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}