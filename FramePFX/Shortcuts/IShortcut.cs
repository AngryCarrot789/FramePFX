// 
// Copyright (c) 2024-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Usage;

namespace FramePFX.Shortcuts;

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

    // In terms of adding multiple shortcuts to do the same thing (e.g. CTRL+R and F2 to rename), you
    // can just create a 2nd shortcut. Cannot add some sort of "InputStrokeSet", and use them below, because
    // what if the two shortcuts are CTRL+R,CTRL+R and F12?

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

    /// <summary>
    /// A helper function for checking if <see cref="PrimaryStroke"/> equals the given stroke (to prevent additional boxing of possible struct types)
    /// </summary>
    bool IsPrimaryStroke(IInputStroke input);
}