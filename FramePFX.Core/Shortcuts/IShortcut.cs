using System;
using System.Collections.Generic;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Usage;

namespace FramePFX.Core.Shortcuts {
    /// <summary>
    /// The base class for all shortcuts
    /// </summary>
    public interface IShortcut {
        /// <summary>
        /// Whether this shortcut is a keyboard-based shortcut. When false, it may be something else (mouse, joystick, etc)
        /// </summary>
        bool IsKeyboard { get; }

        /// <summary>
        /// Whether this shortcut is a mouse-based shortcut. When false, it may be something else (keyboard, joystick, etc)
        /// </summary>
        bool IsMouse { get; }

        /// <summary>
        /// Returns whether this shortcut is empty, meaning it has no input strokes, meaning that it cannot be activated
        /// </summary>
        bool IsEmpty { get; }

        /// <summary>
        /// Whether this shortcut has secondary input strokes or not. When it does, it requires
        /// a "Usage" implementation, in order to track the progression of key strokes
        /// <para>
        /// This is typically implemented by checking if <see cref="InputStrokes"/> yields more than 1 item
        /// </para>
        /// </summary>
        bool HasSecondaryStrokes { get; }

        /// <summary>
        /// This shortcut's primary input stroke for initial or full activation
        /// </summary>
        IInputStroke PrimaryStroke { get; }

        /// <summary>
        /// All of this shortcut's input strokes, including the primary stroke
        /// </summary>
        IEnumerable<IInputStroke> InputStrokes { get; }

        /// <summary>
        /// Creates a shortcut usage for this shortcut
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">This shortcut is empty (has no input strokes)</exception>
        IShortcutUsage CreateUsage();

        bool IsPrimaryStroke(IInputStroke input);
    }
}