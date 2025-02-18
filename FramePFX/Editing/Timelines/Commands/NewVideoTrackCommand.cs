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

public class NewVideoTrackCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return e.ContextData.ContainsKey(DataKeys.TimelineKey) ? Executability.Valid : Executability.Invalid;
    }

    protected override Task ExecuteCommandAsync(CommandEventArgs e) {
        // if (DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? timelineElement)) {
        //     var seleted = timelineElement.Selection.SelectedItems.FirstOrDefault();
        // }
        if (DataKeys.TrackUIKey.TryGetContext(e.ContextData, out ITrackElement? trackUIContext)) {
            Timeline timeline = trackUIContext.Timeline.Timeline!;
            int index = timeline.IndexOf(trackUIContext.Track!);
            if (index == -1)
                throw new Exception("Fatal error... track index error");

            VideoTrack track = new VideoTrack() {
                DisplayName = "New Video Track"
            };

            timeline.InsertTrack(index, track);
        }
        else if (DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? timeline)) {
            VideoTrack track = new VideoTrack() {
                DisplayName = "New Video Track"
            };

            timeline.InsertTrack(0, track);
        }

        return Task.CompletedTask;
    }
}