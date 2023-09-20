using System;
using FFmpeg.AutoGen;
using FramePFX.Actions.Contexts;
using FramePFX.Editor.ViewModels;
using FramePFX.Editor.ViewModels.Timelines;

namespace FramePFX.Editor.Actions {
    public static class EditorActionUtils {
        public static TimelineViewModel FindTimeline(IDataContext context) {
            if (context.TryGetContext(out ClipViewModel clip)) {
                return clip.Track?.Timeline;
            }
            else if (context.TryGetContext(out TrackViewModel track)) {
                return track.Timeline;
            }
            else if (context.TryGetContext(out TimelineViewModel timeline)) {
                return timeline;
            }
            else if (context.TryGetContext(out ProjectViewModel project)) {
                return project.Timeline;
            }
            else if (context.TryGetContext(out VideoEditorViewModel editor)) {
                return editor.ActiveProject?.Timeline;
            }
            else {
                return null;
            }
        }

        public static bool GetNewTrackData(IDataContext context, out TimelineViewModel timeline, out int index, AVType type) {
            timeline = default;
            index = -1;

            int selectedTrackIndex;
            TrackViewModel track;
            if (context.TryGetContext(out timeline)) {
                selectedTrackIndex = (track = timeline.PrimarySelectedTrack) != null ? timeline.Tracks.IndexOf(track) : -1;
            }
            else if (context.TryGetContext(out track)) {
                if ((timeline = track.Timeline) == null)
                    return false;
                selectedTrackIndex = timeline.Tracks.IndexOf(track);
            }
            else if (context.TryGetContext(out ClipViewModel clip)) {
                if ((track = clip.Track) == null)
                    return false;
                if ((timeline = track.Timeline) == null)
                    return false;
                selectedTrackIndex = timeline.Tracks.IndexOf(track);
            }
            else {
                return false;
            }

            if (selectedTrackIndex == -1) {
                switch (type) {
                    case AVType.Video: index = 0; break;
                    case AVType.Audio: index = timeline.Tracks.Count; break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }
            else {
                switch (type) {
                    case AVType.Video: index = selectedTrackIndex; break;
                    case AVType.Audio: index = Math.Min(selectedTrackIndex + 1, timeline.Tracks.Count); break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), type, null);
                }
            }

            return true;
        }
    }
}