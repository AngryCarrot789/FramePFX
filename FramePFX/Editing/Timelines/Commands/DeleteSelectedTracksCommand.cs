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

using FramePFX.Editing.Timelines.Tracks;
using FramePFX.Editing.UI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;

namespace FramePFX.Editing.Timelines.Commands;

public class DeleteSelectedTracksCommand : Command {
    public static bool GetTrackSelection(IContextData data, out Track[] tracks) {
        if (DataKeys.TimelineUIKey.TryGetContext(data, out ITimelineElement? timeline)) {
            if (timeline.Selection.Count > 0) {
                tracks = timeline.Selection.SelectedItems.Select(x => x.Track).ToArray();
                return true;
            }
        }

        if (DataKeys.TrackKey.TryGetContext(data, out Track? track)) {
            tracks = new Track[] { track };
            return true;
        }

        tracks = null;
        return false;
    }

    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return GetTrackSelection(e.ContextData, out _) ? Executability.Valid : Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (!GetTrackSelection(e.ContextData, out Track[] tracks))
            return Task.CompletedTask;

        foreach (Track track in tracks) {
            track.Timeline?.RemoveTrack(track);
            track.Destroy();
        }

        return Task.CompletedTask;
    }
}