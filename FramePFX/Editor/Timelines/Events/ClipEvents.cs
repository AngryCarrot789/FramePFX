using System;
using FramePFX.Utils;

namespace FramePFX.Editor.Timelines.Events {
    public delegate void ClipRenderInvalidatedEventHandler(Clip clip);
    public delegate void ClipSpanChangedEventHandler(Clip clip, FrameSpan oldSpan, FrameSpan newSpan);

    public delegate void ClipAddedEventHandler(Track track, Clip clip, int index);
    public delegate void ClipRemovedEventHandler(Track track, Clip clip, int index);
    public delegate void ClipMovedEventHandler(ClipMovedEventArgs e);

    /// <summary>
    /// Event args for when a clip is moved from one track to another
    /// </summary>
    public class ClipMovedEventArgs : EventArgs {
        /// <summary>
        /// The source/original track
        /// </summary>
        public Track OldTrack { get; }

        /// <summary>
        /// The target/destination folder
        /// </summary>
        public Track NewTrack { get; }

        /// <summary>
        /// The clip that was moved
        /// </summary>
        public Clip Clip { get; }

        /// <summary>
        /// The old index that <see cref="Clip"/> was located at when it existed in <see cref="OldTrack"/>
        /// </summary>
        public int OldIndex { get; }

        /// <summary>
        /// The index of <see cref="Clip"/> in <see cref="NewTrack"/>
        /// </summary>
        public int NewIndex { get; }

        /// <summary>
        /// An additional object that can be used to pass information between handlers
        /// </summary>
        public object Parameter { get; set; }

        public ClipMovedEventArgs(Track oldTrack, Track newTrack, Clip clip, int oldIndex, int newIndex) {
            this.OldTrack = oldTrack;
            this.NewTrack = newTrack;
            this.Clip = clip;
            this.OldIndex = oldIndex;
            this.NewIndex = newIndex;
        }
    }
}