using System;

namespace FramePFX.Automation.Keyframe {
    public enum AutomationDataType : ushort {
        // To add a new data type:
        //   Obviously add new enum value below (do not modify the order of old ones)
        //   Define new KeyFrame and view-model implementation
        //   Define new AutomationKey implementation
        //   Define new KeyDescriptor implementation
        //   Define getter and setter methods in AutomationSequence
        //   Add cases to:
        //     KeyFrameViewModel methods
        //     AutomationSequenceEditor.SetupRenderingInfo
        //     AutomationUtils.SetValue
        //     AutomationUtils.CreateAssignmentInternal

        // float only exists for performance reasons, to avoid casts from double to float
        // long->int isn't that much of a performance overhead
        Float,
        Double,
        Long,
        Boolean,
        Vector2
    }

    public static class AutomationDataTypeUtils {
        /// <summary>
        /// Creates an instance of a key frame from the given data type
        /// </summary>
        /// <param name="dataType">The key frame data type</param>
        /// <returns>A purely new key frame instance</returns>
        /// <exception cref="ArgumentOutOfRangeException">An invalid data type was provided</exception>
        public static KeyFrame NewKeyFrame(AutomationDataType dataType) {
            switch (dataType) {
                case AutomationDataType.Float: return new KeyFrameFloat();
                case AutomationDataType.Double: return new KeyFrameDouble();
                case AutomationDataType.Long: return new KeyFrameLong();
                case AutomationDataType.Boolean: return new KeyFrameBoolean();
                case AutomationDataType.Vector2: return new KeyFrameVector2();
                default: throw new ArgumentOutOfRangeException(nameof(dataType), dataType, $"Invalid data type: {dataType}");
            }
        }
    }
}