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

using System.Xml;
using FramePFX.Shortcuts;
using FramePFX.Utils;

namespace FramePFX.Plugins.XML;

public static class PluginDescriptorParser {
    public static AssemblyPluginDescriptor Parse(Stream stream) {
        XmlDocument document = new XmlDocument();
        document.Load(stream);
        if (!(document.SelectSingleNode("/Plugin") is XmlElement pluginRoot)) {
            throw new Exception("Expected element of type 'Plugin' to be the root element for the XML document");
        }

        return new AssemblyPluginDescriptor() {
            EntryPoint = GetAttributeNullable(pluginRoot, "EntryPoint", true),
            EntryPointLibraryPath = GetAttributeNullable(pluginRoot, "EntryPointLibraryPath", true),
            XamlResources = ParseXamlResources(pluginRoot).ToList()
        };
    }

    private static IEnumerable<string> ParseXamlResources(XmlElement pluginRoot) {
        foreach (XmlElement node in pluginRoot.ChildNodes.OfType<XmlElement>()) {
            if (node.Name == "XamlResource") {
                string? sourcePath = GetAttributeNullable(node, "Source", true)?.Trim();
                if (sourcePath != null) {
                    yield return sourcePath;
                }
            }
        }
    }

    public static string? GetAttributeNullable(XmlElement element, string key, bool whitespacesToNull = true) {
        string attribute = element.GetAttribute(key);
        if (whitespacesToNull ? string.IsNullOrWhiteSpace(attribute) : string.IsNullOrEmpty(attribute))
            return null;
        return attribute;
    }

    public static string GetAttributeNonNull(IGroupedObject owner, XmlElement element, string key, bool requireNonWhitespaces = true) {
        if (!element.HasAttribute(key)) {
            throw new Exception($"'{key}' attribute must be provided, for object at path '{owner.FullPath ?? "<root>"}'");
        }

        string attribute = element.GetAttribute(key);
        if (requireNonWhitespaces) {
            if (string.IsNullOrWhiteSpace(attribute)) {
                throw new Exception($"'{key}' attribute cannot be an empty string or consist of only whitespaces, for object at path '{owner.FullPath ?? "<root>"}'");
            }
        }
        else if (attribute.Length < 1) {
            throw new Exception($"'{key}' attribute cannot be an empty string, for object at path '{owner.FullPath ?? "<root>"}'");
        }

        return attribute;
    }

    public static bool? GetBool(XmlElement element, string key) {
        string? value = GetAttributeNullable(element, key);
        return value == null ? null : "true".EqualsIgnoreCase(value);
    }
}