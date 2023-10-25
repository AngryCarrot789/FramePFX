using System;
using System.Numerics;
using FramePFX.Editor.Timelines.Effects;
using FramePFX.Editor.Timelines.Effects.Video;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class AdjustmentVideoClip : VideoClip {
        public AdjustmentVideoClip() {
        }

        public override Vector2? GetFrameSize() => (Vector2?) this.Project.Settings.Resolution;

        protected override Clip NewInstanceForClone() {
            return new AdjustmentVideoClip();
        }

        public override bool IsEffectTypeAllowed(BaseEffect effect) {
            return !(effect is ITransformationEffect) && base.IsEffectTypeAllowed(effect);
        }

        public override bool IsEffectTypeAllowed(Type effectType) {
            return !effectType.instanceof(typeof(ITransformationEffect)) && base.IsEffectTypeAllowed(effectType);
        }
    }
}