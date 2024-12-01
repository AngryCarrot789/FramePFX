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

using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Editing.Automation.Params;

namespace FramePFX.Editing.Automation.Keyframes;

[SwitchAutomationDataType]
public static class KeyFrameUtils {
    #region Unsafe/Raw Setters

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBooleanValue(this KeyFrame kf, bool value) => ((KeyFrameBoolean) kf).Value = value;

    #endregion

    #region Clamped Setters

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetFloatValue(this KeyFrame kf, float value, ParameterDescriptor desc) => ((KeyFrameFloat) kf).Value = ((ParameterDescriptorFloat) desc).Clamp(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetDoubleValue(this KeyFrame kf, double value, ParameterDescriptor desc) => ((KeyFrameDouble) kf).Value = ((ParameterDescriptorDouble) desc).Clamp(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLongValue(this KeyFrame kf, long value, ParameterDescriptor desc) => ((KeyFrameLong) kf).Value = ((ParameterDescriptorLong) desc).Clamp(value);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetBooleanValue(this KeyFrame kf, bool value, ParameterDescriptor desc) => ((KeyFrameBoolean) kf).Value = value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetVector2Value(this KeyFrame kf, Vector2 value, ParameterDescriptor desc) => ((KeyFrameVector2) kf).Value = ((ParameterDescriptorVector2) desc).Clamp(value);

    public static void SetVector2Value(this KeyFrame kf, float x, float y, ParameterDescriptor desc) => ((KeyFrameVector2) kf).Value = ((ParameterDescriptorVector2) desc).Clamp(new Vector2(x, y));

    #endregion

    public static KeyFrame GetDefaultKeyFrame(this IAutomatable automatable, Parameter parameter) {
        return automatable.AutomationData[parameter].DefaultKeyFrame;
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterFloat parameter, float value, bool enableOverride = false) {
        AutomationSequence seq = automatable.AutomationData[parameter];
        if (enableOverride)
            seq.IsOverrideEnabled = true;
        seq.DefaultKeyFrame.SetFloatValue(value, ((Parameter) parameter).Descriptor);
        if (!seq.CanAutomate)
            seq.UpdateValue();
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterDouble parameter, double value, bool enableOverride = false) {
        AutomationSequence seq = automatable.AutomationData[parameter];
        if (enableOverride)
            seq.IsOverrideEnabled = true;
        seq.DefaultKeyFrame.SetDoubleValue(value, ((Parameter) parameter).Descriptor);
        if (!seq.CanAutomate)
            seq.UpdateValue();
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterLong parameter, long value, bool enableOverride = false) {
        AutomationSequence seq = automatable.AutomationData[parameter];
        if (enableOverride)
            seq.IsOverrideEnabled = true;
        seq.DefaultKeyFrame.SetLongValue(value, ((Parameter) parameter).Descriptor);
        if (!seq.CanAutomate)
            seq.UpdateValue();
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterBool parameter, bool value, bool enableOverride = false) {
        AutomationSequence seq = automatable.AutomationData[parameter];
        if (enableOverride)
            seq.IsOverrideEnabled = true;
        seq.DefaultKeyFrame.SetBooleanValue(value, ((Parameter) parameter).Descriptor);
        if (!seq.CanAutomate)
            seq.UpdateValue();
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterVector2 parameter, Vector2 value, bool enableOverride = false) {
        AutomationSequence seq = automatable.AutomationData[parameter];
        if (enableOverride)
            seq.IsOverrideEnabled = true;
        seq.DefaultKeyFrame.SetVector2Value(value, ((Parameter) parameter).Descriptor);
        if (!seq.CanAutomate)
            seq.UpdateValue();
    }

    public static void SetDefaultValue(this IAutomatable automatable, ParameterVector2 parameter, float x, float y) => SetDefaultValue(automatable, parameter, new Vector2(x, y));
}