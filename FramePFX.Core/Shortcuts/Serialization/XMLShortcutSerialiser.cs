using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FramePFX.Core.Actions.Contexts;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Shortcuts.Serialization {
    public abstract class XMLShortcutSerialiser {
        #region Serialisation

        public XmlDocument Serialise(ShortcutGroup root) {
            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("KeyMap");
            this.SerialiseGroupData(document, element, root);
            document.AppendChild(element);
            return document;
        }

        private void SerialiseGroupData(XmlDocument doc, XmlElement groupElement, ShortcutGroup group) {
            foreach (ShortcutGroup innerGroup in group.Groups) {
                XmlElement childGroupElement = doc.CreateElement("Group");
                if (innerGroup.Name != null) // guaranteed not to be empty or only whitespaces
                    childGroupElement.SetAttribute("Name", innerGroup.Name);
                if (!string.IsNullOrWhiteSpace(innerGroup.DisplayName))
                    childGroupElement.SetAttribute("DisplayName", innerGroup.DisplayName);
                if (innerGroup.IsGlobal)
                    childGroupElement.SetAttribute("IsGlobal", "true");
                if (innerGroup.Inherit)
                    childGroupElement.SetAttribute("Inherit", "true");
                if (!string.IsNullOrWhiteSpace(innerGroup.Description))
                    childGroupElement.SetAttribute("Description", innerGroup.Description);
                this.SerialiseGroupData(doc, childGroupElement, innerGroup);
                groupElement.AppendChild(childGroupElement);
            }

            foreach (GroupedShortcut shortcut in group.Shortcuts) {
                XmlElement shortcutElement = doc.CreateElement("Shortcut");
                shortcutElement.SetAttribute("Name", shortcut.Name); // guaranteed non-null, not empty and not whitespaces
                if (!string.IsNullOrWhiteSpace(shortcut.DisplayName))
                    shortcutElement.SetAttribute("DisplayName", shortcut.DisplayName);
                if (shortcut.IsGlobal)
                    shortcutElement.SetAttribute("IsGlobal", "true");
                if (shortcut.IsInherited)
                    shortcutElement.SetAttribute("Inherit", "true");
                if (!string.IsNullOrWhiteSpace(shortcut.ActionId))
                    shortcutElement.SetAttribute("ActionId", shortcut.ActionId);
                if (!string.IsNullOrWhiteSpace(shortcut.Description))
                    shortcutElement.SetAttribute("Description", shortcut.Description);
                if (shortcut.ActionContext != null)
                    this.SerialiseContext(doc, shortcutElement, shortcut.ActionContext);

                if (!shortcut.Shortcut.IsEmpty) {
                    // trust that IsEmpty is correct
                    foreach (IInputStroke stroke in shortcut.Shortcut.InputStrokes) {
                        if (stroke is MouseStroke ms) {
                            this.SerialiseMousestroke(doc, shortcutElement, ms);
                        }
                        else if (stroke is KeyStroke ks) {
                            this.SerialiseKeystroke(doc, shortcutElement, ks);
                        }
                        else {
                            throw new Exception($"Unexpected input stroke: {stroke} ({stroke?.GetType()})");
                        }
                    }
                }

                groupElement.AppendChild(shortcutElement);
            }
        }

        protected void SerialiseContext(XmlDocument doc, XmlElement shortcutElement, DataContext context) {
            if (context.InternalDataMap != null && context.InternalDataMap.Count > 0) {
                List<string> flags = new List<string>();
                List<KeyValuePair<string, string>> entries = new List<KeyValuePair<string, string>>();
                foreach (KeyValuePair<string, object> pair in context.InternalDataMap) {
                    if (string.IsNullOrWhiteSpace(pair.Key)) {
                        continue;
                    }

                    if (pair.Value is bool flag) {
                        if (flag) {
                            flags.Add(pair.Key);
                        }
                        else {
                            entries.Add(new KeyValuePair<string, string>(pair.Key, "false"));
                        }
                    }
                    else if (pair.Value is string str) {
                        // allow empty strings
                        entries.Add(new KeyValuePair<string, string>(pair.Key, str));
                    }
                    else {
                        throw new Exception($"Context entry with key '{pair.Key}' was not a string: {pair.Value}");
                    }
                }

                XmlElement contextElement = doc.CreateElement("Shortcut.Context");
                if (flags.Count > 0) {
                    XmlElement element = doc.CreateElement("Flags");
                    element.InnerText = string.Join(" ", flags);
                    contextElement.AppendChild(element);
                }

                if (entries.Count > 0) {
                    foreach (KeyValuePair<string, string> pair in entries) {
                        XmlElement element = doc.CreateElement("Flag");
                        element.SetAttribute("Key", pair.Key);
                        element.SetAttribute("Value", pair.Value);
                        contextElement.AppendChild(element);
                    }
                }

                if (contextElement.ChildNodes.Count > 0) {
                    shortcutElement.AppendChild(contextElement);
                }
            }
        }

        #endregion

        #region Deserialisation

        public ShortcutGroup Deserialise(string filePath) {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);
            return this.Deserialise(document);
        }

        public ShortcutGroup Deserialise(Stream stream) {
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            return this.Deserialise(document);
        }

        public ShortcutGroup Deserialise(XmlDocument document) {
            ShortcutGroup root = ShortcutGroup.CreateRoot();
            if (!(document.SelectSingleNode("/KeyMap") is XmlElement rootElement)) {
                throw new Exception("Expected element of type 'KeyMap' to be the root element for the XML document");
            }

            this.DeserialiseGroupData(rootElement, root);
            return root;
        }

        public void DeserialiseGroupData(XmlElement element, ShortcutGroup group) {
            foreach (XmlElement child in element.ChildNodes.OfType<XmlElement>()) {
                string name = child.GetAttribute("Name");
                if (string.IsNullOrWhiteSpace(name)) {
                    throw new Exception($"Invalid 'Name' attribute for element in group '{group.FullPath ?? "<root>"}'");
                }

                DataContext context = null;
                switch (child.Name) {
                    case "Group": {
                        ShortcutGroup innerGroup = @group.CreateGroupByName(name, GetIsGlobal(child), GetIsInherit(child));
                        innerGroup.Description = GetDescription(child);
                        innerGroup.DisplayName = GetDisplayName(child);
                        this.DeserialiseGroupData(child, innerGroup);
                        break;
                    }
                    case "Shortcut": {
                        List<IInputStroke> inputs = new List<IInputStroke>();
                        foreach (XmlElement innerElement in child.ChildNodes.OfType<XmlElement>()) {
                            switch (innerElement.Name) {
                                case "KeyStroke":
                                case "Keystroke":
                                case "keystroke":
                                    inputs.Add(this.DeserialiseKeyStroke(innerElement)); break;
                                case "MouseStroke":
                                case "Mousestroke":
                                case "mousestroke":
                                    inputs.Add(this.DeserialiseMouseStroke(innerElement)); break;
                                case "Shortcut.Context":
                                case "Shortcut.context": {
                                    if (innerElement.ChildNodes.Count < 1) {
                                        break;
                                    }

                                    context = new DataContext();
                                    foreach (XmlElement contextNode in innerElement.ChildNodes.OfType<XmlElement>()) {
                                        if (contextNode.Name.Equals("flags", StringComparison.OrdinalIgnoreCase)) {
                                            string flags = contextNode.InnerText;
                                            if (string.IsNullOrWhiteSpace(flags)) {
                                                throw new Exception($"Missing or invalid flags string");
                                            }

                                            foreach (string flag in flags.Split(' ')) {
                                                if (!string.IsNullOrWhiteSpace(flag)) {
                                                    context.Set(flag, true);
                                                }
                                            }
                                        }
                                        else {
                                            bool isBoolFlag = contextNode.Name.Equals("flag", StringComparison.OrdinalIgnoreCase);
                                            if (isBoolFlag || contextNode.Name.Equals("entry", StringComparison.OrdinalIgnoreCase)) {
                                                string key = GetAttributeNullable(contextNode, "Key");
                                                string value = GetAttributeNullable(contextNode, "Value");
                                                if (string.IsNullOrEmpty(key)) {
                                                    throw new Exception($"Invalid flag key. Got '{key}'");
                                                }

                                                if (isBoolFlag) {
                                                    if ("true".Equals(value, StringComparison.OrdinalIgnoreCase)) {
                                                        context.Set(key, BoolBox.True);
                                                    }
                                                    else if ("false".Equals(value, StringComparison.OrdinalIgnoreCase)) {
                                                        context.Set(key, BoolBox.False);
                                                    }
                                                    else {
                                                        throw new Exception($"Invalid flag value. Expected 'true' or 'false', but got '{value}'");
                                                    }
                                                }
                                                else {
                                                    context.Set(key, value ?? "");
                                                }
                                            }
                                        }
                                    }

                                    if (context.InternalDataMap == null || context.InternalDataMap.Count < 1) {
                                        context = null;
                                    }

                                    break;
                                }
                            }
                        }

                        List<KeyStroke> keyStrokes = inputs.OfType<KeyStroke>().ToList();
                        List<MouseStroke> mouseStrokes = inputs.OfType<MouseStroke>().ToList();
                        IShortcut shortcut;
                        if (mouseStrokes.Count > 0 && keyStrokes.Count > 0) {
                            shortcut = new MouseKeyboardShortcut(inputs);
                        }
                        else if (keyStrokes.Count > 0) {
                            shortcut = new KeyboardShortcut(keyStrokes);
                        }
                        else if (mouseStrokes.Count > 0) {
                            shortcut = new MouseShortcut(mouseStrokes);
                        }
                        else {
                            continue;
                        }

                        GroupedShortcut managed = @group.AddShortcut(name, shortcut, GetIsGlobal(child), GetIsInherit(child));
                        managed.ActionId = GetAttributeNullable(child, "ActionId");
                        managed.Description = GetDescription(child);
                        managed.DisplayName = GetDisplayName(child);
                        managed.ActionContext = context;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Util functions

        protected static bool GetIsGlobal(XmlElement element) { // false by default
            string attrib = element.GetAttribute("IsGlobal");
            if (string.IsNullOrWhiteSpace(attrib))
                attrib = element.GetAttribute("Global");
            return !string.IsNullOrWhiteSpace(attrib) && attrib.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        protected static bool GetIsInherit(XmlElement element) { // true by default
            string attrib = element.GetAttribute("IsInherit");
            if (string.IsNullOrWhiteSpace(attrib))
                attrib = element.GetAttribute("Inherit");
            return string.IsNullOrWhiteSpace(attrib) || attrib.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        protected static string GetDescription(XmlElement element) {
            return GetAttributeNullable(element, "Description");
        }

        protected static string GetDisplayName(XmlElement element) {
            return GetAttributeNullable(element, "DisplayName");
        }

        protected static string GetAttributeNullable(XmlElement element, string key) {
            XmlAttribute node = element.GetAttributeNode(key);
            if (node == null)
                return null;
            string value = node.Value;
            return string.IsNullOrEmpty(value) ? null : value;
        }

        #endregion

        protected abstract KeyStroke DeserialiseKeyStroke(XmlElement element);
        protected abstract MouseStroke DeserialiseMouseStroke(XmlElement element);
        protected abstract void SerialiseKeystroke(XmlDocument doc, XmlElement shortcut, in KeyStroke stroke);
        protected abstract void SerialiseMousestroke(XmlDocument doc, XmlElement shortcut, in MouseStroke stroke);
    }
}