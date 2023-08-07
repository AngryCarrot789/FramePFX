namespace FramePFX.Core.Editor.Timelines.AudioClips
{
    public class AudioClip : Clip
    {
        public float Volume { get; set; }

        public bool IsMuted { get; set; }

        protected override Clip NewInstance()
        {
            return new AudioClip();
        }

        protected override void LoadDataIntoClone(Clip clone)
        {
            base.LoadDataIntoClone(clone);
            AudioClip clip = (AudioClip) clone;
            clip.Volume = this.Volume;
            clip.IsMuted = this.IsMuted;
        }
    }
}