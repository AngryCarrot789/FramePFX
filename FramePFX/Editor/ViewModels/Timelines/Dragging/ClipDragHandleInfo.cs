namespace FramePFX.Editor.ViewModels.Timelines.Dragging
{
    public class ClipDragHandleInfo
    {
        public readonly ClipViewModel clip;
        public readonly ClipDragOperation operation;

        // Used to store excessive drag frames when trying to drag below 0
        public long accumulator;

        public ClipDragHandleInfo(ClipDragOperation operation, ClipViewModel clip)
        {
            this.operation = operation;
            this.clip = clip;
        }
    }
}