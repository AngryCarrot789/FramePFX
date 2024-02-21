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

using System.Windows;
using FramePFX.CommandSystem;
using FramePFX.Editors.Timelines.Tracks;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Editors.Commands {
    public class NewProjectCommand : Command {
        // true: project was already closed or is now closed
        // false: close was cancelled; cancel entire operation

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor)) {
                return;
            }

            if (!CloseProjectCommand.CloseProject(editor)) {
                return;
            }

            Project project = new Project();
            VideoTrack track = new VideoTrack() {
                DisplayName = "Video Track 1"
            };

            project.MainTimeline.AddTrack(track);
            editor.SetProject(project);
        }
    }
}