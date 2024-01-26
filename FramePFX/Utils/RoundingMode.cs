namespace FramePFX.Utils {
    /// <summary>
    /// A mode for how to treat a decimal number whose decimal part is non-zero
    /// </summary>
    public enum RoundingMode {
        /// <summary>
        /// Does nothing. This may not be valid in all cases, meaning this value may default to <see cref="Cast"/>
        /// </summary>
        None,
        /// <summary>
        /// Casts the decimal number to an integer number. May not result in the desired effect with negative numbers
        /// </summary>
        Cast,
        /// <summary>
        /// Floors the decimal number then casts to an integer number
        /// </summary>
        Floor,
        /// <summary>
        /// Ceilings the decimal number then casts to an integer number
        /// </summary>
        Ceil,
        /// <summary>
        /// Rounds the decimal number to the nearest integer number
        /// </summary>
        Round
    }
}