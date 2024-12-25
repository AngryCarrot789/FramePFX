//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

namespace FramePFX.PropertyEditing;

/// <summary>
/// The main class for a property editor. This handles selection and the root-level group
/// </summary>
public class PropertyEditor {
    private readonly HashSet<PropertyEditorSlot> selectedSlots;

    public SimplePropertyEditorGroup Root { get; }

    public PropertyEditor() {
        this.selectedSlots = new HashSet<PropertyEditorSlot>();
        this.Root = new SimplePropertyEditorGroup(typeof(object)) {
            DisplayName = "Root Object", IsExpanded = true
        };

        BasePropertyEditorObject.SetPropertyEditor(this.Root, this);
    }

    internal static void InternalProcessSelectionChanged(PropertyEditorSlot slot) {
        PropertyEditor? editor = slot.PropertyEditor;
        if (editor != null) {
            if (slot.IsSelected) {
                editor.selectedSlots.Add(slot);
            }
            else {
                editor.selectedSlots.Remove(slot);
            }
        }
    }

    internal static void InternalProcessSelectionForEditorChanged(PropertyEditorSlot slot, PropertyEditor? oldEditor, PropertyEditor? newEditor) {
        if (oldEditor != null && slot.IsSelected) {
            oldEditor.selectedSlots.Remove(slot);
        }

        if (newEditor != null && slot.IsSelected) {
            newEditor.selectedSlots.Add(slot);
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