using System.Collections.Generic;
using System.Linq;

namespace FramePFX.PropertyEditing {
    public abstract class BasePropertyEditor {
        private readonly HashSet<PropertyEditorSlot> selectedSlots;

        public SimplePropertyEditorGroup Root { get; }

        public BasePropertyEditor() {
            this.selectedSlots = new HashSet<PropertyEditorSlot>();
            this.Root = new SimplePropertyEditorGroup(typeof(object)) {
                DisplayName = "Root Object", IsExpanded = true
            };

            BasePropertyEditorObject.SetPropertyEditor(this.Root, this);
        }

        public static void UpdateSelection(PropertyEditorSlot slot) {
            BasePropertyEditor editor = slot.PropertyEditor;
            if (editor != null) {
                if (slot.IsSelected) {
                    editor.selectedSlots.Add(slot);
                }
                else {
                    editor.selectedSlots.Remove(slot);
                }
            }
        }

        public void ClearSelection() {
            // ToList() is required, otherwise the collection will be modified concurrently
            foreach (PropertyEditorSlot slot in this.selectedSlots.ToList()) {
                if (slot.IsSelectable)
                    slot.IsSelected = false;
            }
        }
    }
}