using FramePFX.Editor.Timelines.Events;
using FramePFX.Editor.Timelines.ResourceHelpers;
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
        /// An event fired when this clip's <see cref="FrameSpan"/> changes
        /// </summary>
        event ClipSpanChangedEventHandler ClipSpanChanged;
    }
}