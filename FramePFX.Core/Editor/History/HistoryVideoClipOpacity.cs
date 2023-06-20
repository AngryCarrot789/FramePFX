using System.Threading.Tasks;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Clips;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    public class HistoryVideoClipOpacity : IHistoryAction {
        public VideoClipViewModel Layer { get; }
        public Transaction<double> Opacity { get; }

        public HistoryVideoClipOpacity(VideoClipViewModel layer) {
            this.Layer = layer;
            this.Opacity = Transactions.Immutable(layer.Opacity);
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