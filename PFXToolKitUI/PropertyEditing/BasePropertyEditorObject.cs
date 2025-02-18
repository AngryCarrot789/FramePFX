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

namespace PFXToolKitUI.PropertyEditing;

public delegate void PropertyEditorObjectEventHandler(BasePropertyEditorObject sender);

/// <summary>
/// A base class for all items in a property editor hierarchy, such as slots, groups, separators, etc.
/// </summary>
public abstract class BasePropertyEditorObject {
    /// <summary>
    /// Gets this property editor item's parent group. May be null for the root <see cref="BasePropertyEditorGroup"/>
    /// </summary>
    public BasePropertyEditorGroup? Parent { get; private set; }

    public PropertyEditor? PropertyEditor { get; private set; }

    protected BasePropertyEditorObject() {
    }

    /// <summary>
    /// Called when this object is added or removed from a group whose property editor is different.
    /// This method is called for an entire object hierarchy; the children of a group's children
    /// </summary>
    /// <param name="oldEditor">The previous editor</param>
    /// <param name="newEditor">The new editor</param>
    protected virtual void OnPropertyEditorChanged(PropertyEditor? oldEditor, PropertyEditor? newEditor) {
    }

    protected static void OnAddedToGroup(BasePropertyEditorObject propObj, BasePropertyEditorGroup parent) {
        if (propObj.Parent == parent)
            throw new InvalidOperationException("Object already added to this parent");
        propObj.Parent = parent;
        SetPropertyEditor(propObj, parent.PropertyEditor!);
    }

    protected static void OnRemovedFromGroup(BasePropertyEditorObject propObj, BasePropertyEditorGroup parent) {
        if (propObj.Parent == null)
            throw new InvalidOperationException("Object does not exist in a parent");
        propObj.Parent = null;
        SetPropertyEditor(propObj, null);
    }

    internal static void SetPropertyEditor(BasePropertyEditorObject obj, PropertyEditor? newEditor) {
        PropertyEditor? oldEditor = obj.PropertyEditor;
        if (oldEditor != newEditor) {
            obj.PropertyEditor = newEditor;
            obj.OnPropertyEditorChanged(oldEditor, newEditor);
        }
    }
}