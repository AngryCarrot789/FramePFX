using System;
using System.Collections.ObjectModel;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class MoveTrackCommand : Command {
        public int Offset { get; }

        public MoveTrackCommand(int offset) {
            this.Offset = offset;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.TrackKey.TryGetContext(e.DataContext, out Track track)) {
                if (DataKeys.TimelineKey.TryGetContext(e.DataContext, out Timeline timeline)) {
                    if (timeline.SelectedTracks.Count < 1)
                        return;
                    track = timeline.SelectedTracks[timeline.SelectedTracks.Count - 1];
                }
                else {
                    return;
                }
            }

            MoveTrack(track, this.Offset);
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

    public class MoveTrackUpCommand : MoveTrackCommand {
        public MoveTrackUpCommand() : base(-1) {}
    }

    public class MoveTrackDownCommand : MoveTrackCommand {
        public MoveTrackDownCommand() : base(1) {}
    }

    public class MoveTrackToTopCommand : MoveTrackCommand {
        public MoveTrackToTopCommand() : base(int.MinValue) {}
    }

    public class MoveTrackToBottomCommand : MoveTrackCommand {
        public MoveTrackToBottomCommand() : base(int.MaxValue) {}
    }
}