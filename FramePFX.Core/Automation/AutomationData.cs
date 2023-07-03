using System;
using System.Collections.Generic;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.RBC;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Automation {
    /// <summary>
    /// Contains automation data, which is a collection of <see cref="AutomationSequence"/>s mapped by an <see cref="AutomationKey"/>
    /// </summary>
    public class AutomationData : IRBESerialisable {
        private readonly Dictionary<AutomationKey, AutomationSequence> map;
        private readonly List<AutomationSequence> sequences;

        public IEnumerable<AutomationSequence> Sequences => this.sequences;

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
                if (this.map.TryGetValue(key ?? throw new ArgumentNullException(nameof(key), "Key cannot be null"), out AutomationSequence sequence))
                    return sequence;

                #if DEBUG
                System.Diagnostics.Debugger.Break();
                #endif

                throw new Exception($"Key has not been assigned: {key}");
            }
        }

        /// <summary>
        /// The active key's full ID, only really used by the front-end/view models
        /// </summary>
        public string ActiveKeyFullId { get; set; }

        public AutomationData(IAutomatable owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.map = new Dictionary<AutomationKey, AutomationSequence>();
            this.sequences = new List<AutomationSequence>();
        }

        /// <summary>
        /// Adds an automation sequence for the given key, allowing it to be automated
        /// </summary>
        /// <param name="key">The key to add</param>
        public AutomationSequence AssignKey(AutomationKey key, UpdateAutomationValueEventHandler updateValueHandler) {
            if (this.map.ContainsKey(key))
                throw new Exception("Key is already assigned");
            AutomationSequence sequence = new AutomationSequence(this, key);
            this.map[key] = sequence;
            this.sequences.Add(sequence);
            if (updateValueHandler != null) {
                sequence.UpdateValue += updateValueHandler;
            }

            return sequence;
        }

        public bool TryGetData(AutomationKey key, out AutomationSequence value) => this.map.TryGetValue(key, out value);

        public AutomationSequence GetData(AutomationKey key) => this.map[key];

        public void WriteToRBE(RBEDictionary data) {
            if (this.ActiveKeyFullId != null)
                data.SetString(nameof(this.ActiveKeyFullId), this.ActiveKeyFullId);
            RBEList list = data.CreateList(nameof(this.Sequences));
            foreach (AutomationSequence sequence in this.sequences) {
                RBEDictionary dictionary = list.AddDictionary();
                dictionary.SetString("KeyId", sequence.Key.FullId);
                sequence.WriteToRBE(dictionary);
            }
        }

        public void ReadFromRBE(RBEDictionary data) {
            this.ActiveKeyFullId = data.GetString(nameof(this.ActiveKeyFullId), null);
            RBEList list = data.GetList(nameof(this.Sequences));
            foreach (RBEBase rbe in list.List) {
                if (!(rbe is RBEDictionary dictionary))
                    throw new Exception("Expected a list of dictionaries");
                string fullId = dictionary.GetString("KeyId");
                if (!fullId.Split(AutomationKey.FullIdSplitter, out string domain, out string id))
                    throw new Exception($"Invalid KeyId: {fullId}");

                AutomationKey key = AutomationKey.GetKey(domain, id);
                if (key == null)
                    throw new Exception("Unknown automation key: " + fullId);
                if (!this.map.TryGetValue(key, out AutomationSequence sequence))
                    throw new Exception("Missing/unassigned key: " + fullId);
                sequence.ReadFromRBE(dictionary);
            }
        }

        public void LoadDataIntoClone(AutomationData clone) {
            clone.ActiveKeyFullId = this.ActiveKeyFullId;
            for (int i = 0, c = this.sequences.Count; i < c; i++) {
                AutomationSequence.LoadDataIntoClone(this.sequences[i], clone.sequences[i]);
            }
        }
    }
}