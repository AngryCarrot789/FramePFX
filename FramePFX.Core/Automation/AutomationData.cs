using System.Collections.Generic;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;

namespace FramePFX.Core.Automation {
    /// <summary>
    /// Contains data about a target's automation
    /// </summary>
    public class KeyFrameSequenceCollection {
        private readonly Dictionary<AutomationKey, AutomationSequence> dataMap;

        public KeyFrameSequenceCollection() {
            this.dataMap = new Dictionary<AutomationKey, AutomationSequence>();
        }

        public AutomationSequence GetData(AutomationKey key) {
            if (!this.dataMap.TryGetValue(key, out AutomationSequence data))
                this.dataMap[key] = data = new AutomationSequence(key);
            return data;
        }
    }
}