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
using FramePFX.BaseFrontEnd.PropertyEditing.Automation;
using FramePFX.BaseFrontEnd.PropertyEditing.Core;
using FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer;
using FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer.Automatic;
using FramePFX.BaseFrontEnd.PropertyEditing.DataTransfer.Enums;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Editing.PropertyEditors;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Core;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer.Automatic;
using FramePFX.PropertyEditing.DataTransfer.Enums;
using FramePFX.Utils;

namespace FramePFX.BaseFrontEnd.PropertyEditing;

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
        // specific case editors
        Registry.RegisterType<DisplayNamePropertyEditorSlot>(() => new DisplayNamePropertyEditorSlotControl());
        Registry.RegisterType<VideoClipMediaFrameOffsetPropertyEditorSlot>(() => new VideoClipMediaFrameOffsetPropertyEditorSlotControl());
        Registry.RegisterType<TimecodeFontFamilyPropertyEditorSlot>(() => new TimecodeFontFamilyPropertyEditorSlotControl());

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

        // automation parameter editors
        Registry.RegisterType<ParameterFloatPropertyEditorSlot>(() => new ParameterFloatPropertyEditorSlotControl());
        Registry.RegisterType<ParameterDoublePropertyEditorSlot>(() => new ParameterDoublePropertyEditorSlotControl());
        Registry.RegisterType<ParameterLongPropertyEditorSlot>(() => new ParameterLongPropertyEditorSlotControl());
        Registry.RegisterType<ParameterVector2PropertyEditorSlot>(() => new ParameterVector2PropertyEditorSlotControl());
        Registry.RegisterType<ParameterBoolPropertyEditorSlot>(() => new ParameterBoolPropertyEditorSlotControl());
    }

    public static void RegisterEnumProperty<TEnum, TSlot>() where TEnum : struct, Enum where TSlot : DataParameterEnumPropertyEditorSlot<TEnum> {
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