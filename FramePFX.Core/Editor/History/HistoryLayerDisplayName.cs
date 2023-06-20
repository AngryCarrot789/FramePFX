using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryLayerDisplayName : IHistoryAction {
        public LayerViewModel Layer { get; }
        public Transaction<string> DisplayName { get; }

        public HistoryLayerDisplayName(LayerViewModel layer) {
            this.Layer = layer;
            this.DisplayName = Transactions.ImmutableType(layer.DisplayName);
        }

        public async Task UndoAsync() {
            try {
                this.Layer.IsHistoryChanging = true;
                this.Layer.DisplayName = this.DisplayName.Original;
            }
            finally {
                this.Layer.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            try {
                this.Layer.IsHistoryChanging = true;
                this.Layer.DisplayName = this.DisplayName.Current;
            }
            finally {
                this.Layer.IsHistoryChanging = false;
            }
        }

        public void OnRemoved() {

        }
    }
}