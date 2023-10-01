using System.Threading.Tasks;
using FramePFX.Editor.ViewModels.Timelines.Tracks;
using FramePFX.History;

namespace FramePFX.Editor.History
{
    public class HistoryTrackOpacity : BaseHistoryHolderAction<VideoTrackViewModel>
    {
        public Transaction<double> Opacity { get; }

        public HistoryTrackOpacity(VideoTrackViewModel track) : base(track)
        {
            this.Opacity = Transactions.ImmutableType(track.Opacity);
        }

        protected override Task UndoAsyncForHolder()
        {
            this.Holder.Opacity = this.Opacity.Original;
            return Task.CompletedTask;
        }

        protected override Task RedoAsyncForHolder()
        {
            this.Holder.Opacity = this.Opacity.Current;
            return Task.CompletedTask;
        }
    }
}