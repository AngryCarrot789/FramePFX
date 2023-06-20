using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryLayerIsVisible : BaseHistoryHolderAction<VideoLayerViewModel> {
        public Transaction<bool> IsVisible { get; }

        public HistoryLayerIsVisible(VideoLayerViewModel layer, bool newValue) : base(layer) {
            this.IsVisible = new Transaction<bool>(layer.IsVisible, newValue);
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