namespace FramePFX.Core.Editor.Timeline.AudioClips {
    public class AudioClipModel : ClipModel {
        public float Volume { get; set; }

        public bool IsMuted { get; set; }

        protected override ClipModel NewInstance() {
            return new AudioClipModel();
        }

        protected override void LoadDataIntoClone(ClipModel clone) {
            base.LoadDataIntoClone(clone);
            AudioClipModel clip = (AudioClipModel) clone;
            clip.Volume = this.Volume;
            clip.IsMuted = this.IsMuted;
        }


    }
}