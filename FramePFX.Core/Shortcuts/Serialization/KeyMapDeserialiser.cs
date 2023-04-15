using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using FramePFX.Core.Shortcuts.Inputs;
using FramePFX.Core.Shortcuts.Managing;

namespace FramePFX.Core.Shortcuts.Serialization {
    public abstract class KeyMapDeserialiser {
        private static readonly XmlSerializer Serializer;

        static KeyMapDeserialiser() {
            Serializer = new XmlSerializer(typeof(KeyMap));
        }

        public KeyMapDeserialiser() {

        }

        protected abstract Keystroke SerialiseKeystroke(in KeyStroke stroke);

        protected abstract Mousestroke SerialiseMousestroke(in MouseStroke stroke);

        protected abstract KeyStroke DeserialiseKeystroke(Keystroke stroke);

        protected abstract MouseStroke DeserialiseMousestroke(Mousestroke stroke);

        protected virtual void SerialiseInputStrokeToShortcut(Shortcut shortcut, IInputStroke stroke) {
            if (stroke is MouseStroke secondMouseStroke) {
                shortcut.Strokes.Add(this.SerialiseMousestroke(secondMouseStroke));
            }
            else if (stroke is KeyStroke secondKeyStroke) {
                shortcut.Strokes.Add(this.SerialiseKeystroke(secondKeyStroke));
            }
            else {
                throw new Exception("Unknown input stroke type: " + stroke?.GetType());
            }
        }

        public virtual void Serialise(Stream output, ShortcutGroup group) {
            KeyMap map = new KeyMap();
            this.SerialiseGroup(map, group);
            Serializer.Serialize(new XmlTextWriter(output, null) {
                Formatting = Formatting.Indented,
                Indentation = 4,
                Settings = {
                    CloseOutput = false
                }
            }, map);
        }

        /// <summary>
        /// Deserializes the given input stream into the root <see cref="ShortcutGroup"/>
        /// </summary>
        /// <param name="input"></param>
        /// <param name="manager"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual ShortcutGroup Deserialise(Stream input) {
            KeyMap result = (KeyMap) Serializer.Deserialize(input);
            if (result == null) {
                throw new Exception("Failed to deserialize key map; null returned");
            }

            ShortcutGroup root = ShortcutGroup.CreateRoot();
            this.DeserialiseGroupData(result, root);
            return root;
        }

        protected virtual IEnumerable<IInputStroke> DeserialiseStrokes(List<object> strokes) {
            foreach (object stroke in strokes) {
                if (stroke is Keystroke ks) {
                    yield return this.DeserialiseKeystroke(ks);
                }
                else if (stroke is Mousestroke ms) {
                    yield return this.DeserialiseMousestroke(ms);
                }
                else {
                    throw new Exception("Unknown input stroke type: " + stroke?.GetType());
                }
            }
        }

        protected virtual void DeserialiseGroupData(Group keyGroup, ShortcutGroup realKeyGroup) {
            if (keyGroup.Shortcuts != null && keyGroup.Shortcuts.Count > 0) {
                foreach (Shortcut cut in keyGroup.Shortcuts) {
                    bool hasKey = false;
                    bool hasMouse = false;
                    if (cut.Strokes != null && cut.Strokes.Any(x => x is Keystroke)) {
                        hasKey = true;
                    }

                    if (cut.Strokes != null && cut.Strokes.Any(x => x is Mousestroke)) {
                        hasMouse = true;
                    }

                    IShortcut shortcut;
                    if (hasKey && hasMouse) {
                        shortcut = new MouseKeyboardShortcut(this.DeserialiseStrokes(cut.Strokes));
                    }
                    else if (hasKey) {
                        List<KeyStroke> strokes = cut.Strokes.OfType<Keystroke>().Select(this.DeserialiseKeystroke).ToList();
                        shortcut = new KeyboardShortcut(strokes);
                    }
                    else if (hasMouse) {
                        List<MouseStroke> strokes = cut.Strokes.OfType<Mousestroke>().Select(this.DeserialiseMousestroke).ToList();
                        shortcut = new MouseShortcut(strokes);
                    }
                    else {
                        continue;
                    }

                    GroupedShortcut managed = realKeyGroup.AddShortcut(cut.Name, shortcut, cut.IsGlobalBool);
                    managed.ActionId = cut.ActionId;
                    managed.Description = cut.Description;
                    managed.DisplayName = cut.DisplayName;
                }
            }

            if (keyGroup.InnerGroups != null && keyGroup.InnerGroups.Count > 0) {
                foreach (Group innerGroup in keyGroup.InnerGroups) {
                    ShortcutGroup realInnerGroup = realKeyGroup.CreateGroupByName(innerGroup.Name, innerGroup.IsGlobalBool, innerGroup.InheritBool);
                    realInnerGroup.Description = innerGroup.Description;
                    realInnerGroup.DisplayName = innerGroup.DisplayName;
                    this.DeserialiseGroupData(innerGroup, realInnerGroup);
                }
            }
        }

        protected virtual void SerialiseGroup(Group group, ShortcutGroup focusGroup) {
            group.Name = focusGroup.Name;
            group.DisplayName = focusGroup.DisplayName;
            group.Description = string.IsNullOrWhiteSpace(focusGroup.Description) ? null : focusGroup.Description;
            group.IsGlobal = SerialiseObject(focusGroup.IsGlobal, false);
            group.InheritFromParent = SerialiseObject(focusGroup.InheritFromParent, false);
            group.InnerGroups = new List<Group>();
            group.Shortcuts = new List<Shortcut>();
            foreach (GroupedShortcut shortcut in focusGroup.Shortcuts) {
                if (shortcut.Shortcut.IsEmpty) {
                    continue;
                }

                Shortcut cut = new Shortcut {
                    Name = shortcut.Name,
                    Description = shortcut.Description,
                    ActionId = shortcut.ActionId,
                    IsGlobal = SerialiseObject(shortcut.IsGlobal, false),
                    Strokes = new List<object>()
                };

                foreach (IInputStroke stroke in shortcut.Shortcut.InputStrokes) {
                    this.SerialiseInputStrokeToShortcut(cut, stroke);
                }

                group.Shortcuts.Add(cut);
            }

            foreach (ShortcutGroup innerGroup in focusGroup.Groups) {
                Group inner = new Group();
                group.InnerGroups.Add(inner);
                this.SerialiseGroup(inner, innerGroup);
            }
        }

        private static string SerialiseObject(bool value, bool def) {
            return value == def ? null : (value ? "true" : "false");
        }
    }
}