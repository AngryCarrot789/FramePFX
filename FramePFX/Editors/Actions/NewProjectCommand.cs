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
using FramePFX.Interactivity.DataContexts;

namespace FramePFX.Editors.Actions {
    public class NewProjectCommand : Command {
        // true: project was already closed or is now closed
        // false: close was cancelled; cancel entire operation
        public static bool CloseProject(VideoEditor editor) {
            Project oldProject = editor.Project;
            if (oldProject == null) {
                return true;
            }

            MessageBoxResult result = IoC.MessageService.ShowMessage("Project already open", "A project is already open. Do you want to save it?", MessageBoxButton.YesNoCancel);
            switch (result) {
                case MessageBoxResult.Cancel: return false;
                case MessageBoxResult.Yes: {
                    bool? saveResult = SaveProjectCommand.SaveProject(editor.Project);
                    if (!saveResult.HasValue) {
                        return false;
                    }

                    editor.CloseProject();
                    break;
                }
                default: {
                    editor.CloseProject();
                    break;
                }
            }

            return true;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.DataContext, out VideoEditor editor)) {
                return;
            }

            if (!CloseProject(editor)) {
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