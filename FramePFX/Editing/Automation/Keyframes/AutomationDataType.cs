// 
// Copyright (c) 2023-2024 REghZy
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

namespace FramePFX.Editing.Automation.Keyframes;

/// <summary>
/// All of the data types that are currently automatable
/// </summary>
public enum AutomationDataType : byte
{
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
    Boolean,

    /// <summary>
    /// An automated vector2 value. This contains both an X and Y value
    /// </summary>
    Vector2
}