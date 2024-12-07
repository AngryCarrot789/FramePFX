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
using FramePFX.Editing.Timelines.Clips;
using FramePFX.Editing.UI;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editing.Timelines.Commands;

public class DeleteClipsCommand : Command {
    protected override void Execute(CommandEventArgs e) {
        if (DataKeys.TimelineUIKey.TryGetContext(e.ContextData, out ITimelineElement? timeline)) {
            List<IClipElement> list = timeline.ClipSelection.SelectedItems.ToList();
            
            // Must clear the selection sine removing clips doesn't automatically de-selet them at the moment
            timeline.ClipSelection.Clear();
            
            foreach (IClipElement clip in list) {
                Clip model = clip.Clip;
                model.Track?.RemoveClip(model);
                model.Destroy();
            }
        }
    }
}