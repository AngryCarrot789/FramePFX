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
    public class SelectAllTracksCommand : Command
    {
        public static void SelectAll(Timeline timeline, Track focusedTrack, bool selectClipsToo = false)
        {
            foreach (Track t in timeline.Tracks)
            {
                if (selectClipsToo)
                {
                    t.SelectAll();
                }

                t.SetIsSelected(true);
            }

            if (focusedTrack != null)
            {
                focusedTrack.SetIsSelected(true, true);
            }
            else if (timeline.Tracks.Count > 0)
            {
                timeline.Tracks[timeline.Tracks.Count - 1].SetIsSelected(true, true);
            }

            VideoEditorPropertyEditor.Instance.UpdateTrackSelectionAsync(timeline);
        }

        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track) && track.Timeline != null)
                return ExecutabilityState.Executable;
            return e.ContextData.ContainsKey(DataKeys.TimelineKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e)
        {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track) && ((timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.ContextData, out timeline)))
            {
                SelectAll(timeline, track, false);
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTrackCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            return e.ContextData.ContainsKey(DataKeys.TrackKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e)
        {
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track))
            {
                track.SelectAll();
                VideoEditorPropertyEditor.Instance.UpdateClipSelectionAsync(track.Timeline);
            }

            return Task.CompletedTask;
        }
    }

    public class SelectAllClipsInTimelineCommand : Command
    {
        public override ExecutabilityState CanExecute(CommandEventArgs e)
        {
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track) && track.Timeline != null)
                return ExecutabilityState.Executable;
            return e.ContextData.ContainsKey(DataKeys.TimelineKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override Task Execute(CommandEventArgs e)
        {
            Timeline timeline;
            if (DataKeys.TrackKey.TryGetContext(e.ContextData, out Track track) && ((timeline = track.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.ContextData, out timeline)))
            {
                SelectAllTracksCommand.SelectAll(timeline, track, true);
            }

            return Task.CompletedTask;
        }
    }
}