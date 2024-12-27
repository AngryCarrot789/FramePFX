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

namespace FramePFX.Plugins;

/// <summary>
/// A plugin descriptor for an internal or 'core' plugin. These are inside the main FramePFX project itself
/// </summary>
public class CorePluginDescriptor : PluginDescriptor {
    public Type PluginType { get; }

    public CorePluginDescriptor(Type? pluginType) {
        if (!typeof(Plugin).IsAssignableFrom(pluginType))
            throw new ArgumentException($"The plugin type is not an instance of {nameof(Plugin)}: '{pluginType}'");
        
        if (pluginType.IsInterface || pluginType.IsAbstract)
            throw new ArgumentException($"The plugin type is an interface or abstract: '{pluginType}'");
        
        this.PluginType = pluginType;
    }
}