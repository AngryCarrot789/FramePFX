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
using FramePFX.Avalonia.PropertyEditing.Automation;
using FramePFX.Avalonia.PropertyEditing.Core;
using FramePFX.Avalonia.PropertyEditing.DataTransfer;
using FramePFX.Avalonia.PropertyEditing.DataTransfer.Automatic;
using FramePFX.Avalonia.Utils;
using FramePFX.Editing.Exporting.FFmpeg;
using FramePFX.Editing.PropertyEditors;
using FramePFX.PropertyEditing;
using FramePFX.PropertyEditing.Automation;
using FramePFX.PropertyEditing.Core;
using FramePFX.PropertyEditing.DataTransfer;
using FramePFX.PropertyEditing.DataTransfer.Automatic;
using FramePFX.Utils;

namespace FramePFX.Avalonia.PropertyEditing;

public abstract class BasePropEditControlContent : TemplatedControl
{
    public static readonly ModelControlRegistry<PropertyEditorSlot, BasePropEditControlContent> Registry;

    public PropertyEditorSlotControl? SlotControl { get; private set; }

    public PropertyEditorSlot? SlotModel => this.SlotControl?.Model;

    public bool IsConnected => this.SlotControl != null;

    protected BasePropEditControlContent() {
    }

    static BasePropEditControlContent()
    {
        Registry = new ModelControlRegistry<PropertyEditorSlot, BasePropEditControlContent>();
        // specific case editors
        Registry.RegisterType<DisplayNamePropertyEditorSlot>(() => new DisplayNamePropertyEditorControl());
        Registry.RegisterType<VideoClipMediaFrameOffsetPropertyEditorSlot>(() => new VideoClipMediaFrameOffsetPropertyEditorControl());
        Registry.RegisterType<TimecodeFontFamilyPropertyEditorSlot>(() => new TimecodeFontFamilyPropertyEditorControl());

        // standard editors
        Registry.RegisterType<DataParameterLongPropertyEditorSlot>(() => new DataParameterLongPropertyEditorControl());
        Registry.RegisterType<DataParameterDoublePropertyEditorSlot>(() => new DataParameterDoublePropertyEditorControl());
        Registry.RegisterType<DataParameterFloatPropertyEditorSlot>(() => new DataParameterFloatPropertyEditorControl());
        Registry.RegisterType<DataParameterBoolPropertyEditorSlot>(() => new DataParameterBoolPropertyEditorControl());
        Registry.RegisterType<DataParameterStringPropertyEditorSlot>(() => new DataParameterStringPropertyEditorControl());
        Registry.RegisterType<DataParameterPointPropertyEditorSlot>(() => new DataParameterPointPropertyEditorControl());

        // automatic editors
        Registry.RegisterType<AutomaticDataParameterFloatPropertyEditorSlot>(() => new AutomaticDataParameterFloatPropertyEditorControl());
        Registry.RegisterType<AutomaticDataParameterDoublePropertyEditorSlot>(() => new AutomaticDataParameterDoublePropertyEditorControl());
        Registry.RegisterType<AutomaticDataParameterLongPropertyEditorSlot>(() => new AutomaticDataParameterLongPropertyEditorControl());
        Registry.RegisterType<AutomaticDataParameterPointPropertyEditorSlot>(() => new AutomaticDataParameterPointPropertyEditorControl());

        // automation parameter editors
        Registry.RegisterType<ParameterFloatPropertyEditorSlot>(() => new ParameterFloatPropertyEditorControl());
        Registry.RegisterType<ParameterDoublePropertyEditorSlot>(() => new ParameterDoublePropertyEditorControl());
        Registry.RegisterType<ParameterLongPropertyEditorSlot>(() => new ParameterLongPropertyEditorControl());
        Registry.RegisterType<ParameterVector2PropertyEditorSlot>(() => new ParameterVector2PropertyEditorControl());
        Registry.RegisterType<ParameterBoolPropertyEditorSlot>(() => new ParameterBoolPropertyEditorControl());
        
        // Enums
        Registry.RegisterType<DataParameterAVCodecIDPropertyEditorSlot>(() => new DataParameterAVCodedIdPropertyEditorControl());
    }

    public static BasePropEditControlContent NewContentInstance(PropertyEditorSlot slot)
    {
        Validate.NotNull(slot);
        return Registry.NewInstance(slot);
    }

    public void Connect(PropertyEditorSlotControl slot)
    {
        this.SlotControl = slot;
        this.OnConnected();
    }

    public void Disconnect()
    {
        this.OnDisconnected();
        this.SlotControl = null;
    }

    protected abstract void OnConnected();

    protected abstract void OnDisconnected();
}