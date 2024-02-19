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
using FramePFX.Utils;

namespace FramePFX.Editors.Actions {
    public class OpenProjectCommand : Command {
        public override bool CanExecute(CommandEventArgs e) {
            return e.Context.ContainsKey(DataKeys.VideoEditorKey);
        }

        public override void Execute(CommandEventArgs e) {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.Context, out VideoEditor editor)) {
                return;
            }

            if (!NewProjectCommand.CloseProject(editor)) {
                return;
            }

            string filePath = IoC.FilePickService.OpenFile("Open a project file (.fpfx)", Filters.ProjectType);
            if (filePath == null) {
                return;
            }

            if (!File.Exists(filePath)) {
                IoC.MessageService.ShowMessage("No such file", "That project file does not exist");
            }

            OpenProjectAt(editor, filePath);
        }

        public static bool OpenProjectAt(VideoEditor editor, string filePath) {
            Project project;
            try {
                project = Project.ReadProjectAt(filePath);
            }
            catch (Exception ex) {
                IoC.MessageService.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                return false;
            }

            editor.SetProject(project);

            if (!ResourceLoaderDialog.TryLoadResources(project.ResourceManager.RootContainer)) {
                try {
                    editor.CloseProject();
                }
                catch (Exception e) {
                    IoC.MessageService.ShowMessage("Close Error", "An exception occurred while closing the project", e.GetToString());
                }

                return false;
            }

            project.SetUnModified();
            AutomationEngine.UpdateValues(project.MainTimeline);
            project.MainTimeline.RenderManager.InvalidateRender();
            Debug.Assert(project.IsModified == false, "Expected automation update and render invalidation to not mark project as modified");

            return true;
        }
    }
}