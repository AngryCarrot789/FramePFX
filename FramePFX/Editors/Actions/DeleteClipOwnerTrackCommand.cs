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
using FramePFX.Editors.Timelines.Clips;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Actions {
    public class DeleteClipOwnerTrackCommand : Command {
        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ClipKey.TryGetContext(e.Context, out Clip clip)) {
                clip.Timeline?.DeleteTrack(clip.Track);
            }
        }
    }
}