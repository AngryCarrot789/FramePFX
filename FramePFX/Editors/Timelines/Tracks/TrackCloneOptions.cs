namespace FramePFX.Editors.Timelines.Tracks {
    public struct TrackCloneOptions {
        public static readonly TrackCloneOptions Default = new TrackCloneOptions() {
            ClipCloneOptions = ClipCloneOptions.Default,
            CloneClips = true
        };

        public bool CloneClips;
        public ClipCloneOptions ClipCloneOptions;

        public TrackCloneOptions(bool cloneClips, ClipCloneOptions clipCloneOptions) {
            this.CloneClips = cloneClips;
            this.ClipCloneOptions = clipCloneOptions;
        }
    }
}