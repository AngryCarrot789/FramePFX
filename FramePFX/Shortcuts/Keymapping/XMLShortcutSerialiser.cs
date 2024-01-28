using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using FramePFX.Shortcuts.Inputs;
using FramePFX.Shortcuts.Managing;
using FramePFX.Utils;

namespace FramePFX.Shortcuts.Keymapping {
    public abstract class XMLShortcutSerialiser : IKeymapSerialiser {
        #region Serialisation

        public void Serialise(Keymap keymap, Stream stream) {
            if (keymap.Root == null)
                throw new Exception("Missing keymap group");

            XmlDocument document = new XmlDocument();
            XmlElement element = document.CreateElement("KeyMap");
            element.SetAttribute("Version", keymap.Version.ToString());
            this.SerialiseGroupData(document, element, keymap.Root);
            document.AppendChild(element);
            document.Save(stream);
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
                if (!shortcut.IsInherited) // inherit is true by default, so only serialise if explicitly false
                    shortcutElement.SetAttribute("Inherit", "false");
                switch (shortcut.RepeatMode) {
                    case RepeatMode.NonRepeat:
                        shortcutElement.SetAttribute("RepeatMode", "NonRepeat");
                        break;
                    case RepeatMode.RepeatOnly:
                        shortcutElement.SetAttribute("RepeatMode", "RepeatOnly");
                        break;
                }

                if (!string.IsNullOrWhiteSpace(shortcut.ActionId))
                    shortcutElement.SetAttribute("ActionId", shortcut.ActionId);
                if (!string.IsNullOrWhiteSpace(shortcut.Description))
                    shortcutElement.SetAttribute("Description", shortcut.Description);

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
                            throw new Exception($"Unknown input stroke: {stroke} ({stroke?.GetType()})");
                        }
                    }
                }

                groupElement.AppendChild(shortcutElement);
            }

            foreach (GroupedInputState state in group.InputStates) {
                bool isEqualAbsolutely;
                XmlElement elem = doc.CreateElement("InputState");
                elem.SetAttribute("Name", state.Name);
                if (state.ActivationStroke is KeyStroke ka) {
                    if (state.DeactivationStroke is KeyStroke kd) {
                        if ((isEqualAbsolutely = ka.Equals(kd)) || ka.EqualsExceptRelease(kd)) {
                            if (isEqualAbsolutely)
                                elem.SetAttribute("CanToggle", "true");
                            KeyStroke stroke = new KeyStroke(ka.KeyCode, ka.Modifiers, false);
                            this.SerialiseKeystroke(doc, elem, stroke, "InputState.ActivationKeyStroke");
                        }
                        else {
                            this.SerialiseKeystroke(doc, elem, in ka, "InputState.ActivationKeyStroke");
                            this.SerialiseKeystroke(doc, elem, in kd, "InputState.DeactivationKeyStroke");
                        }
                    }
                    else if (state.DeactivationStroke is MouseStroke md) {
                        this.SerialiseKeystroke(doc, elem, in ka, "InputState.ActivationKeyStroke");
                        this.SerialiseMousestroke(doc, elem, in md, "InputState.DeactivationMouseStroke");
                    }
                    else {
                        throw new Exception($"Unknown deactivation stroke: {state.DeactivationStroke}");
                    }
                }
                else if (state.ActivationStroke is MouseStroke ma) {
                    if (state.DeactivationStroke is MouseStroke md) {
                        if ((isEqualAbsolutely = ma.Equals(md)) || ma.EqualsExceptRelease(md)) {
                            if (isEqualAbsolutely)
                                elem.SetAttribute("CanToggle", "true");
                            MouseStroke stroke = new MouseStroke(ma.MouseButton, ma.Modifiers, false, ma.ClickCount, ma.WheelDelta);
                            this.SerialiseMousestroke(doc, elem, in stroke, "InputState.ActivationMouseStroke");
                        }
                        else {
                            this.SerialiseMousestroke(doc, elem, in ma, "InputState.ActivationMouseStroke");
                            this.SerialiseMousestroke(doc, elem, in md, "InputState.DeactivationMouseStroke");
                        }
                    }
                    else if (state.DeactivationStroke is KeyStroke kd) {
                        this.SerialiseMousestroke(doc, elem, in ma, "InputState.ActivationMouseStroke");
                        this.SerialiseKeystroke(doc, elem, in kd, "InputState.DeactivationKeyStroke");
                    }
                    else {
                        throw new Exception($"Unknown deactivation stroke: {state.DeactivationStroke}");
                    }
                }
                else {
                    throw new Exception($"Unknown activation stroke: {state.ActivationStroke}");
                }

                if (state.StateManager != null) {
                    if (state.StateManager.Id.Equals(state.Parent.FullPath)) {
                        elem.SetAttribute("UseGroupAsManager", "true");
                    }
                    else {
                        elem.SetAttribute("StateManager", state.StateManager.Id);
                    }
                }

                groupElement.AppendChild(elem);
            }
        }

        #endregion

        #region Deserialisation

        public Keymap Deserialise(ShortcutManager manager, Stream stream) {
            XmlDocument document = new XmlDocument();
            document.Load(stream);
            if (!(document.SelectSingleNode("/KeyMap") is XmlElement rootElement)) {
                throw new Exception("Expected element of type 'KeyMap' to be the root element for the XML document");
            }

            string version = GetAttributeNullable(rootElement, "Version");
            Version keymapVersion = !string.IsNullOrEmpty(version) ? Version.Parse(version) : new Version(1, 0, 0);

            ShortcutGroup root = ShortcutGroup.CreateRoot(manager);
            this.DeserialiseGroupData(rootElement, root);
            return new Keymap() {
                Version = keymapVersion,
                Root = root
            };
        }

        private void DeserialiseGroupData(XmlElement src, ShortcutGroup dst) {
            foreach (XmlElement child in src.ChildNodes.OfType<XmlElement>()) {
                switch (child.Name) {
                    case "Group": {
                        ShortcutGroup innerGroup = dst.CreateGroupByName(GetElementName(dst, child), GetIsGlobal(child), GetIsInherit(child));
                        innerGroup.Description = GetDescription(child);
                        innerGroup.DisplayName = GetDisplayName(child);
                        this.DeserialiseGroupData(child, innerGroup);
                        break;
                    }
                    case "InputState": {
                        string name = GetElementName(dst, child);
                        Dictionary<string, XmlElement> elements = new Dictionary<string, XmlElement>();
                        foreach (XmlElement element in child.ChildNodes.OfType<XmlElement>()) {
                            elements[element.Name] = element;
                        }

                        bool? canToggle = GetBool(child, "CanToggle");
                        IInputStroke activator, deactivator;
                        if (elements.TryGetValue("InputState.ActivationKeyStroke", out XmlElement activationKeyStroke)) {
                            KeyStroke activationStroke = this.DeserialiseKeyStroke(activationKeyStroke);
                            KeyStroke deativationStroke;
                            if (elements.TryGetValue("InputState.DeactivationKeyStroke", out XmlElement deativationKeyStroke)) {
                                deativationStroke = this.DeserialiseKeyStroke(deativationKeyStroke);
                            }
                            else if (canToggle == true) {
                                deativationStroke = activationStroke;
                            }
                            else if (activationStroke.IsRelease) {
                                deativationStroke = activationStroke;
                                activationStroke = new KeyStroke(activationStroke.KeyCode, activationStroke.Modifiers, false);
                            }
                            else {
                                deativationStroke = new KeyStroke(activationStroke.KeyCode, activationStroke.Modifiers, true);
                            }

                            activator = activationStroke;
                            deactivator = deativationStroke;
                        }
                        else if (elements.TryGetValue("InputState.ActivationMouseStroke", out XmlElement activationMouseStroke)) {
                            MouseStroke activationStroke = this.DeserialiseMouseStroke(activationMouseStroke);
                            MouseStroke deativationStroke;
                            if (elements.TryGetValue("InputState.DeactivationMouseStroke", out XmlElement deativationMouseStroke)) {
                                deativationStroke = this.DeserialiseMouseStroke(deativationMouseStroke);
                            }
                            else if (canToggle == true) {
                                deativationStroke = activationStroke;
                            }
                            else if (activationStroke.IsRelease) {
                                deativationStroke = activationStroke;
                                activationStroke = new MouseStroke(activationStroke.MouseButton, activationStroke.Modifiers, false, activationStroke.ClickCount);
                            }
                            else {
                                deativationStroke = new MouseStroke(activationStroke.MouseButton, activationStroke.Modifiers, true, activationStroke.ClickCount);
                            }

                            activator = activationStroke;
                            deactivator = deativationStroke;
                        }
                        else {
                            throw new Exception("Missing 'ActivationKeyStroke' or 'ActivationMouseStroke' for a key state");
                        }

                        string id;
                        GroupedInputState state = dst.AddInputState(name, activator, deactivator);
                        if (GetBool(child, "UseGroupAsManager") == true) {
                            dst.GetInputStateManager().Add(state);
                        }
                        else if ((id = GetAttributeNullable(child, "StateManager")) != null) {
                            dst.Manager.GetInputStateManager(id).Add(state);
                        }

                        break;
                    }
                    case "Shortcut": {
                        string name = GetElementName(dst, child);
                        List<IInputStroke> inputs = new List<IInputStroke>();
                        foreach (XmlElement innerElement in child.ChildNodes.OfType<XmlElement>()) {
                            // XML should have strict name cases, buuuut... why not be nice
                            switch (innerElement.Name) {
                                case "KeyStroke":
                                case "Keystroke":
                                case "keystroke":
                                    inputs.Add(this.DeserialiseKeyStroke(innerElement));
                                    break;
                                case "MouseStroke":
                                case "Mousestroke":
                                case "mousestroke":
                                    inputs.Add(this.DeserialiseMouseStroke(innerElement));
                                    break;
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

                        GroupedShortcut managed = dst.AddShortcut(name, shortcut, GetIsGlobal(child));
                        managed.IsInherited = GetIsInherit(child);
                        managed.RepeatMode = GetRepeatMode(child);
                        managed.ActionId = GetAttributeNullable(child, "ActionId");
                        managed.Description = GetDescription(child);
                        managed.DisplayName = GetDisplayName(child);
                        break;
                    }
                }
            }
        }

        #endregion

        #region Util functions

        protected static bool GetIsGlobal(XmlElement element) {
            // false by default
            string attrib = element.GetAttribute("IsGlobal");
            if (attrib.Length == 0)
                attrib = element.GetAttribute("Global");
            return !string.IsNullOrWhiteSpace(attrib) && attrib.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        protected static bool GetIsInherit(XmlElement element) {
            // true by default
            string attrib = element.GetAttribute("IsInherit");
            if (attrib.Length == 0)
                attrib = element.GetAttribute("Inherit");
            return string.IsNullOrWhiteSpace(attrib) || attrib.Equals("True", StringComparison.OrdinalIgnoreCase);
        }

        protected static RepeatMode GetRepeatMode(XmlElement element) {
            // true by default
            string attrib = element.GetAttribute("RepeatMode");
            if (string.IsNullOrWhiteSpace(attrib)) {
                return RepeatMode.Ignored;
            }
            else if (attrib.EqualsIgnoreCase("nonrepeat") || attrib.EqualsIgnoreCase("norepeat") || attrib.EqualsIgnoreCase("nonrepeated")) {
                return RepeatMode.NonRepeat;
            }
            else if (attrib.EqualsIgnoreCase("repeatonly") || attrib.EqualsIgnoreCase("repeat") || attrib.EqualsIgnoreCase("onlyrepeat")) {
                return RepeatMode.RepeatOnly;
            }
            else {
                return RepeatMode.NonRepeat;
            }
        }

        protected static string GetDescription(XmlElement element) => GetAttributeNullable(element, "Description");

        protected static string GetDisplayName(XmlElement element) => GetAttributeNullable(element, "DisplayName");

        protected static string GetAttributeNullable(XmlElement element, string key, bool whitespacesToNull = true) {
            string attribute = element.GetAttribute(key);
            if (whitespacesToNull ? string.IsNullOrWhiteSpace(attribute) : string.IsNullOrEmpty(attribute))
                return null;
            return attribute;
        }

        protected static string GetElementName(IGroupedObject owner, XmlElement child) => GetAttributeNonNull(owner, child, "Name");

        protected static string GetAttributeNonNull(IGroupedObject owner, XmlElement element, string key, bool requireNonWhitespaces = true) {
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

        protected static bool? GetBool(XmlElement element, string key) {
            string value = GetAttributeNullable(element, key);
            if (value == null) {
                return null;
            }

            return "true".EqualsIgnoreCase(value);
        }

        #endregion

        protected abstract KeyStroke DeserialiseKeyStroke(XmlElement element);
        protected abstract MouseStroke DeserialiseMouseStroke(XmlElement element);
        protected abstract void SerialiseKeystroke(XmlDocument doc, XmlElement elem, in KeyStroke stroke, string childElementName = "KeyStroke");
        protected abstract void SerialiseMousestroke(XmlDocument doc, XmlElement elem, in MouseStroke stroke, string childElementName = "MouseStroke");
    }
}