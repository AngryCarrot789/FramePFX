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

namespace PFXToolKitUI.Icons;

public enum StretchMode {
    /// <summary>
    /// The content preserves its original size.
    /// </summary>
    None,

    /// <summary>
    /// The content is resized to fill the destination dimensions. The aspect ratio is not
    /// preserved.
    /// </summary>
    Fill,

    /// <summary>
    /// The content is resized to fit in the destination dimensions while preserving its
    /// native aspect ratio.
    /// </summary>
    Uniform,

    /// <summary>
    /// The content is resized to completely fill the destination rectangle while preserving
    /// its native aspect ratio. A portion of the content may not be visible if the aspect
    /// ratio of the content does not match the aspect ratio of the allocated space.
    /// </summary>
    UniformToFill,
}