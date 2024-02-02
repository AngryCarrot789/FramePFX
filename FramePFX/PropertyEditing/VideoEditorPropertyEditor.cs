using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.PropertyEditors.Effects;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.PropertyEditing.Standard;

namespace FramePFX.PropertyEditing {
    /// <summary>
    /// A class which stores the video editor's general property editor information
    /// </summary>
    public class VideoEditorPropertyEditor : BasePropertyEditor {
        public static VideoEditorPropertyEditor Instance { get; } = new VideoEditorPropertyEditor();

        public SimplePropertyEditorGroup ClipGroup { get; }

        public EffectListPropertyEditorGroup EffectListGroup { get; }

        private VideoEditorPropertyEditor() {
            {
                this.ClipGroup = new SimplePropertyEditorGroup(typeof(Clip)) {
                    DisplayName = "Clip", IsExpanded = true
                };

                this.ClipGroup.AddItem(new ClipDisplayNamePropertyEditorSlot());
                this.ClipGroup.AddItem(new ParameterDoublePropertyEditorSlot(VideoClip.OpacityParameter, typeof(VideoClip), "Opacity", DragStepProfile.UnitOne));
                this.ClipGroup.AddItem(new VideoClipMediaFrameOffsetPropertyEditorSlot());

                SimplePropertyEditorGroup shapeGroup = new SimplePropertyEditorGroup(typeof(VideoClipShape));
                shapeGroup.AddItem(new ParameterVector2PropertyEditorSlot(VideoClipShape.SizeParameter, typeof(VideoClipShape), "Size", DragStepProfile.InfPixelRange));
                this.ClipGroup.AddItem(shapeGroup);

                this.EffectListGroup = new EffectListPropertyEditorGroup();
                this.ClipGroup.AddItem(this.EffectListGroup);
            }

            this.Root.AddItem(this.ClipGroup);
        }

        public void UpdateClipSelection(Timeline timeline) {
            List<Clip> selection = timeline.SelectedClips.ToList();
            this.ClipGroup.SetupHierarchyState(selection);
            if (selection.Count == 1) {
                this.EffectListGroup.SetupHierarchyState(selection[0]);
            }
            else {
                this.EffectListGroup.ClearHierarchy();
            }
        }
    }
}