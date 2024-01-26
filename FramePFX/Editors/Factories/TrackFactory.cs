using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Factories {
    public class TrackFactory : ReflectiveObjectFactory<Track> {
        public static TrackFactory Instance { get; } = new TrackFactory();

        private TrackFactory() {
            this.RegisterType("track_vid", typeof(VideoTrack));
        }

        public Track NewTrack(string id) {
            return base.NewInstance(id);
        }
    }
}