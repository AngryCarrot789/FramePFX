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

namespace FramePFX.Avalonia.AvControls.Dragger;

public enum DragDirection
{
    /// <summary>
    /// Decrement the value when dragged left, increment when dragged right (default)
    /// </summary>
    LeftDecrRightIncr,

    /// <summary>
    /// Increment the value when dragged left, decrement when dragged right
    /// </summary>
    LeftIncrRightDecr,

    /// <summary>
    /// Decrement the value when dragged up, and increment when dragged down (default)
    /// </summary>
    UpDecrDownIncr,

    /// <summary>
    /// Increment the value when dragged up, and decrement when dragged down
    /// </summary>
    UpIncrDownDecr
}