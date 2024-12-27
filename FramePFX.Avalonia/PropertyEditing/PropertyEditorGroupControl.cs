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

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using FramePFX.Avalonia.Bindings;
using FramePFX.Avalonia.Utils;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Grids;

namespace FramePFX.Avalonia.PropertyEditing;

/// <summary>
/// A control that contains a collection of property editor objects, such as slots, groups and separators
/// </summary>
public class PropertyEditorGroupControl : TemplatedControl {
    public static readonly ModelControlRegistry<BasePropertyEditorObject, Control> Registry;

    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<PropertyEditorGroupControl, bool>("IsExpanded");
    public static readonly StyledProperty<GroupType> GroupTypeProperty = AvaloniaProperty.Register<PropertyEditorGroupControl, GroupType>("GroupType");
    public static readonly StyledProperty<PropertyEditorControl?> PropertyEditorProperty = AvaloniaProperty.Register<PropertyEditorGroupControl, PropertyEditorControl?>("PropertyEditor");

    public bool IsExpanded {
        get => this.GetValue(IsExpandedProperty);
        set => this.SetValue(IsExpandedProperty, value);
    }

    public GroupType GroupType {
        get => this.GetValue(GroupTypeProperty);
        set => this.SetValue(GroupTypeProperty, value);
    }

    public PropertyEditorControl? PropertyEditor {
        get => this.GetValue(PropertyEditorProperty);
        set => this.SetValue(PropertyEditorProperty, value);
    }

    public PropertyEditorItemsPanel? Panel { get; private set; }

    public BasePropertyEditorGroup? Model { get; private set; }

    public Expander? TheExpander { get; private set; }

    private readonly AutoEventPropertyBinder<BasePropertyEditorGroup> displayNameBinder = new AutoEventPropertyBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.DisplayNameChanged), UpdateControlDisplayName);
    private readonly AutoEventPropertyBinder<BasePropertyEditorGroup> isVisibleBinder = new AutoEventPropertyBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsCurrentlyApplicableChanged), obj => ((PropertyEditorGroupControl) obj.Control).IsVisible = obj.Model.IsRoot || obj.Model.IsVisible);
    private readonly AutoEventPropertyBinder<BasePropertyEditorGroup> isExpandedBinder = new AutoEventPropertyBinder<BasePropertyEditorGroup>(nameof(BasePropertyEditorGroup.IsExpandedChanged), obj => ((PropertyEditorGroupControl) obj.Control).IsExpanded = obj.Model.IsExpanded, obj => obj.Model.IsExpanded = ((PropertyEditorGroupControl) obj.Control).IsExpanded);

    public PropertyEditorGroupControl() {
    }

    static PropertyEditorGroupControl() {
        Registry = new ModelControlRegistry<BasePropertyEditorObject, Control>();
        Registry.RegisterType<GridPropertyEditorGroup>(() => new PropertyEditorGridGroupControl());
        Registry.RegisterType<BasePropertyEditorGroup>((x) => x.GroupType == GroupType.NoExpander ? new PropertyEditorGroupNonExpanderControl() : new PropertyEditorGroupControl());
        Registry.RegisterType<PropertyEditorSlot>(() => new PropertyEditorSlotControl());
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        // WONKY CHANGE e.OriginalSource -> e.Source
        if (!e.Handled && e.Source is PropertyEditorItemsPanel) {
            e.Handled = true;
            this.PropertyEditor?.PropertyEditor?.ClearSelection();
            this.Focus();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.Panel = e.NameScope.GetTemplateChild<PropertyEditorItemsPanel>("PART_Panel");
        this.Panel.OwnerGroup = this;
        if (e.NameScope.TryGetTemplateChild("PART_Expander", out Expander? expander)) {
            this.TheExpander = expander;
        }
    }

    public void ConnectModel(PropertyEditorControl propertyEditor, BasePropertyEditorGroup group) {
        if (propertyEditor == null)
            throw new ArgumentNullException(nameof(propertyEditor));
        this.PropertyEditor = propertyEditor;
        this.Model = group;
        group.ItemAdded += this.ModelOnItemAdded;
        group.ItemRemoved += this.ModelOnItemRemoved;
        group.ItemMoved += this.ModelOnItemMoved;
        this.GroupType = group.GroupType;
        this.displayNameBinder.Attach(this, group);
        this.isVisibleBinder.Attach(this, group);
        this.isExpandedBinder.Attach(this, group);

        int i = 0;
        foreach (BasePropertyEditorObject obj in group.PropertyObjects) {
            this.Panel!.InsertItem(obj, i++);
        }
    }

    public void DisconnectModel() {
        for (int i = this.Panel!.Count - 1; i >= 0; i--) {
            this.Panel.RemoveItem(i);
        }

        this.displayNameBinder.Detach();
        this.isVisibleBinder.Detach();
        this.isExpandedBinder.Detach();
        this.Model!.ItemAdded -= this.ModelOnItemAdded;
        this.Model.ItemRemoved -= this.ModelOnItemRemoved;
        this.Model.ItemMoved -= this.ModelOnItemMoved;
        this.Model = null;
    }

    private void ModelOnItemAdded(BasePropertyEditorGroup group, BasePropertyEditorObject item, int index) {
        this.Panel!.InsertItem(item, index);
    }

    private void ModelOnItemRemoved(BasePropertyEditorGroup group, BasePropertyEditorObject item, int index) {
        this.Panel!.RemoveItem(index);
    }

    private void ModelOnItemMoved(BasePropertyEditorGroup group, BasePropertyEditorObject item, int oldindex, int newindex) {
        this.Panel!.MoveItem(oldindex, newindex);
    }

    private static void UpdateControlDisplayName(IBinder<BasePropertyEditorGroup> obj) {
        PropertyEditorGroupControl ctrl = (PropertyEditorGroupControl) obj.Control;
        if (ctrl.TheExpander != null)
            ctrl.TheExpander.Header = obj.Model.DisplayName;
    }
}