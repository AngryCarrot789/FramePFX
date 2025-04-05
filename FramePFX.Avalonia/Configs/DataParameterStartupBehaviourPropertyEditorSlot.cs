using System;
using System.Collections.Generic;
using FramePFX.Editing;
using PFXToolKitUI.DataTransfer;
using PFXToolKitUI.PropertyEditing.DataTransfer.Enums;

namespace FramePFX.Avalonia.Configs;

public class DataParameterStartupBehaviourPropertyEditorSlot : DataParameterEnumPropertyEditorSlot<EnumStartupBehaviour> {
    public static DataParameterEnumInfo<EnumStartupBehaviour> CodedIdEnumInfo { get; }

    public DataParameterStartupBehaviourPropertyEditorSlot(DataParameter<EnumStartupBehaviour> parameter, Type applicableType, string? displayName = null) : base(parameter, applicableType, displayName ?? "Codec ID", DataParameterEnumInfo<EnumStartupBehaviour>.EnumValuesOrderedByName, CodedIdEnumInfo) { }

    static DataParameterStartupBehaviourPropertyEditorSlot() {
        CodedIdEnumInfo = DataParameterEnumInfo<EnumStartupBehaviour>.All(new Dictionary<EnumStartupBehaviour, string> {
            [EnumStartupBehaviour.OpenStartupWindow] = "Open startup window",
            [EnumStartupBehaviour.OpenDemoProject] = "Open a demo project",
            [EnumStartupBehaviour.OpenEmptyProject] = "Open a new empty project",
        });
    }
}