using FramePFX.Editor.Timelines.Effects;

namespace FramePFX.Editor.Timelines.AudioClips {
    public class AudioClip : Clip {
        public float Volume { get; set; }

        public bool IsMuted { get; set; }

        protected override Clip NewInstanceForClone() {
            return new AudioClip();
        }

        protected override void LoadUserDataIntoClone(Clip clone, ClipCloneFlags flags) {
            base.LoadUserDataIntoClone(clone, flags);
            AudioClip clip = (AudioClip) clone;
            clip.Volume = this.Volume;
            clip.IsMuted = this.IsMuted;
        }

        public override bool IsEffectTypeAllowed(BaseEffect effect) {
            return false; // no audio effects yet :(
        }
    }
}