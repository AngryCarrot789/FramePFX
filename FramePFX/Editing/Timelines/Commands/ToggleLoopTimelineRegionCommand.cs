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

using FramePFX.CommandSystem;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Timelines.Commands;

public class ToggleLoopTimelineRegionCommand : Command {
    /// <summary>
    /// Gets or sets the special behaviour state which allows us to update the loop region around
    /// selected clips if applicable and keep the loop enabled rather than toggle it. If false, we just toggle it as usual
    /// </summary>
    public bool CanUpdateRegionToClipSelection { get; init; }

    public ToggleLoopTimelineRegionCommand() {
    }

    protected override void Execute(CommandEventArgs e) {
        if (!DataKeys.TimelineKey.TryGetContext(e.ContextData, out Timeline? timeline)) {
            return;
        }

        // Create loop region spanning the whole timeline as a default value
        if (this.CanUpdateRegionToClipSelection) {
            if (DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? ui)) {
                if (FrameSpan.TryUnionAll(ui.ClipSelection.SelectedItems.Select(x => x.Clip.FrameSpan), out FrameSpan span)) {
                    // If the region is not the same, then the user just wanted to update their loop region
                    // So we won't actually toggle it but instead ensure it's enabled and also update the region
                    if (timeline.LoopRegion != span) {
                        timeline.LoopRegion = span;
                        timeline.IsLoopRegionEnabled = true;
                        return;
                    }
                }
            }
        }
        else if (!timeline.IsLoopRegionEnabled && !timeline.LoopRegion.HasValue) {
            timeline.LoopRegion = new FrameSpan(0, timeline.MaxDuration);
        }

        timeline.IsLoopRegionEnabled = !timeline.IsLoopRegionEnabled;
    }
}