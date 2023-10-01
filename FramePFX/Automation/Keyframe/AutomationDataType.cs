namespace FramePFX.Automation.Keyframe
{
    public enum AutomationDataType : ushort
    {
        // To add a new data type:
        //   Obviously add new enum value below (do not modify the order of old ones)
        //   Define new KeyFrame implementation
        //   Define new AutomationKey implementation
        //   Define new KeyDescriptor implementation
        //   Define getter and setter methods in AutomationSequence
        //   Add new case to the KeyFrame.CreateInstance methods
        //

        // float only exists for performance reasons, to avoid casts from double to float
        // long->int isn't that much of a performance overhead
        Float,
        Double,
        Long,
        Boolean,
        Vector2
    }
}