using FramePFX.Editors.Timelines.Clips;
using FramePFX.Editors.Timelines.Tracks;

namespace FramePFX.Editors.Rendering {
    /// <summary>
    /// A class that manages the render state of a track
    /// </summary>
    public class TrackRenderState {
        /// <summary>
        /// The primary clip to render
        /// </summary>
        public VideoClip ClipA { get; private set; }

        /// <summary>
        /// A secondary clip that is used to create a render transition between A and B
        /// </summary>
        public VideoClip ClipB { get; private set; }

        /// <summary>
        /// The track that owns this render state data
        /// </summary>
        public VideoTrack Track { get; }

        public TrackRenderState(VideoTrack track) {
            this.Track = track;
        }

        public void Prepare(long frame) {

        }
    }
}