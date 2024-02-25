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
using System.Diagnostics;
using System.IO;
using FramePFX.CommandSystem;
using FramePFX.Editors.Automation;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Interactivity.Contexts;
using FramePFX.Progression;
using FramePFX.Utils;

namespace FramePFX.Editors.Commands {
    public class OpenProjectCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.VideoEditorKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor)) {
                return;
            }

            string filePath = IoC.FilePickService.OpenFile("Open a project file (.fpfx)", Filters.ProjectType);
            if (filePath == null) {
                return;
            }

            if (!File.Exists(filePath)) {
                IoC.MessageService.ShowMessage("No such file", "That project file does not exist");
                return;
            }

            IProgressTracker tracker = new ModalProgressTracker() {
                IsIndeterminate = false, HeaderText = "Opening project..."
            };

            ActivityDialog dialog = ActivityDialog.ShowAsNonModal(tracker);
            try {
                tracker.CompletionValue = 0.2;
                if (CloseProjectCommand.CloseProject(editor, tracker)) {
                    tracker.CompletionValue = 0.7;
                    OpenProjectAt(editor, filePath, tracker);
                    tracker.CompletionValue = 1.0;
                }
            }
            finally {
                dialog.Close();
            }
        }

        public static bool OpenProjectAt(VideoEditor editor, string filePath, IProgressTracker tracker) {
            Project project;
            if (tracker != null)
                tracker.Text = "Reading project data from file";

            try {
                project = Project.ReadProjectAt(filePath);
            }
            catch (Exception ex) {
                IoC.MessageService.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                return false;
            }

            if (tracker != null)
                tracker.Text = "Loading project";
            editor.SetProject(project);

            if (tracker != null)
                tracker.Text = "Loading resources";
            if (!ResourceLoaderDialog.TryLoadResources(project.ResourceManager.RootContainer)) {
                try {
                    editor.CloseProject();
                }
                catch (Exception e) {
                    IoC.MessageService.ShowMessage("Close Error", "An exception occurred while closing the project", e.GetToString());
                }

                return false;
            }

            if (tracker != null)
                tracker.Text = "Updating automation and rendering";

            project.SetUnModified();
            AutomationEngine.UpdateValues(project.MainTimeline);
            project.MainTimeline.RenderManager.InvalidateRender();
            Debug.Assert(project.IsModified == false, "Expected automation update and render invalidation to not mark project as modified");

            return true;
        }
    }
}