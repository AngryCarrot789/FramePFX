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

namespace FramePFX.Avalonia.Shortcuts.Managing;

/// <summary>
/// An interface implemented by <see cref="ShortcutGroup"/>, <see cref="GroupedShortcut"/> and <see cref="GroupedInputState"/>
/// </summary>
public interface IGroupedObject {
    /// <summary>
    /// Gets the manager that this object belongs to. This typically is equal to <see cref="ShortcutManager.Instance"/>
    /// </summary>
    ShortcutManager Manager { get; }

    /// <summary>
    /// Gets the group that contains this object. Null means that this object is the
    /// root <see cref="ShortcutGroup"/> for a <see cref="ShortcutManager"/>
    /// </summary>
    ShortcutGroup Parent { get; }

    /// <summary>
    /// Gets the name of this grouped object. If this instance is a <see cref="ShortcutGroup"/> and is the root
    /// for a <see cref="ShortcutManager"/>, then this value will be null. Otherwise, This will not be null,
    /// empty or consist of only whitespaces; it will always be a valid string (even if only 1 character)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the full path of this grouped object, which combines the parent (<see cref="Parent"/>)'s <see cref="FullPath"/> and
    /// the current instance's <see cref="Name"/> with a '/' character. If the parent is null, this is equal to <see cref="Name"/>.
    /// <para>
    /// As per the docs for <see cref="Name"/>, it will always be a valid string (with at least 1 character)
    /// </para>
    /// </summary>
    string FullPath { get; }
}