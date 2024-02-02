using System.Windows;
using System.Windows.Controls;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Controls;

namespace FramePFX.Editors.PropertyEditors.AControls {
    public class VideoClipMediaFrameOffsetPropertyEditorControl : BasePropEditControlContent {
        public VideoClipMediaFrameOffsetPropertyEditorSlot SlotModel => (VideoClipMediaFrameOffsetPropertyEditorSlot) base.SlotControl.Model;

        private TextBlock textBlock;

        public VideoClipMediaFrameOffsetPropertyEditorControl() {
        }

        static VideoClipMediaFrameOffsetPropertyEditorControl() => DefaultStyleKeyProperty.OverrideMetadata(typeof(VideoClipMediaFrameOffsetPropertyEditorControl), new FrameworkPropertyMetadata(typeof(VideoClipMediaFrameOffsetPropertyEditorControl)));

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.textBlock = this.GetTemplateChild<TextBlock>("PART_TextBlock");
        }

        private void SlotModelOnUpdateMediaFrameOffset(PropertyEditorSlot sender) {
            if (this.textBlock != null)
                this.textBlock.Text = this.SlotModel.MediaFrameOffset.ToString();
        }

        protected override void OnConnected() {
            this.SlotModel.UpdateMediaFrameOffset += this.SlotModelOnUpdateMediaFrameOffset;
        }

        protected override void OnDisconnected() {
            this.SlotModel.UpdateMediaFrameOffset -= this.SlotModelOnUpdateMediaFrameOffset;
        }
    }
}