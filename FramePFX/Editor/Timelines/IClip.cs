using FramePFX.Editor.Timelines.Events;
using FramePFX.RBC.Events;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// An interface for clip objects, including the base <see cref="Clip"/> class
    /// </summary>
    public interface IClip {
        /// <summary>
        /// Gets the project associated with this clip. May be null if the clip is disconnected
        /// </summary>
        Project Project { get; }

        /// <summary>
        /// An event fired when this clip is being removed from a track (where new track is null), being
        /// added to a track (where the previous track is null), or moved between tracks (where neither are null)
        /// </summary>
        event TrackChangedEventHandler TrackChanged;

        /// <summary>
        /// An event fired when the track (that holds us) timeline changes (as in, a track was
        /// added to, removed from or moved between timelines)
        /// </summary>
        event TimelineChangedEventHandler TrackTimelineChanged;

        /// <summary>
        /// An event fired when the user seeks a specific frame on the timeline. This is not fired during playback
        /// </summary>
        event FrameSeekedEventHandler FrameSeeked;

        /// <summary>
        /// An event fired when this clip is being serialised
        /// </summary>
        event WriteToRBEEventHandler SerialiseExtension;

        /// <summary>
        /// An event fired when this clip is being deserialised
        /// </summary>
        event ReadFromRBEEventHandler DeserialiseExtension;
    }
}