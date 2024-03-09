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

using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.Commands
{
    public class DeleteSelectedTracksCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            return e.ContextData.ContainsKey(DataKeys.TrackKey) || e.ContextData.ContainsKey(DataKeys.TimelineKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e)
        {
            int focusedIndex = -1;
            HashSet<Track> tracks = new HashSet<Track>();
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track focusedTrack))
            {
                focusedIndex = focusedTrack.IndexInTimeline;
            }

            Timeline timeline;
            if ((timeline = focusedTrack?.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.ContextData, out timeline))
            {
                foreach (Track track in timeline.SelectedTracks)
                {
                    tracks.Add(track);
                }
            }

            foreach (Track track in tracks)
            {
                track.Timeline.DeleteTrack(track);
            }

            focusedTrack?.Timeline?.DeleteTrack(focusedTrack);

            if (timeline != null)
            {
                if (timeline.Tracks.Count > 0)
                {
                    if (focusedIndex >= 0)
                    {
                        if (focusedIndex >= timeline.Tracks.Count)
                        {
                            timeline.Tracks[timeline.Tracks.Count - 1].SetIsSelected(true, true);
                        }
                        else
                        {
                            timeline.Tracks[focusedIndex].SetIsSelected(true, true);
                        }
                    }
                    else
                    {
                        timeline.Tracks[0].SetIsSelected(true, true);
                    }
                }

                VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
            }

            return Task.CompletedTask;
        }
    }
}