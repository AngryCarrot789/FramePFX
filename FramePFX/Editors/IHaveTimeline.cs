using FramePFX.Editors.Timelines;

namespace FramePFX.Editors {
    /// <summary>
    /// An interface for an object that exists in a timeline, somewhere. This could be a track, clip or effect
    /// </summary>
    public interface IHaveTimeline : IHaveProject {
        /// <summary>
        /// Gets the timeline associated with this object. May return null
        /// </summary>
        Timeline Timeline { get; }

        /// <summary>
        /// Gets the playhead that is relative to this object. Return 0 for a null timeline
        /// <para>
        /// Tracks just return the play head. Clips return 'playhead - FrameBegin'. Track effects
        /// return the track's play head and clip effects return the clip's relative play head
        /// </para>
        /// </summary>
        long RelativePlayHead { get; }
    }
}