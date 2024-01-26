using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FramePFX.Editors.Media_OLD {
    /// <summary>
    /// A class used to 'fetch' a media frame from some sort of unknown source.
    /// This is used by things like clips in order to generate an output
    /// <para>
    /// An example is a MediaDecoderSource
    /// </para>
    /// </summary>
    public abstract class MediaSource {
        private readonly List<MediaSourceTrack> tracks;

        public ReadOnlyCollection<MediaSourceTrack> Tracks { get; }

        protected MediaSource() {
            this.tracks = new List<MediaSourceTrack>();
            this.Tracks = new ReadOnlyCollection<MediaSourceTrack>(this.tracks);
        }

        protected void AddTrack(MediaSourceTrack track) {
            this.InsertTrack(this.tracks.Count, track);
        }

        protected void InsertTrack(int index, MediaSourceTrack track) {
            this.tracks.Insert(index, track);
        }

        protected bool RemoveTrack(MediaSourceTrack track) {
            return this.tracks.Remove(track);
        }

        protected void RemoveTrackAt(int index) {
            this.tracks.RemoveAt(index);
        }
    }
}