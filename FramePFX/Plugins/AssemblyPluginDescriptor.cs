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
/// A plugin descriptor for a plugin loaded from an assembly
/// </summary>
public class AssemblyPluginDescriptor : PluginDescriptor {
    /// <summary>
    /// Gets or sets the DLL path that contains the plugin class. This can be relative to the plugin folder,
    /// so it can just be the actual DLL's name, with .dll on the end
    /// </summary>
    public string? EntryPointLibraryPath { get; set; }

    /// <summary>
    /// Gets or sets the full type name of the class that extends <see cref="Plugin"/> 
    /// </summary>
    public string? EntryPoint { get; set; }

    public List<string>? XamlResources { get; set; }
}