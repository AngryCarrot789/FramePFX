using FramePFX.Core.Automation.Keys;

namespace FramePFX.Core.Automation.Keyframe {
    /// <summary>
    /// Contains all of the key frames for a specific <see cref="AutomationKey"/>
    /// </summary>
    public class KeyFrameTimeline {
        public AutomationKey Key { get; }

        

        public KeyFrameTimeline(AutomationKey key) {
            this.Key = key;
        }
    }
}