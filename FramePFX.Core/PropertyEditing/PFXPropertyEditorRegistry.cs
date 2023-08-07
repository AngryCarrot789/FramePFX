using FramePFX.Core.Editor.ViewModels.Timelines;
using FramePFX.Core.PropertyEditing.Editors.Editor;

namespace FramePFX.Core.PropertyEditing {
    public class PFXPropertyEditorRegistry : PropertyEditorRegistry {
        public static PFXPropertyEditorRegistry Instance { get; } = new PFXPropertyEditorRegistry();

        private PFXPropertyEditorRegistry() {
            PropertyGroupViewModel group = this.CreateRootGroup(typeof(ClipViewModel), "Clip Info");
            group.AddPropertyEditor("ClipDataEditor", new ClipDataEditorViewModel());
            group.AddPropertyEditor("VideoClipDataEditor_Single", new VideoClipDataSingleEditorViewModel());
            group.AddPropertyEditor("VideoClipDataEditor_Multi", new VideoClipDataMultipleEditorViewModel());
        }
    }
}