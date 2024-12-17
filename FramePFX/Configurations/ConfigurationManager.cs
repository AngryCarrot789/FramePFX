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

namespace FramePFX.Configurations;

/// <summary>
/// A class which exposes information about a hierarchy of configuration pages.
/// FramePFX has two: the application settings and project settings, both of which
/// are separate instances and store their own hierarchy of configuration pages
/// </summary>
public abstract class ConfigurationManager
{
    /// <summary>
    /// Gets our root configuration entry
    /// </summary>
    public ConfigurationEntry RootEntry { get; }
    
    public ConfigurationManager()
    {
        this.RootEntry = new ConfigurationEntry() { DisplayName = "<root>" };
    }

    private const int Flag_None = 0;
    private const int Flag_OnlyIfModified = 1;

    public async Task ApplyHierarchyAsync()
    {
        await ApplyPagesRecursive(this.RootEntry, (x) => x.Apply(), Flag_OnlyIfModified);
    }
    
    public async Task LoadContextAsync(ConfigurationContext context)
    {
        await ApplyPagesRecursive(this.RootEntry, (x) =>
        {
            x.IsMarkedImmediatelyModified = false;
            return x.OnContextCreated(context);
        }, Flag_None);
    }
    
    public async Task UnloadContextAsync(ConfigurationContext context)
    {
        await ApplyPagesRecursive(this.RootEntry, (x) => x.OnContextDestroyed(context), Flag_None);
    }

    private static async ValueTask ApplyPagesRecursive(ConfigurationEntry entry, Func<ConfigurationPage, ValueTask> action, int flags)
    {
        if (entry.Page != null)
        {
            // ReSharper disable once ReplaceWithSingleAssignment.True
            
            bool canExec = true;
            if ((flags & Flag_OnlyIfModified) != 0 && !entry.Page.IsModified())
                canExec = false;
            
            if (canExec)
                await action(entry.Page);
        }

        foreach (ConfigurationEntry item in entry.Items)
        {
            await ApplyPagesRecursive(item, action, flags);
        }
    }
}