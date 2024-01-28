using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FramePFX.Actions;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class MoveTrackAction : AnAction {
        public int Offset { get; }

        public MoveTrackAction(int offset) {
            this.Offset = offset;
        }

        public override Task ExecuteAsync(AnActionEventArgs e) {
            if (!e.DataContext.TryGetContext(DataKeys.TrackKey, out Track track)) {
                if (e.DataContext.TryGetContext(DataKeys.TimelineKey, out Timeline timeline)) {
                    if (timeline.SelectedTracks.Count < 1)
                        return Task.CompletedTask;
                    track = timeline.SelectedTracks[timeline.SelectedTracks.Count - 1];
                }
                else {
                    return Task.CompletedTask;
                }
            }

            MoveTrack(track, this.Offset);
            return Task.CompletedTask;
        }

        public static void MoveTrack(Track track, int offset) {
            if (track.Timeline == null)
                return;

            ReadOnlyCollection<Track> list = track.Timeline.Tracks;
            int oldIndex = list.IndexOf(track);
            if (oldIndex == -1)
                throw new Exception("???????");

            int newIndex = (int) Maths.Clamp((long) oldIndex + offset, 0, list.Count - 1);
            if (newIndex != oldIndex) {
                track.Timeline.MoveTrackIndex(oldIndex, newIndex);
            }
        }
    }

    public class MoveTrackUpAction : MoveTrackAction {
        public MoveTrackUpAction() : base(-1) {}
    }

    public class MoveTrackDownAction : MoveTrackAction {
        public MoveTrackDownAction() : base(1) {}
    }

    public class MoveTrackToTopAction : MoveTrackAction {
        public MoveTrackToTopAction() : base(int.MinValue) {}
    }

    public class MoveTrackToBottomAction : MoveTrackAction {
        public MoveTrackToBottomAction() : base(int.MaxValue) {}
    }
}