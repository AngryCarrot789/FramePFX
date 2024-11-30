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

using FramePFX.Avalonia.Shortcuts.Inputs;

namespace FramePFX.Avalonia.Shortcuts.Usage;

public interface IMouseShortcutUsage : IShortcutUsage {
    /// <summary>
    /// A reference to the shortcut that created this instance
    /// </summary>
    IMouseShortcut MouseShortcut { get; }

    /// <summary>
    /// <para>
    /// The stroke that is currently to be processed. During the constructor for this
    /// class, this is set to the first mouse stroke of the <see cref="IShortcut"/>'s secondary inputs
    /// </para>
    /// <para>
    /// When a stroke input is received by the user, if it matches this stroke, then
    /// this usage will be progressed and this field will be set to the next next stroke
    /// </para>
    /// <para>
    /// If null, then all strokes have been processed and the shortcut is ready to be activated
    /// </para>
    /// </summary>
    MouseStroke CurrentMouseStroke { get; }

    /// <summary>
    /// Attempts to progress current stroke to the next stroke in the sequence. If this usage is already
    /// completed (<see cref="IShortcutUsage.IsCompleted"/>), then true is returned. If the current stroke matches
    /// the input stroke, then the current stroke is progressed and true is returned. Otherwise, false is returned
    /// </summary>
    /// <param name="stroke">The key stroke that was pressed by the user</param>
    /// <returns>
    /// Whether the stroke matches the current stroke, or if this usage is already completed
    /// </returns>
    bool OnMouseStroke(in MouseStroke stroke);
}