using FramePFX.Editors.Timelines.Clips;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.PropertyEditors.Clips {
    public class VideoClipMediaFrameOffsetPropertyEditorSlot : PropertyEditorSlot {
        public override bool IsSelectable => false;

        public override HandlerCountMode HandlerCountMode => HandlerCountMode.Single;

        public VideoClip SingleSelection => this.Handlers.Count == 1 ? ((VideoClip) this.Handlers[0]) : null;

        public long MediaFrameOffset => this.SingleSelection?.MediaFrameOffset ?? 0L;

        public event PropertyEditorSlotEventHandler UpdateMediaFrameOffset;

        public VideoClipMediaFrameOffsetPropertyEditorSlot() : base(typeof(VideoClip)) {

        }

        protected override void OnHandlersLoaded() {
            base.OnHandlersLoaded();
            this.SingleSelection.MediaFrameOffsetChanged += this.OnMediaFrameOffsetChanged;
            this.UpdateMediaFrameOffset?.Invoke(this);
        }

        protected override void OnClearingHandlers() {
            base.OnClearingHandlers();
            this.SingleSelection.MediaFrameOffsetChanged -= this.OnMediaFrameOffsetChanged;
        }

        private void OnMediaFrameOffsetChanged(Clip clip, long oldoffset, long newoffset) {
            this.UpdateMediaFrameOffset?.Invoke(this);
        }
    }
}