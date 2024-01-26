namespace FramePFX.Editors.Automation.Keyframes {
    /// <summary>
    /// All of the data types that are currently automatable
    /// </summary>
    public enum AutomationDataType : byte {
        /// <summary>
        /// An automated 32-bit floating point number
        /// </summary>
        Float,
        /// <summary>
        /// An automated 64-bit double-precision floating point number
        /// </summary>
        Double,
        /// <summary>
        /// An automated 64-bit integer number
        /// </summary>
        Long,
        /// <summary>
        /// An automated boolean value
        /// </summary>
        Boolean

        // TODO: maybe vector2 automation?
    }
}