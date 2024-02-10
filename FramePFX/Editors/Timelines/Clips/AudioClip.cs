using System;

namespace FramePFX.Editors.Timelines.Clips {
    public class AudioClip : Clip {
        public override bool IsEffectTypeAccepted(Type effectType) {
            return false;
        }
    }
}