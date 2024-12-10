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

using System.Diagnostics.CodeAnalysis;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.Timelines;
using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Utils;

namespace FramePFX.Editing;

public sealed class ResourceClipRegistry
{
    private readonly Dictionary<Type, IResourceDropInformation> information;

    public ResourceClipRegistry()
    {
        this.information = new Dictionary<Type, IResourceDropInformation>();
    }

    public void Register(Type resourceType, IResourceDropInformation info)
    {
        Validate.NotNull(resourceType);
        Validate.NotNull(info);

        if (!typeof(ResourceItem).IsAssignableFrom(resourceType))
            throw new ArgumentException("Resource type is not an instance of " + nameof(ResourceItem));
        
        this.information[resourceType] = info;
    }

    public bool TryGetValue(Type key, [NotNullWhen(true)] out IResourceDropInformation? value) => this.information.TryGetValue(key, out value);
}

public interface IResourceDropInformation
{
    long GetClipDurationForDrop(ResourceItem resource);
    
    Task OnDroppedInTrack(Track track, ResourceItem resource, FrameSpan span);
}