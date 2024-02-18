using FramePFX.Editors.Timelines;

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
        /// Tries to get the play head relative to this object. If <see cref="Timeline"/> is null or the play head
        /// is otherwise inaccessible or the play head is not in range then false is returned
        /// </summary>
        /// <param name="playHead">The relative play head</param>
        /// <returns>True if in range</returns>
        bool GetRelativePlayHead(out long playHead);

        /// <summary>
        /// An event fired when this object's effective timeline changed. This may be called when:
        /// <br/>
        /// - We are an effect and we get added to or removed from a clip
        /// <br/>
        /// - We are a clip and we are get added to or removed from a track,
        ///   or our owner track is added to or removed from a timeline
        /// <br/>
        /// - We are a track and we are added to or removed from a timeline
        /// </summary>
        event TimelineChangedEventHandler TimelineChanged;
    }
}