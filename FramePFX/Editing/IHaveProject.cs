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

namespace FramePFX.Editing;

/// <summary>
/// An interface for an object that exists in a project, somewhere. Examples include timeline, track, clip, effect, resource, etc.
/// </summary>
public interface IHaveProject {
    /// <summary>
    /// Gets the project associated with this object. May return null if not associated with
    /// a project yet (e.g. clip not placed in a track)
    /// </summary>
    Project? Project { get; }
}