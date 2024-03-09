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

using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;
using FramePFX.PropertyEditing;

namespace FramePFX.Editors.Commands
{
    public class DeleteSpecificTrackCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track))
                return ExecutabilityState.Invalid;
            return track.Timeline != null ? ExecutabilityState.Executable : ExecutabilityState.ValidButCannotExecute;
        }

        public override Task Execute(CommandEventArgs e)
        {
            if (!DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track))
                return Task.CompletedTask;
            Timeline timeline = track.Timeline;
            if (timeline == null)
                return Task.CompletedTask;
            int index = track.IndexInTimeline;
            timeline.DeleteTrack(track);

            if (timeline.Tracks.Count > 0)
            {
                if (index >= 0)
                {
                    if (index >= timeline.Tracks.Count)
                    {
                        timeline.Tracks[timeline.Tracks.Count - 1].SetIsSelected(true, true);
                    }
                    else
                    {
                        timeline.Tracks[index].SetIsSelected(true, true);
                    }
                }
                else
                {
                    timeline.Tracks[0].SetIsSelected(true, true);
                }
            }

            VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
            return Task.CompletedTask;
        }
    }
}