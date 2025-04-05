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
using System.Text;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Persistence;
using PFXToolKitUI.Plugins.Exceptions;
using PFXToolKitUI.Plugins.XML;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Plugins;

public sealed class PluginLoader {
    private readonly List<Plugin> plugins;
    private List<CorePluginDescriptor>? corePlugins;
    private HashSet<Type> loadedPluginTypes; // is this a bad idea?

    public ReadOnlyCollection<Plugin> Plugins { get; }

    public PluginLoader() {
        this.plugins = new List<Plugin>();
        this.Plugins = this.plugins.AsReadOnly();
        this.loadedPluginTypes = new HashSet<Type>();
    }

    /// <summary>
    /// Adds a core plugin descriptor to the list of plugins to load once the application begins
    /// loading plugins. A core plugin is a plugin directly referenced by FramePFX, rather than
    /// being completely dynamically loaded
    /// </summary>
    /// <param name="descriptor">the descriptor</param>
    public void AddCorePluginEntry(CorePluginDescriptor descriptor) {
        Validate.NotNull(descriptor);
        (this.corePlugins ??= new List<CorePluginDescriptor>()).Add(descriptor);
    }

    /// <summary>
    /// Loads all registered core plugins
    /// </summary>
    /// <param name="exceptions">A list of exceptions encountered</param>
    public void LoadCorePlugins(List<BasePluginLoadException> exceptions) {
        if (this.corePlugins == null) {
            return;
        }

        foreach (CorePluginDescriptor descriptor in this.corePlugins) {
            if (this.loadedPluginTypes.Contains(descriptor.PluginType)) {
                exceptions.Add(new BasePluginLoadException("Plugin type already in use: " + descriptor.PluginType));
            }
            else {
                Plugin? instance;
                try {
                    instance = (Plugin?) Activator.CreateInstance(descriptor.PluginType) ?? throw new InvalidOperationException($"Failed to create plugin instance of type {descriptor.PluginType}");
                }
                catch (Exception e) {
                    exceptions.Add(new BasePluginLoadException("Failed to create instance of plugin", e));
                    continue;
                }

                this.OnPluginCreated(null, instance, descriptor);
            }
        }

        this.corePlugins.Clear();
        this.corePlugins = null;
    }

    /// <summary>
    /// Loads all dynamic plugins. This can be called multiple times with different target folders. Don't call twice with the same folder!
    /// </summary>
    /// <param name="pluginsFolder">The plugins folder to load from</param>
    /// <param name="exceptions">A list of exceptions encountered</param>
    public async Task LoadPlugins(string pluginsFolder, List<BasePluginLoadException> exceptions) {
        string[] dirs;
        try {
            // Plugins use relative paths, so we need to give them the full path so they can do their thing.
            // By using the full path here, it ensures GetDirectories returns full paths... hopefully
            string fullPath = Path.GetFullPath(pluginsFolder);
            dirs = Directory.Exists(fullPath) ? Directory.GetDirectories(fullPath) : [];
        }
        catch {
            // Plugins dir doesn't exist maybe
            dirs = [];
        }

        foreach (string folder in dirs) {
            (Plugin, AssemblyPluginDescriptor)? info = null;
            try {
                info = await this.ReadDescriptorAndCreatePluginInstance(folder);
            }
            catch (BasePluginLoadException e) {
                exceptions.Add(e);
            }
            catch (Exception e) {
                exceptions.Add(new UnknownPluginLoadException(e));
            }

            if (info.HasValue) {
                this.OnPluginCreated(folder, info.Value.Item1, info.Value.Item2);
            }
        }
    }

    public void RegisterCommands(CommandManager manager) {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterCommands(manager);
        }
    }

    public void RegisterServices() {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterServices();
        }
    }

    /// <summary>
    /// Gets all of the xaml files that plugins require to be injected into the application resources.
    /// This gives absolute paths
    /// </summary>
    /// <returns></returns>
    public List<(Plugin, string)> GetInjectableXamlResources() {
        List<(Plugin, string)> fullPaths = new List<(Plugin, string)>();
        foreach (Plugin plugin in this.plugins) {
            List<string> pluginPaths = new List<string>();
            plugin.GetXamlResources(pluginPaths);

            if (pluginPaths.Count > 0) {
                string? asmFullName = null;
                if (plugin.Descriptor is AssemblyPluginDescriptor descriptor) {
                    asmFullName = Path.GetDirectoryName(descriptor.EntryPointLibraryPath);
                }

                if (string.IsNullOrWhiteSpace(asmFullName)) {
                    Assembly asm = plugin.GetType().Assembly;
                    if ((asmFullName = asm.GetName().Name) == null) {
                        asmFullName = plugin.GetType().Namespace;
                    }
                }

                if (string.IsNullOrWhiteSpace(asmFullName)) {
                    AppLogger.Instance.WriteLine($"Could not identify {plugin.Name}'s assembly name");
                    continue;
                }

                foreach (string path in pluginPaths) {
                    if (!string.IsNullOrWhiteSpace(path)) {
                        StringBuilder sb = new StringBuilder();
                        sb.Append("avares://").Append(asmFullName);
                        if (path[0] != '/') {
                            sb.Append('/');
                        }

                        sb.Append(path);
                        fullPaths.Add((plugin, sb.ToString()));
                    }
                }
            }
        }

        return fullPaths;
    }

    private void OnPluginCreated(string? pluginFolder, Plugin plugin, PluginDescriptor descriptor) {
        this.plugins.Add(plugin);
        plugin.Descriptor = descriptor;
        plugin.PluginLoader = this;
        plugin.PluginFolder = pluginFolder;
        plugin.OnCreated();
    }

    private async Task<(Plugin, AssemblyPluginDescriptor)> ReadDescriptorAndCreatePluginInstance(string folder) {
        string path = Path.Combine(folder, "plugin.xml");
        AssemblyPluginDescriptor descriptor;

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
            descriptor.EntryPointLibraryPath = Path.Combine(folder, descriptor.EntryPointLibraryPath);
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

        if (this.loadedPluginTypes.Contains(entryType)) {
            throw new BasePluginLoadException("Plugin type already in use: " + entryType);
        }
        else {
            Plugin plugin = (Plugin) Activator.CreateInstance(entryType)! ?? throw new Exception("Failed to instantiate plugin");
            return (plugin, descriptor);
        }
    }

    public void RegisterConfigurations(PersistentStorageManager manager) {
        foreach (Plugin plugin in this.plugins) {
            plugin.RegisterConfigurations(manager);
        }
    }

    public async Task OnApplicationFullyLoaded() {
        foreach (Plugin plugin in this.plugins) {
            await plugin.OnApplicationFullyLoaded();
        }
    }

    public void OnApplicationExiting() {
        foreach (Plugin plugin in this.plugins) {
            plugin.OnApplicationExiting();
        }
    }
}