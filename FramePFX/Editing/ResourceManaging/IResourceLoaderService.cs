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
/// A service for a user interface for loading resources
/// </summary>
public interface IResourceLoaderService
{
    Task<bool> TryLoadResources(BaseResource[] resources);
}

public static class ResourceLoaderServiceExtensions
{
    public static Task<bool> TryLoadResource(this IResourceLoaderService resourceLoaderService, BaseResource resource)
    {
        return resourceLoaderService.TryLoadResources(new BaseResource[] { resource });
    }
}