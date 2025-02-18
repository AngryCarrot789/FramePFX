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

using FramePFX.Editing.UI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Utils;

namespace FramePFX.Editing.Timelines.Commands;

public class SelectClipsInTracksCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return DataKeys.TimelineUIKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        if (DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? timeline)) {
            List<ITrackElement> selection = timeline.Selection.SelectedItems.ToList();
            if (DataKeys.TrackUIKey.TryGetContext(e.ContextData, out ITrackElement? contextTrack)) {
                selection.TryAdd(contextTrack);
            }

            foreach (ITrackElement track in selection) {
                track.Selection.SelectAll();
            }
        }

        return Task.CompletedTask;
    }
}