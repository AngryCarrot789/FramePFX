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

[Flags]
public enum EnumDropType {
    /// <summary>
    /// No drop (default state)
    /// </summary>
    None = 0,

    /// <summary>
    /// A copy drop; the object should be copied from the source to the target
    /// </summary>
    Copy = 1,

    /// <summary>
    /// A move drop; the object should be removed from the source and added to the target
    /// </summary>
    Move = 2,

    /// <summary>
    /// A link drop; a reference to the source object should be added to the target
    /// </summary>
    Link = 4,

    // not entirely sure what scroll is for, maybe to notify a list to scroll up/down?
    Scroll = -2147483648, // 0x80000000
    All = Scroll | Move | Copy, // 0x80000003
}