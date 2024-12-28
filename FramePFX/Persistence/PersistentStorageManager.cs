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

using System.Diagnostics;
using System.Xml;
using FramePFX.Logging;
using FramePFX.Utils;

namespace FramePFX.Persistence;

/// <summary>
/// The service which manages all application-wide persistent configurations
/// </summary>
public sealed class PersistentStorageManager {
    private readonly List<PersistentConfiguration> allConfigs;
    private readonly Dictionary<string, Dictionary<string, PersistentConfiguration>> areaMap;
    private readonly Dictionary<Type, PersistentConfiguration> typeMap;

    /// <summary>
    /// Gets the location of the configuration storage directory
    /// </summary>
    public string StorageDirectory { get; }

    public PersistentStorageManager(string storageDirectory) {
        Validate.NotNullOrWhiteSpaces(storageDirectory);

        this.allConfigs = new List<PersistentConfiguration>();
        this.areaMap = new Dictionary<string, Dictionary<string, PersistentConfiguration>>();
        this.typeMap = new Dictionary<Type, PersistentConfiguration>();
        this.StorageDirectory = storageDirectory;
    }
    
    public T GetConfiguration<T>() where T : PersistentConfiguration {
        return (T) this.typeMap[typeof(T)];
    }

    /// <summary>
    /// Registers a persistent configuration with the given name and area
    /// </summary>
    /// <param name="config">The config</param>
    /// <param name="area">The name of the area</param>
    /// <param name="name">The name of the configuration</param>
    public void Register(PersistentConfiguration config, string? area, string name) {
        Validate.NotNull(config);
        Validate.NotNullOrWhiteSpaces(area ??= "application");
        Validate.NotNullOrWhiteSpaces(name);

        if (area.Contains("..")) {
            throw new InvalidOperationException("storage path cannot contain '..'");
        }

        if (area.StartsWith('/') || area.StartsWith('\\')) {
            throw new InvalidOperationException("storage path cannot start with the root directory character");
        }

        if (!this.areaMap.TryGetValue(area, out Dictionary<string, PersistentConfiguration>? configMap)) {
            this.areaMap[area] = configMap = new Dictionary<string, PersistentConfiguration>();
        }
        else if (configMap.TryGetValue(name, out PersistentConfiguration? existingConfig)) {
            throw new InvalidOperationException($"Config already registered in the option set with the name '{name}' of type '{existingConfig.GetType()}'");
        }

        Debug.Assert(!this.allConfigs.Contains(config), "Config should not exist in the list");

        configMap.Add(name, config);
        this.allConfigs.Add(config);
        this.typeMap[config.GetType()] = config;
        PersistentConfiguration.InternalOnRegistered(config, this, area, name);
    }

    /// <summary>
    /// Loads all configurations from the system
    /// </summary>
    public async Task LoadAllAsync(List<string>? missingConfigSets) {
        try {
            Directory.CreateDirectory(this.StorageDirectory);
        }
        catch {
            // ignored
        }

        foreach (KeyValuePair<string, Dictionary<string, PersistentConfiguration>> areaEntry in this.areaMap) {
            if (areaEntry.Value.Count < 1) {
                continue;
            }

            string configFilePath = Path.GetFullPath(Path.Combine(this.StorageDirectory, areaEntry.Key + ".xml"));
            if (!File.Exists(configFilePath)) {
                missingConfigSets?.Add(areaEntry.Key);
                continue;
            }

            FileStream fileStream;
            try {
                fileStream = File.OpenRead(configFilePath);
            }
            catch (Exception e) {
                missingConfigSets?.Add(areaEntry.Key);
                continue;
            }

            XmlDocument document;
            try {
                await using Stream stream = new BufferedStream(fileStream);
                document = new XmlDocument();
                document.Load(stream);
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine($"Error reading configuration XML file at {configFilePath}:\n" + e.GetToString());
                continue;
            }

            if (!(document.SelectSingleNode("/ConfigurationArea") is XmlElement rootElement)) {
                throw new Exception("Expected element of type 'KeyMap' to be the root element for the XML document");
            }

            Dictionary<string, XmlElement> configToElementMap = rootElement.GetElementsByTagName("Configuration").OfType<XmlElement>().Select(x => {
                if (x.GetAttribute("name") is string configName && !string.IsNullOrWhiteSpace(configName)) {
                    return new KeyValuePair<string, XmlElement>(configName, x);
                }
                else {
                    return default;
                }
            }).Where(x => x.Value != null!).ToDictionary();

            foreach (KeyValuePair<string, PersistentConfiguration> configMap in areaEntry.Value) {
                if (configToElementMap.TryGetValue(configMap.Key, out XmlElement? configElement)) {
                    // TODO: versioning
                    LoadConfiguration(configMap.Value, configElement);
                }
            }
        }
    }
    
    private static void LoadConfiguration(PersistentConfiguration config, XmlElement configElement) {
        Dictionary<string, XmlElement> propertyToElementMap = configElement.GetElementsByTagName("Property").OfType<XmlElement>().Select(x => {
            if (x.GetAttribute("name") is string configName && !string.IsNullOrWhiteSpace(configName)) {
                return new KeyValuePair<string, XmlElement>(configName, x);
            }
            else {
                return default;
            }
        }).Where(x => x.Value != null!).ToDictionary();
        
        foreach (PersistentProperty property in config.GetProperties()) {
            if (propertyToElementMap.TryGetValue(property.Name, out XmlElement? propertyElement)) {
                property.Deserialize(config, propertyElement);
            }
        }
    }

    public void SaveAll() {
        foreach (string area in this.areaMap.Keys) {
            this.SaveArea(area);
        }
    }

    public void SaveArea(string area) {
        if (this.areaMap.TryGetValue(area, out Dictionary<string, PersistentConfiguration>? configSet) && configSet.Count > 0) {
            try {
                Directory.CreateDirectory(this.StorageDirectory);
            }
            catch {
                // ignored
            }

            string configFilePath = Path.GetFullPath(Path.Combine(this.StorageDirectory, area + ".xml"));
            XmlDocument document = new XmlDocument();

            XmlElement rootElement = document.CreateElement("ConfigurationArea");
            document.AppendChild(rootElement);

            foreach (KeyValuePair<string, PersistentConfiguration> config in configSet) {
                SaveConfiguration(config.Value, document, rootElement);
            }

            try {
                document.Save(configFilePath);
            }
            catch (Exception e) {
                AppLogger.Instance.WriteLine($"Failed to save configuration at {configFilePath}: " + e.GetToString());
            }
        }
    }

    private static void SaveConfiguration(PersistentConfiguration config, XmlDocument document, XmlElement rootElement) {
        XmlElement configElement = (XmlElement) rootElement.AppendChild(document.CreateElement("Configuration"))!;
        configElement.SetAttribute("name", config.Name);

        foreach (PersistentProperty property in config.GetProperties()) {
            XmlElement propertyElement = document.CreateElement("Property");
            propertyElement.SetAttribute("name", property.Name);
            if (property.Serialize(config, document, propertyElement)) {
                configElement.AppendChild(propertyElement);
            }
        }
    }

    public void SaveArea(PersistentConfiguration configuration) => this.SaveArea(configuration.Area);
}