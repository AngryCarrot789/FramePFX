using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Automation.Keyframes {
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

        public static void SetDefaultValue(this IAutomatable automatable, ParameterFloat parameter, float value) {
            automatable.AutomationData[parameter].DefaultKeyFrame.SetFloatValue(value, ((Parameter) parameter).Descriptor);
        }

        public static void SetDefaultValue(this IAutomatable automatable, ParameterDouble parameter, double value) {
            automatable.AutomationData[parameter].DefaultKeyFrame.SetDoubleValue(value, ((Parameter) parameter).Descriptor);
        }

        public static void SetDefaultValue(this IAutomatable automatable, ParameterLong parameter, long value) {
            automatable.AutomationData[parameter].DefaultKeyFrame.SetLongValue(value, ((Parameter) parameter).Descriptor);
        }

        public static void SetDefaultValue(this IAutomatable automatable, ParameterBoolean parameter, bool value) {
            automatable.AutomationData[parameter].DefaultKeyFrame.SetBooleanValue(value, ((Parameter) parameter).Descriptor);
        }

        public static void SetDefaultValue(this IAutomatable automatable, ParameterVector2 parameter, Vector2 value) {
            automatable.AutomationData[parameter].DefaultKeyFrame.SetVector2Value(value, ((Parameter) parameter).Descriptor);
        }

        public static void SetDefaultValue(this IAutomatable automatable, ParameterVector2 parameter, float x, float y) => SetDefaultValue(automatable, parameter, new Vector2(x, y));
    }
}