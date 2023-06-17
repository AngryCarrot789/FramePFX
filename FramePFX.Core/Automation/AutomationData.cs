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

        public AutomationData(IAutomatable owner) {
            this.Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.dataMap = new Dictionary<AutomationKey, AutomationSequence>();
        }

        /// <summary>
        /// Adds an automation sequence for the given key, allowing it to be automated
        /// </summary>
        /// <param name="key">The key to add</param>
        public void AssignKey(AutomationKey key) {
            if (this.dataMap.ContainsKey(key))
                throw new Exception("Key is already assigned");
            this.dataMap[key] = new AutomationSequence(key);
        }

        public bool TryGetData(AutomationKey key, out AutomationSequence value) => this.dataMap.TryGetValue(key, out value);

        public AutomationSequence GetData(AutomationKey key) => this.dataMap[key];

        public void WriteToRBE(RBEDictionary data) {

        }

        public void ReadFromRBE(RBEDictionary data) {
            throw new NotImplementedException();
        }
    }
}