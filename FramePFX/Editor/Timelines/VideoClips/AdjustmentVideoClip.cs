using System.Numerics;

namespace FramePFX.Editor.Timelines.VideoClips {
    public class AdjustmentVideoClip : VideoClip {
        public AdjustmentVideoClip() {
        }

        public override Vector2? GetSize() => (Vector2?) this.Project.Settings.Resolution;

        protected override Clip NewInstanceForClone() {
            return new AdjustmentVideoClip();
        }
    }
}