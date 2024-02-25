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

using System;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Tasks;
using FramePFX.Utils;

namespace FramePFX.Editors.Commands {
    public class SaveProjectCommand : Command {
        public static bool? SaveProject(Project project) {
            if (project.HasSavedOnce && !string.IsNullOrEmpty(project.ProjectFilePath)) {
                return SaveProjectInternal(project, project.ProjectFilePath);
            }
            else {
                return SaveProjectAs(project);
            }
        }

        public static bool? SaveProjectAs(Project project) {
            const string message = "Specify a file path for the project file. Any project data will be stored in the same folder, so it's best to create a project-specific folder";
            string filePath = IoC.FilePickService.SaveFile(message, Filters.ProjectType, project.ProjectFilePath);
            if (filePath == null) {
                return null;
            }

            return SaveProjectInternal(project, filePath);
        }

        private static bool SaveProjectInternal(Project project, string filePath) {
            project.Editor?.Playback.Pause();

            IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
            progress.Text = "Serialising project...";

            progress.OnProgress(0.5);
            try {
                project.WriteToFile(filePath);
                progress.OnProgress(0.5);
                return true;
            }
            catch (Exception e) {
                IoC.MessageService.ShowMessage("Save Error", "An exception occurred while saving project", e.GetToString());
                progress.OnProgress(0.5);
                return false;
            }
        }

        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.ProjectKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project project)) {
                SaveProject(project);
            }
        }
    }

    public class SaveProjectAsCommand : SaveProjectCommand {
        public override void Execute(CommandEventArgs e) {
            if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project project)) {
                SaveProjectAs(project);
            }
        }
    }
}