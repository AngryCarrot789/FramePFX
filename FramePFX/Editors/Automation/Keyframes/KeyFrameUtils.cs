using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Editors.Automation.Params;

namespace FramePFX.Editors.Automation.Keyframes {
    public static class KeyFrameUtils {
        #region Unsafe/Raw Setters

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetFloatValue(this KeyFrame kf, float value) => ((KeyFrameFloat) kf).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDoubleValue(this KeyFrame kf, double value) => ((KeyFrameDouble) kf).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLongValue(this KeyFrame kf, long value) => ((KeyFrameLong) kf).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBooleanValue(this KeyFrame kf, bool value) => ((KeyFrameBoolean) kf).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector2Value(this KeyFrame kf, Vector2 value) => ((KeyFrameVector2) kf).Value = value;

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
    }
}