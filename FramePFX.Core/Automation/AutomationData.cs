using System;
using System.Collections.Generic;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;

namespace FramePFX.Core.Automation {
    /// <summary>
    /// Contains automation data, which is a collection of <see cref="AutomationSequence"/>s mapped by an <see cref="AutomationKey"/>
    /// </summary>
    public class AutomationData : IRBESerialisable {
        private readonly Dictionary<AutomationKey, AutomationSequence> dataMap;

        public IEnumerable<AutomationSequence> Sequences => this.dataMap.Values;

        /// <summary>
        /// The automatable instance that owns this instance
        /// </summary>
        public IAutomatable Owner { get; }

        /// <summary>
        /// Gets the automation sequence (aka timeline) for the specific automation key
        /// </summary>
        /// <param name="key"></param>
        public AutomationSequence this[AutomationKey key] {
            get {
                if (this.dataMap.TryGetValue(key ?? throw new ArgumentNullException(nameof(key), "Key cannot be null"), out AutomationSequence sequence))
                    return sequence;
                throw new Exception($"Key has not been assigned: {key}");
            }
        }

        public AutomationData(IAutomatable owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.dataMap = new Dictionary<AutomationKey, AutomationSequence>();
        }

        /// <summary>
        /// Adds an automation sequence for the given key, allowing it to be automated
        /// </summary>
        /// <param name="key">The key to add</param>
        public AutomationSequence AssignKey(AutomationKey key, UpdateAutomationValueEventHandler updateValueHandler) {
            if (this.dataMap.ContainsKey(key))
                throw new Exception("Key is already assigned");
            AutomationSequence sequence = new AutomationSequence(key);
            this.dataMap[key] = sequence;
            if (updateValueHandler != null) {
                sequence.UpdateValue += updateValueHandler;
            }

            return sequence;
        }

        public bool TryGetData(AutomationKey key, out AutomationSequence value) => this.dataMap.TryGetValue(key, out value);

        public AutomationSequence GetData(AutomationKey key) => this.dataMap[key];

        public void WriteToRBE(RBEDictionary data) {
            RBEList list = data.CreateList(nameof(this.Sequences));
            foreach (AutomationSequence sequence in this.dataMap.Values) {
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString("Key::Domain", sequence.Key.Domain);
                dictionary.SetString("Key::Id", sequence.Key.Id);
                sequence.WriteToRBE(dictionary);
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            RBEList list = data.GetList(nameof(this.Sequences));
            foreach (RBEBase rbe in list.List) {
                if (!(rbe is RBEDictionary dictionary))
                    throw new Exception("Expected a list of dictionaries");
                string domain = dictionary.GetString("Key::Domain");
                string id = dictionary.GetString("Key::Id");
                AutomationKey key = AutomationKey.GetKey(domain, id);
                if (key == null)
                    throw new Exception("Unknown automation key: " + key.FullId);
                if (!this.dataMap.TryGetValue(key, out AutomationSequence sequence))
                    throw new Exception("Missing/unassigned key: " + key.FullId);
                sequence.ReadFromRBE(dictionary);
            }
        }

        public void LoadDataIntoClone(AutomationData clone) {
            foreach (AutomationSequence sequence in this.dataMap.Values)
                clone.dataMap[sequence.Key] = sequence.Clone();
        }
    }
}