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

using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Bindings;
using FramePFX.PropertyEditing;
using FramePFX.Utils;

namespace FramePFX.Avalonia.PropertyEditing;

public class PropertyEditorSlotControl : ContentControl {
    public static readonly StyledProperty<bool> IsSelectedProperty = AvaloniaProperty.Register<PropertyEditorSlotControl, bool>("IsSelected", coerce: (o, val) => ((PropertyEditorSlotControl) o).IsSelectable && val);
    public static readonly StyledProperty<bool> IsSelectableProperty = AvaloniaProperty.Register<PropertyEditorSlotControl, bool>("IsSelectable");

    /// <summary>
    /// Whether or not this slot is selected. Setting this property automatically affects
    /// our <see cref="PropertyEditor"/>'s selected items
    /// </summary>
    [Category("Appearance")]
    public bool IsSelected {
        get => this.GetValue(IsSelectedProperty);
        set => this.SetValue(IsSelectedProperty, value);
    }

    [Category("Appearance")]
    public bool IsSelectable {
        get => this.GetValue(IsSelectableProperty);
        set => this.SetValue(IsSelectableProperty, value);
    }

    public PropertyEditorSlot? Model { get; private set; }

    public PropertyEditorGroupControl OwnerGroup { get; private set; }

    private readonly GetSetAutoUpdateAndEventPropertyBinder<PropertyEditorSlot> isSelectedBinder = new GetSetAutoUpdateAndEventPropertyBinder<PropertyEditorSlot>(IsSelectedProperty, nameof(PropertyEditorSlot.IsSelectedChanged), b => b.Model.IsSelected.Box(), (b, v) => b.Model.IsSelected = (bool) v!);

    public PropertyEditorSlotControl() {
    }

    static PropertyEditorSlotControl() {
        IsSelectedProperty.Changed.AddClassHandler<PropertyEditorSlotControl, bool>((o, e) => o.OnSelectionChanged(e.GetOldValue<bool>(), e.GetNewValue<bool>()));
        IsSelectableProperty.Changed.AddClassHandler<PropertyEditorSlotControl, bool>((o, e) => o.CoerceValue(IsSelectedProperty));
        PointerPressedEvent.AddClassHandler<PropertyEditorSlotControl>((t, e) => t.OnPreviewMouseDown(e), RoutingStrategies.Tunnel);
    }

    private void OnPreviewMouseDown(PointerPressedEventArgs e) {
        if (!e.Handled && this.IsSelectable) {
            if ((e.KeyModifiers & KeyModifiers.Control) != 0) {
                this.IsSelected = !this.IsSelected;
            }
            else if (this.Model?.PropertyEditor is PropertyEditor editor) {
                editor.ClearSelection();
                this.IsSelected = true;
            }
            else {
                return;
            }

            if (this.OwnerGroup?.PropertyEditor is PropertyEditorControl editorControl) {
                editorControl.TouchedSlot = this;
            }

            // if (!(e.OriginalSource is Control element)) {
            //     e.Handled = true;
            //     return;
            // }
            //
            // if (CanHandleClick(element, element is global::Avalonia.Controls.Control fe ? (fe.TemplatedParent as Control) : null)) {
            //     e.Handled = true;
            // }
        }
    }

    // private static bool CanHandleClick(Control originalSource, Control templatedParent) {
    //     if (originalSource.Focusable || templatedParent != null && templatedParent.Focusable) {
    //         return false;
    //     }
    //
    //     if (originalSource is TextBoxBase || originalSource.GetType().Name == "TextBoxView") {
    //         return false;
    //     }
    //
    //     return true;
    // }

    public void OnPreConnection(PropertyEditorGroupControl ownerGroup, PropertyEditorSlot item) {
        BasePropEditControlContent content = BasePropEditControlContent.NewContentInstance(item);
        this.Model = item;
        this.OwnerGroup = ownerGroup;
        this.Content = content;
    }

    public void ConnectModel() {
        this.IsSelectable = this.Model!.IsSelectable;
        this.isSelectedBinder.Attach(this, this.Model);
        this.Model.IsCurrentlyApplicableChanged += this.Model_IsCurrentlyApplicableChanged;

        BasePropEditControlContent content = (BasePropEditControlContent) this.Content!;
        content.ApplyStyling();
        content.ApplyTemplate();
        if (content.Template == null)
            Debug.Assert(false, "Slot control has no template, this could result in a crash soon");

        content.Connect(this);
        this.UpdateVisibility();
    }

    public void DisconnectModel() {
        ((BasePropEditControlContent) this.Content!).Disconnect();
        this.isSelectedBinder.Detach();
        this.UpdateVisibility();
        this.Model!.IsCurrentlyApplicableChanged -= this.Model_IsCurrentlyApplicableChanged;
        this.Model = null;
        this.OwnerGroup = null;
    }

    private void OnSelectionChanged(bool oldValue, bool newValue) {
    }

    private void Model_IsCurrentlyApplicableChanged(BasePropertyEditorItem sender) {
        this.UpdateVisibility();
    }

    protected virtual void UpdateVisibility() {
        this.IsVisible = this.Model!.IsVisible;
    }
}