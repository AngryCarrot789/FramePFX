using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.ResourceHelpers;
using FramePFX.RBC.Events;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines {
    /// <summary>
    /// An interface for clip objects, including the base <see cref="Clip"/> class
    /// </summary>
    public interface IClip : IResourceHolder, IProjectBound {
        /// <summary>
        /// Gets the track that this clip is currently placed in
        /// </summary>
        Track Track { get; }

        /// <summary>
        /// The position of this clip in terms of video frames, in the form of a
        /// <see cref="Utils.FrameSpan"/> which has a begin and duration property
        /// </summary>
        FrameSpan FrameSpan { get; }

        /// <summary>
        /// An event fired when this clip is being removed from a track (where new track is null), being
        /// added to a track (where the previous track is null), or moved between tracks (where neither are null)
        /// </summary>
        event TrackChangedEventHandler TrackChanged;

        /// <summary>
        /// An event fired when the track (that holds us) timeline changes (as in, a track was added to,
        /// removed from or moved between timelines). Typically, this is only called when when a track is
        /// created and added to the timeline, or deleted/removed from the timeline.
        /// <para>
        /// However, a track could be moved from the project timeline to a composition
        /// timeline, in which case, the old and new tracks will be non-null
        /// </para>
        /// </summary>
        event TimelineChangedEventHandler TrackTimelineChanged;

        /// <summary>
        /// An event fired when the out track's timeline's project changes
        /// </summary>
        event ProjectChangedEventHandler TrackTimelineProjectChanged;

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

        /// <summary>
        /// An event fired when this clip's <see cref="FrameSpan"/> changes
        /// </summary>
        event ClipSpanChangedEventHandler ClipSpanChanged;
    }
}