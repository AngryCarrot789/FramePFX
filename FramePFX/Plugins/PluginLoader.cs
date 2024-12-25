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

using System.Collections.ObjectModel;
using System.Reflection;
using FramePFX.CommandSystem;
using FramePFX.Plugins.Exceptions;
using FramePFX.Plugins.XML;

namespace FramePFX.Plugins;

public class PluginLoader {
    private readonly List<Plugin> plugins;

    public ReadOnlyCollection<Plugin> Plugins { get; }

    public PluginLoader() {
        this.plugins = new List<Plugin>();
        this.Plugins = this.plugins.AsReadOnly();
    }

    public async Task LoadPlugins(string pluginsFolder, List<BasePluginLoadException> exceptions) {
        foreach (string folder in Directory.GetDirectories(pluginsFolder)) {
            (Plugin, PluginDescriptor)? info = null;
            try {
                info = await LoadPlugin(folder);
            }
            catch (BasePluginLoadException e) {
                exceptions.Add(e);
            }
            catch (Exception e) {
                exceptions.Add(new UnknownPluginLoadException(e));
            }

            if (info.HasValue) {
                this.OnPluginCreated(pluginsFolder, info.Value.Item1, info.Value.Item2);
            }
        }
    }

    public void RegisterCommands(CommandManager manager) {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterCommands(manager);
        }
    }

    private void OnPluginCreated(string pluginsFolder, Plugin plugin, PluginDescriptor descriptor) {
        List<string>? list = descriptor.XamlResources;
        if (list?.Count > 0) {
            for (int i = list.Count - 1; i >= 0; i--) {
                list[i] = Path.GetFullPath(Path.Combine(pluginsFolder, list[i]));
            }
        }

        this.plugins.Add(plugin);
        plugin.Descriptor = descriptor;
        plugin.PluginLoader = this;
        plugin.PluginFolder = pluginsFolder;
        plugin.OnCreated();
    }

    private static async Task<(Plugin, PluginDescriptor)> LoadPlugin(string folder) {
        string path = Path.Combine(folder, "plugin.xml");
        PluginDescriptor descriptor;

        try {
            await using FileStream stream = new FileStream(path, FileMode.Open);
            descriptor = PluginDescriptorParser.Parse(stream);
        }
        catch (Exception e) {
            throw new InvalidPluginDescriptorException(e);
        }

        if (descriptor.EntryPointLibraryPath == null)
            throw new MissingPluginEntryLibraryException();
        if (descriptor.EntryPoint == null)
            throw new MissingPluginEntryClassException();

        Assembly assembly;
        try {
            assembly = Assembly.LoadFrom(descriptor.EntryPointLibraryPath);
        }
        catch (Exception e) {
            throw new PluginAssemblyLoadException(e);
        }

        Type? entryType;
        try {
            entryType = assembly.GetType(descriptor.EntryPoint, true, false);
            if (entryType == null)
                throw new Exception("No such type");
        }
        catch (Exception e) {
            throw new NoSuchEntryTypeException(descriptor.EntryPoint, e);
        }

        if (entryType.IsInterface || entryType.IsAbstract) {
            throw new Exception("Plugin entry class must not be abstract or an interface");
        }

        Plugin plugin = (Plugin) Activator.CreateInstance(entryType)! ?? throw new Exception("Failed to instantiate plugin");

        return (plugin, descriptor);
    }

    public async Task OnApplicationInitialised() {
        foreach (Plugin plugin in this.plugins) {
            await plugin.OnApplicationLoaded();
        }
    }
}