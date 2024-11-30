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

namespace FramePFX.Editing.ResourceManaging.ResourceHelpers;

/// <summary>
/// The base interface for <see cref="IResourcePathKey{T}"/>, so that it can be used in a non-generic context
/// </summary>
public interface IBaseResourcePathKey : IResourceHolder {
    /// <summary>
    /// Gets the active resource link for this resource path entry. This returns null when the resource has not yet or could not
    /// be linked (empty ID, resource does not exist, or the resource does not pass the <see cref="IsItemTypeApplicable"/> test)
    /// </summary>
    ResourceLink ActiveLink { get; }

    /// <summary>
    /// Gets the additional flags associated with this entry
    /// </summary>
    ResourcePathFlags Flags { get; }

    /// <summary>
    /// Gets the unique name/key for this entry relative to the parent's <see cref="ResourceHelper"/>.
    /// This is non-null and contains at least 1 non-whitespace character
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Sets the target ID for this entry. This will cause the <see cref="ActiveLink"/> property to be disposed and replaced with a new value
    /// </summary>
    /// <param name="id">The new resource path ID</param>
    void SetTargetResourceId(ulong id);

    void TryLoadLink();

    /// <summary>
    /// Clears the active resource link, if present
    /// </summary>
    void ClearResourceLink();

    /// <summary>
    /// Checks if the given item's type is actually applicable to this resource path key
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    bool IsItemTypeApplicable(ResourceItem item);

    /// <summary>
    /// Tries to get the resource for this entry
    /// </summary>
    /// <param name="resource">The resource found</param>
    /// <param name="requireIsOnline">
    /// When a resource is found, this function returns this value; True is
    /// returned when the found resource is online, false when no resource
    /// is found or this value is true and the resource is offline
    /// </param>
    /// <typeparam name="T">The type of resource to get</typeparam>
    /// <returns>See above</returns>
    bool TryGetResource<T>(out T resource, bool requireIsOnline = true) where T : ResourceItem;
}