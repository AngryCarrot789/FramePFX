namespace SharpPadV2.Core.Shortcuts.Inputs {
    /// <summary>
    /// An interface defining behaviour for input strokes
    /// </summary>
    public interface IInputStroke {
        /// <summary>
        /// This input stroke is keyboard-based
        /// </summary>
        bool IsKeyboard { get; }

        /// <summary>
        /// This input stroke is mouse-based
        /// </summary>
        bool IsMouse { get; }

        /// <summary>
        /// Gets whether the given stroke matches this stroke. This function may ignore certain details such as mouse click count
        /// </summary>
        /// <param name="stroke">The stroke to compare</param>
        /// <returns>The current instance and the given stroke are "equal/match"</returns>
        bool Equals(IInputStroke stroke);
    }
}