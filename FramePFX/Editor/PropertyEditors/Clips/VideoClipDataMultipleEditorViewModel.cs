using FramePFX.PropertyEditing;

namespace FramePFX.Editor.PropertyEditors.Clips {
    public class VideoClipDataMultipleEditorViewModel : VideoClipDataEditorViewModel {
        public override HandlerCountMode HandlerCountMode => HandlerCountMode.Multi;
    }
}