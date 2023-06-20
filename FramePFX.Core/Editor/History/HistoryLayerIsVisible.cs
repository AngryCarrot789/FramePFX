using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryLayerOpacity : IHistoryAction {
        public VideoLayerViewModel Layer { get; }
        public Transaction<double> Opacity { get; }

        public HistoryLayerOpacity(VideoLayerViewModel layer) {
            this.Layer = layer;
            this.Opacity = Transactions.ImmutableType(layer.Opacity);
        }

        public async Task UndoAsync() {
            try {
                this.Layer.IsHistoryChanging = true;
                this.Layer.Opacity = this.Opacity.Original;
            }
            finally {
                this.Layer.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            try {
                this.Layer.IsHistoryChanging = true;
                this.Layer.Opacity = this.Opacity.Current;
            }
            finally {
                this.Layer.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}