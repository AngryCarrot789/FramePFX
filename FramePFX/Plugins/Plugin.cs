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
using FramePFX.Configurations;
using FramePFX.Persistence;

namespace FramePFX.Plugins;

/// <summary>
/// The base class for a plugin's entry point class
/// </summary>
public abstract class Plugin {
    /// <summary>
    /// Gets the plugin descriptor of this plugin
    /// </summary>
    public PluginDescriptor Descriptor { get; internal set; } = null!;

    /// <summary>
    /// Gets the plugin loader that created this plugin instance
    /// </summary>
    public PluginLoader PluginLoader { get; internal set; } = null!;

    /// <summary>
    /// Gets the folder that this plugin exists in (which contains plugin.xml).
    /// If this is a core plugin (and therefore not loaded from an assembly), this will be null,
    /// in which case you may be able to default to <see cref="Environment.CurrentDirectory"/>
    /// </summary>
    public string? PluginFolder { get; internal set; }
    
    /// <summary>
    /// Gets the display name of this plugin
    /// </summary>
    public virtual string Name => this.GetType().Name;

    protected Plugin() {
    }

    /// <summary>
    /// Invoked after the plugin is created and the descriptor is set.
    /// Exceptions throw in this method will crash the entire application
    /// </summary>
    public virtual void OnCreated() {
    
    }

    /// <summary>
    /// Register this plugin's commands
    /// </summary>
    /// <param name="manager">Command manager</param>
    public virtual void RegisterCommands(CommandManager manager) {
    
    }

    /// <summary>
    /// Register this plugin's services
    /// </summary>
    public virtual void RegisterServices() {
    
    }

    /// <summary>
    /// Registers this plugin's configurations
    /// </summary>
    /// <returns></returns>
    public virtual void RegisterConfigurations(PersistentStorageManager manager) {
    
    }
    
    /// <summary>
    /// Invoked when the application has loaded. This is invoked before any editor window is created.
    /// Things like context menus, clip types, resource types, model->control mappings and so on should be registered here
    /// </summary>
    /// <returns></returns>
    public virtual Task OnApplicationLoaded() {
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Invoked when the application is about to exit. This is sort of pointless, but it exists just in case
    /// </summary>
    public virtual void OnApplicationExiting() {
    
    }

    public virtual void GetXamlResources(List<string> paths) {
        
    }
}