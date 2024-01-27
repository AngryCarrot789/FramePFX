using System.Collections.Generic;
using System.Linq;
using FramePFX.Editors.PropertyEditors.Clips;
using FramePFX.Editors.PropertyEditors.Effects;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Effects;

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
                this.ClipGroup.AddItem(new VideoClipOpacityPropertyEditorSlot());

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