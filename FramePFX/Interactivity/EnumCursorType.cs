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

namespace FramePFX.Interactivity;

/// <summary>
/// An enum for the type of cursor used in a cursor interaction event
/// </summary>
[Flags]
public enum EnumCursorType
{
    /// <summary>
    /// No cursor was involved, e.g. the user just moved their mouse without any buttons pressed
    /// </summary>
    None = 0,

    /// <summary>
    /// Mouse Left click or touch-screen press
    /// </summary>
    Primary = 1,

    /// <summary>
    /// Mouse Right click or, depending on the type of system and touch screen behaviour, typically a long press
    /// </summary>
    Secondary = 2,

    /// <summary>
    /// Mouse middle-click (scroll wheel click). Some systems and touch screens may implement this in their own way too
    /// </summary>
    Middle = 4,

    /// <summary>
    /// XButton1; the back button
    /// </summary>
    XButton1 = 8,

    /// <summary>
    /// XButton2; the forward button
    /// </summary>
    XButton2 = 16
}