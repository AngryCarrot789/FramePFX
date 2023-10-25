using System.Numerics;
using System.Runtime.CompilerServices;
using FramePFX.Automation.Keys;

namespace FramePFX.Automation.Keyframe {
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
        public static void SetFloatValue(this KeyFrame kf, float value, KeyDescriptor desc) => ((KeyFrameFloat) kf).Value = ((KeyDescriptorFloat) desc).Clamp(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetDoubleValue(this KeyFrame kf, double value, KeyDescriptor desc) => ((KeyFrameDouble) kf).Value = ((KeyDescriptorDouble) desc).Clamp(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetLongValue(this KeyFrame kf, long value, KeyDescriptor desc) => ((KeyFrameLong) kf).Value = ((KeyDescriptorLong) desc).Clamp(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBooleanValue(this KeyFrame kf, bool value, KeyDescriptor desc) => ((KeyFrameBoolean) kf).Value = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetVector2Value(this KeyFrame kf, Vector2 value, KeyDescriptor desc) => ((KeyFrameVector2) kf).Value = ((KeyDescriptorVector2) desc).Clamp(value);

        #endregion
    }
}