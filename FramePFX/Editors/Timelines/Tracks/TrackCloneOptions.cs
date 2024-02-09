namespace FramePFX.Editors.Timelines.Tracks {
    public struct TrackCloneOptions {
        public static readonly TrackCloneOptions Default = new TrackCloneOptions() {
            ClipCloneOptions = Tracks.ClipCloneOptions.Default,
        };

        public ClipCloneOptions? ClipCloneOptions;

        public TrackCloneOptions(ClipCloneOptions? clipCloneOptions) {
            this.ClipCloneOptions = clipCloneOptions;
        }
    }
}