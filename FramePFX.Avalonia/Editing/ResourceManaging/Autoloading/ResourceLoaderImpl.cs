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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Avalonia.Controls;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Editing.ResourceManaging.Autoloading;

namespace FramePFX.Avalonia.Editing.ResourceManaging.Autoloading;

public class ResourceLoaderDialogServiceImpl : IResourceLoaderDialogService
{
    public async Task<bool> TryLoadResources(BaseResource[] resources)
    {
        ImmutableList<BaseResource> list = resources.ToImmutableList();
        ResourceLoader loader = new ResourceLoader();
        await LoadResources(list, loader);
        if (loader.Entries.Count < 1)
        {
            return true;
        }

        if (ApplicationImpl.TryGetActiveWindow(out Window? window))
        {
            ResourceLoaderDialog dialog = new ResourceLoaderDialog();
            dialog.ResourceLoader = loader;
            bool? result = await dialog.ShowDialog<bool?>(window);
            if (result == true)
            {
                return true;
            }
        }

        return false;
    }

    private static async ValueTask LoadResources(IEnumerable<BaseResource> resources, ResourceLoader loader)
    {
        foreach (BaseResource obj in resources)
        {
            if (obj is ResourceFolder folder)
            {
                await LoadResources(folder.Items, loader);
            }
            else
            {
                ResourceItem item = (ResourceItem) obj;
                if (!item.IsOnline)
                {
                    await item.TryAutoEnable(loader);
                }
            }
        }
    }
}