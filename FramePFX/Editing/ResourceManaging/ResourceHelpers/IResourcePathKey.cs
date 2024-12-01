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

using System.Diagnostics.CodeAnalysis;

namespace FramePFX.Editing.ResourceManaging.ResourceHelpers;

/// <summary>
/// An interface for entries in a <see cref="ResourceHelper"/>
/// </summary>
/// <typeparam name="T">The type of resource</typeparam>
public interface IResourcePathKey<T> : IBaseResourcePathKey where T : ResourceItem {
    /// <summary>
    /// An event fired when the underlying resource being used has changed. If the new resource isn't applicable to
    /// type <see cref="T"/>, then the newItem parameter is set to null
    /// </summary>
    event EntryResourceChangedEventHandler<T> ResourceChanged;

    /// <summary>
    /// An event fired when the online state of this entry changes, meaning, a resource was linked or unlinked.
    /// This is not fired when the online state of a resource changes, that must be hooked
    /// manually (with the help of the <see cref="ResourceChanged"/> event)
    /// </summary>
    event EntryOnlineStateChangedEventHandler<T> OnlineStateChanged;

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
    bool TryGetResource([NotNullWhen(true)] out T? resource, bool requireIsOnline = true);
}