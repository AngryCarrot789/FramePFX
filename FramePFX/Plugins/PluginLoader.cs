using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FramePFX.Utils;

namespace FramePFX.Plugins {
    public class PluginLoader {
        private readonly List<Plugin> plugins;

        public PluginLoader() {
            this.plugins = new List<Plugin>();
        }

        public void LoadPlugins(string folder) {
            List<string> dlls = new List<string>();
            foreach (string pluginFolder in Directory.EnumerateDirectories(folder)) {
                string listFile = Path.Combine(pluginFolder, "plugin-list.txt");
                if (!File.Exists(listFile)) {
                    AppLogger.WriteLine("Skipping plugin folder due to missing plugin-list.txt at " + pluginFolder);
                    continue;
                }

                string[] lines = File.ReadAllLines(listFile);
                foreach (string line in lines) {
                    string dll = Path.Combine(pluginFolder, line.EndsWith(".dll") ? line : (line + ".dll"));
                    if (File.Exists(dll)) {
                        dlls.Add(dll);
                    }
                    else {
                        AppLogger.WriteLine("Skipping non-existent plugin DLL at " + dll);
                    }
                }
            }

            List<(RegistrationOrderAttribute order, Type type)> types = new List<(RegistrationOrderAttribute, Type)>();
            foreach (string path in dlls) {
                Assembly assembly;
#if !DEBUG
                try {
#endif
                assembly = Assembly.LoadFile(path);
#if !DEBUG
                }
                catch (Exception e) {
                    AppLogger.WriteLine("Failed to load plugin assembly at " + path + ".\n" + e.GetToString());
                    continue;
                }
#endif

                foreach (Type type in assembly.GetTypes()) {
                    if (typeof(Plugin).IsAssignableFrom(type) && type.GetCustomAttribute<IgnoredPluginAttribute>() == null) {
                        types.Add((type.GetCustomAttribute<RegistrationOrderAttribute>(), type));
                    }
                }
            }

            types.Sort((a, b) => {
                if (a.order == null) {
                    return b.order?.Order ?? 0;
                }
                else if (b.order == null) {
                    return a.order.Order;
                }
                else {
                    return a.order.Order.CompareTo(b.order.Order);
                }
            });

            foreach ((RegistrationOrderAttribute order, Type type) in types) {
#if DEBUG
                try {
#endif
                    Plugin plugin = (Plugin) Activator.CreateInstance(type);
                    plugin.OnConstructed(this);
                    this.plugins.Add(plugin);
#if DEBUG
                }
                catch (Exception e) {
                    AppLogger.WriteLine("Failed to load plugin of type " + type + ".\n" + e.GetToString());
                }
#endif
            }

            foreach (Plugin plugin in this.plugins) {
                try {
                    plugin.OnLoad(this);
                }
                catch (Exception e) {
                    AppLogger.WriteLine("Failed to invoke plugin load handler for type " + plugin.GetType() + ". It will be removed\n" + e.GetToString());
                    try {
                        plugin.OnUnload(this);
                    }
                    catch (Exception ex) {
                        AppLogger.WriteLine("Failed to also call the plugin's OnUnload function\n" + ex.GetToString());
                    }
                }
            }
        }
    }
}