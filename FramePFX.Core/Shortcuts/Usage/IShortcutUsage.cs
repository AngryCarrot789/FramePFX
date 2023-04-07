using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;

namespace FramePFX.Core.Shortcuts.Usage {
    public interface IShortcutUsage {
        /// <summary>
        /// A reference to the shortcut that created this instance
        /// </summary>
        IShortcut Shortcut { get; }

        /// <summary>
        /// Whether this input usage has been completed and is ready to be activated
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// The input stroke that was previously the <see cref="CurrentStroke"/> before it was progressed. Given 3 inputs, A, B and C, when <see cref="CurrentStroke"/> is B, <see cref="PreviousStroke"/> is A
        /// </summary>
        IInputStroke PreviousStroke { get; }

        /// <summary>
        /// The input stroke that is required to be input next in order for this usage to be progressed successfully
        /// </summary>
        IInputStroke CurrentStroke { get; }

        /// <summary>
        /// Returns an enumerable of the remaining input strokes required for this usage to be completed
        /// </summary>
        IEnumerable<IInputStroke> RemainingStrokes { get; }

        /// <summary>
        /// Whether the current stroke is a mouse stroke or not
        /// </summary>
        bool IsCurrentStrokeMouseBased { get; }

        /// <summary>
        /// Whether the current stroke is a key stroke or not
        /// </summary>
        bool IsCurrentStrokeKeyBased { get; }

        /// <summary>
        /// Attempts to move the current stroke to the next stroke in the sequence. If this usage is already
        /// completed (<see cref="IsCompleted"/>), then true is returned. If the current stroke matches
        /// the input stroke, then true is returned. Otherwise, false is returned
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns>
        /// Whether the stroke matches the current stroke, or if this usage is already completed
        /// </returns>
        bool OnInputStroke(IInputStroke stroke);
    }
}