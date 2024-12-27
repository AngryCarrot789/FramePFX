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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Grids;
using FramePFX.Utils;

namespace FramePFX.BaseFrontEnd.PropertyEditing;

/// <summary>
/// A panel which is used to store child group and slot controls (as well as things like separators)
/// </summary>
public class PropertyEditorItemsPanel : StackPanel {
    /// <summary>
    /// Gets or sets the group that this panel belongs to
    /// </summary>
    public PropertyEditorGroupControl? OwnerGroup { get; set; }

    public PropertyEditorControl? PropertyEditor => this.OwnerGroup?.PropertyEditor;

    public int Count => this.Children.Count;

    public PropertyEditorItemsPanel() {
    }

    protected override Size MeasureOverride(Size constraint) {
        return base.MeasureOverride(constraint);
    }

    protected override Size ArrangeOverride(Size arrangeSize) {
        return base.ArrangeOverride(arrangeSize);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        PointerPoint point = e.GetCurrentPoint(this);
        if (e.Handled || point.Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed) {
            return;
        }

        // WONKY CHANGE e.OriginalSource -> e.Source
        if (ReferenceEquals(e.Source, this)) {
            this.PropertyEditor?.PropertyEditor?.ClearSelection();
        }
    }

    static PropertyEditorItemsPanel() {
    }

    public void InsertItem(BasePropertyEditorObject item, int index) {
        Validate.NotNull(item);
        PropertyEditorControl? editor = this.OwnerGroup?.PropertyEditor;
        if (editor == null)
            throw new InvalidOperationException("Cannot insert items while our owner group's editor is null");

        Control control;
        if (item is GridPropertyEditorGroup gridGroup) {
            control = new PropertyEditorGridGroupControl();
            this.Children.Insert(index, control);
            control.ApplyStyling();
            control.ApplyTemplate();
            ((PropertyEditorGroupControl) control).ConnectModel(editor, gridGroup);
        }
        else if (item is BasePropertyEditorGroup group) {
            control = group.GroupType == GroupType.NoExpander ? new PropertyEditorGroupNonExpanderControl() : new PropertyEditorGroupControl();
            this.Children.Insert(index, control);
            control.ApplyStyling();
            control.ApplyTemplate();
            ((PropertyEditorGroupControl) control).ConnectModel(editor, group);
        }
        else if (item is PropertyEditorSlot) {
            control = new PropertyEditorSlotControl();
            ((PropertyEditorSlotControl) control).OnPreConnection(this.OwnerGroup!, (PropertyEditorSlot) item);
            this.Children.Insert(index, control);
            control.ApplyStyling();
            control.ApplyTemplate();
            ((PropertyEditorSlotControl) control).ConnectModel();
        }
        else {
            throw new InvalidOperationException("Invalid model: " + item);
        }
    }

    public void RemoveItem(int index) {
        Control item = this.Children[index];
        if (item is PropertyEditorGroupControl) {
            ((PropertyEditorGroupControl) item).DisconnectModel();
        }
        else if (item is PropertyEditorSlotControl) {
            ((PropertyEditorSlotControl) item).DisconnectModel();
        }

        this.Children.RemoveAt(index);
    }

    public void MoveItem(int oldIndex, int newIndex) {
        Control control = this.Children[oldIndex];
        this.Children.RemoveAt(oldIndex);
        this.Children.Insert(newIndex, control);
    }
}