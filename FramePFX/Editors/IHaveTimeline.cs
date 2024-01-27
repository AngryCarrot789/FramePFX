using FramePFX.Editors.Timelines;
using OpenTK.Graphics.ES11;

namespace FramePFX.Editors {
    public delegate void TimelineChangedEventHandler(IHaveTimeline sender, Timeline oldTimeline, Timeline newTimeline);

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

        /// <summary>
        /// An event fired when this object's effective timeline changed. This may be called either when this effect
        /// is added/removed from a clip, this clip is added/removed from a track, when this clip's track is added/removed
        /// from a timeline or when this track is added/removed from a timeline
        /// </summary>
        event TimelineChangedEventHandler TimelineChanged;
    }
}