using FramePFX.Editor.History;

namespace FramePFX.Editor.ViewModels.Timelines.Dragging {
    public class ClipDragInfo {
        public readonly ClipViewModel clip;
        public readonly ClipDragHistoryData history;

        // Used to store excessive drag frames when trying to drag below 0
        public long accumulator;

        public ClipDragInfo(ClipViewModel clip) {
            this.clip = clip;
            this.history = new ClipDragHistoryData(clip);
        }
    }
}