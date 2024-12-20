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

using FramePFX.CommandSystem;

namespace FramePFX.Plugins;

/// <summary>
/// The base class for a plugin's entry point class
/// </summary>
public abstract class Plugin
{
    public PluginDescriptor Descriptor { get; internal set; } = null!;
    
    public PluginLoader PluginLoader { get; internal set; } = null!;

    public string PluginFolder { get; internal set; } = null!;
    
    protected Plugin()
    {
        
    }

    /// <summary>
    /// Invoked after the plugin is created and the descriptor is set
    /// </summary>
    public abstract void OnCreated();

    /// <summary>
    /// Adds this plugin's services to the given service manager
    /// </summary>
    /// <param name="manager">Service manager</param>
    public abstract void RegisterServices(ServiceManager manager);
    
    /// <summary>
    /// Register this plugin's commands
    /// </summary>
    /// <param name="manager">Command manager</param>
    public abstract void RegisterCommands(CommandManager manager);

    /// <summary>
    /// Invoked when the application has loaded. This is invoked before an editor window is created
    /// </summary>
    /// <returns></returns>
    public abstract Task OnApplicationLoaded();
}