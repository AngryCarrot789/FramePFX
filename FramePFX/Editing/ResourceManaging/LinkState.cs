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

namespace FramePFX.Editing.ResourceManaging;

/// <summary>
/// A state for a resource link
/// </summary>
public enum LinkState
{
    /// <summary>
    /// Not linked; link not attempted, no reference count
    /// </summary>
    NotLinked,

    /// <summary>
    /// Link failed due to an incompatible resource object type, no reference count
    /// </summary>
    IncompatibleResource,

    /// <summary>
    /// Link failed because a resource did not exist with the specific ID, no reference count
    /// </summary>
    NoSuchResource,

    /// <summary>
    /// Successfully linked to a resource and a reference is counted
    /// </summary>
    Linked,
}