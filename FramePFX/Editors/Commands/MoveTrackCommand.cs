//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;

namespace FramePFX.Editors.Commands
{
    public class MoveTrackCommand : Command
    {
        public int Offset { get; }

        public MoveTrackCommand(int offset)
        {
            this.Offset = offset;
        }

        public override Task Execute(CommandEventArgs e)
        {
            if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track))
            {
                if (DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline timeline))
                {
                    if (timeline.SelectedTracks.Count < 1)
                        return Task.CompletedTask;
                    track = timeline.SelectedTracks[timeline.SelectedTracks.Count - 1];
                }
                else
                {
                    return Task.CompletedTask;
                }
            }

            MoveTrack(track, this.Offset);
            return Task.CompletedTask;
        }

        public static void MoveTrack(Track track, int offset)
        {
            if (track.Timeline == null)
                return;

            ReadOnlyCollection<Track> list = track.Timeline.Tracks;
            int oldIndex = list.IndexOf(track);
            if (oldIndex == -1)
                throw new Exception("???????");

            int newIndex = (int) Maths.Clamp((long) oldIndex + offset, 0, list.Count - 1);
            if (newIndex != oldIndex)
            {
                track.Timeline.MoveTrackIndex(oldIndex, newIndex);
            }
        }
    }

    public class MoveTrackUpCommand : MoveTrackCommand
    {
        public MoveTrackUpCommand() : base(-1) { }
    }

    public class MoveTrackDownCommand : MoveTrackCommand
    {
        public MoveTrackDownCommand() : base(1) { }
    }

    public class MoveTrackToTopCommand : MoveTrackCommand
    {
        public MoveTrackToTopCommand() : base(int.MinValue) { }
    }

    public class MoveTrackToBottomCommand : MoveTrackCommand
    {
        public MoveTrackToBottomCommand() : base(int.MaxValue) { }
    }
}