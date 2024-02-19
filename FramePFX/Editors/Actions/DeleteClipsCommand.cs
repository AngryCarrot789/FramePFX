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
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines;
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipsCommand : Command {
        public override void Execute(CommandEventArgs e) {
            HashSet<Clip> clips = new HashSet<Clip>();
            if (DataKeys.ClipKey.TryGetContext(e.DataContext, out Clip focusedClip)) {
                clips.Add(focusedClip);
            }

            Timeline timeline;
            if ((timeline = focusedClip?.Timeline) != null || DataKeys.TimelineKey.TryGetContext(e.DataContext, out timeline)) {
                foreach (Clip clip in timeline.SelectedClips) {
                    clips.Add(clip);
                }
            }

            foreach (Clip clip in clips) {
                clip.Destroy();
                clip.Track.RemoveClip(clip);
            }
        }
    }
}