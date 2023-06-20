using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryLayerOpacity : BaseHistoryHolderAction<VideoLayerViewModel> {
        public Transaction<double> Opacity { get; }

        public HistoryLayerOpacity(VideoLayerViewModel layer) : base(layer) {
            this.Opacity = Transactions.ImmutableType(layer.Opacity);
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