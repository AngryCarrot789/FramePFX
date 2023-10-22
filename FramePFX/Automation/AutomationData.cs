using System;
using System.Collections.Generic;
using FramePFX.Automation.Events;
using FramePFX.Automation.Keyframe;
using FramePFX.Automation.Keys;
using FramePFX.RBC;
using FramePFX.Utils;

namespace FramePFX.Automation {
    /// <summary>
    /// Contains a collection of <see cref="AutomationSequence"/> objects mapped by an <see cref="AutomationKey"/>. This
    /// class is designed to be immutable; it is typically created in the constructor of an <see cref="IAutomatable"/>
    /// object, where sequences are assigned via <see cref="AssignKey"/>
    /// </summary>
    public class AutomationData {
        private readonly Dictionary<AutomationKey, AutomationSequence> map;
        private readonly List<AutomationSequence> sequences;

        /// <summary>
        /// An ordered enumerable collection of sequences
        /// </summary>
        public IReadOnlyList<AutomationSequence> Sequences => this.sequences;

        /// <summary>
        /// The object that owns this automation data instance
        /// </summary>
        public IAutomatable Owner { get; }

        /// <summary>
        /// Gets the automation sequence (aka timeline) for the specific automation key
        /// </summary>
        /// <param name="key"></param>
        public AutomationSequence this[AutomationKey key] {
            get {
                if (this.map.TryGetValue(key ?? throw new ArgumentNullException(nameof(key), "Key cannot be null"), out AutomationSequence sequence)) {
                    return sequence;
                }

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

        public AutomationSequence AssignKey(AutomationKey key) {
            return this.AssignKey(key, this.Owner.CreateAssignment(key));
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
                sequence.DoUpdateValue(-1);
            }

            return sequence;
        }

        public bool TryGetData(AutomationKey key, out AutomationSequence value) => this.map.TryGetValue(key, out value);

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

                if (!AutomationKey.TryGetKey(domain, id, out AutomationKey key))
                    throw new Exception("Unknown automation key: " + fullId);
                if (!this.map.TryGetValue(key, out AutomationSequence sequence))
                    throw new Exception("Missing/unassigned automation key: " + fullId);
                sequence.ReadFromRBE(dictionary);
            }

            this.UpdateBackingStorage();
        }

        public void LoadDataIntoClone(AutomationData clone) {
            clone.ActiveKeyFullId = this.ActiveKeyFullId;
            for (int i = 0, c = this.sequences.Count; i < c; i++) {
                AutomationSequence.LoadDataIntoClone(this.sequences[i], clone.sequences[i]);
            }
        }

        /// <summary>
        /// Fires the <see cref="AutomationSequence.UpdateValue"/> event for each sequence stored in this data, and
        /// updates it with a frame of -1, indicating that the <see cref="AutomationSequence.DefaultKeyFrame"/> should
        /// be used to query the value instead of any actual key frame. Useful just after reading the state of an automation owner's data
        /// </summary>
        public void UpdateBackingStorage() {
            foreach (AutomationSequence sequence in this.sequences) {
                sequence.DoUpdateValue(-1);
            }
        }
    }
}