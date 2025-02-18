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

using Avalonia.Controls.Primitives;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.Avalonia.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Avalonia.Utils;
using PFXToolKitUI.PropertyEditing;
using PFXToolKitUI.PropertyEditing.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Automatic;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.PropertyEditing;

/// <summary>
/// The base class for a property editor slot control. Slot controls are parented to a slot container control (which implements selection and visibility).
/// This is the class which represents the visuals of an
/// </summary>
public abstract class BasePropertyEditorSlotControl : TemplatedControl {
    public static readonly ModelControlRegistry<PropertyEditorSlot, BasePropertyEditorSlotControl> Registry;

    public PropertyEditorSlotContainerControl? SlotControl { get; private set; }

    public PropertyEditorSlot? SlotModel => this.SlotControl?.Model;

    public bool IsConnected => this.SlotControl != null;

    protected BasePropertyEditorSlotControl() {
    }

    static BasePropertyEditorSlotControl() {
        Registry = new ModelControlRegistry<PropertyEditorSlot, BasePropertyEditorSlotControl>();
        
        // standard editors
        Registry.RegisterType<DataParameterLongPropertyEditorSlot>(() => new DataParameterLongPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterDoublePropertyEditorSlot>(() => new DataParameterDoublePropertyEditorSlotControl());
        Registry.RegisterType<DataParameterFloatPropertyEditorSlot>(() => new DataParameterFloatPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterBoolPropertyEditorSlot>(() => new DataParameterBoolPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterStringPropertyEditorSlot>(() => new DataParameterStringPropertyEditorSlotControl());
        Registry.RegisterType<DataParameterVector2PropertyEditorSlot>(() => new DataParameterVector2PropertyEditorSlotControl());
        Registry.RegisterType<DataParameterColourPropertyEditorSlot>(() => new DataParameterColourPropertyEditorSlotControl());

        // automatic editors
        Registry.RegisterType<AutomaticDataParameterFloatPropertyEditorSlot>(() => new AutomaticDataParameterFloatPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticDataParameterDoublePropertyEditorSlot>(() => new AutomaticDataParameterDoublePropertyEditorSlotControl());
        Registry.RegisterType<AutomaticDataParameterLongPropertyEditorSlot>(() => new AutomaticDataParameterLongPropertyEditorSlotControl());
        Registry.RegisterType<AutomaticDataParameterVector2PropertyEditorSlot>(() => new AutomaticDataParameterVector2PropertyEditorSlotControl());
    }

    public static void RegisterEnumControl<TEnum, TSlot>() where TEnum : struct, Enum where TSlot : DataParameterEnumPropertyEditorSlot<TEnum> {
        Registry.RegisterType<TSlot>(() => new EnumDataParameterPropertyEditorSlotControl<TEnum>());
    }

    public static BasePropertyEditorSlotControl NewOrCachedContentInstance(PropertyEditorSlot slot) {
        Validate.NotNull(slot);
        return Registry.NewInstance(slot);
    }

    /// <summary>
    /// Connect this slot content to the given control
    /// </summary>
    public void Connect(PropertyEditorSlotContainerControl slotContainer) {
        this.SlotControl = slotContainer;
        this.OnConnected();
    }

    /// <summary>
    /// Disconnect this slot content from the slot control
    /// </summary>
    public void Disconnect() {
        this.OnDisconnected();
        this.SlotControl = null;
    }

    /// <summary>
    /// Invoked when this slot control is attached to a slot model
    /// </summary>
    protected abstract void OnConnected();

    /// <summary>
    /// Invoked when this slot control is detached from a slot model
    /// </summary>
    protected abstract void OnDisconnected();
}