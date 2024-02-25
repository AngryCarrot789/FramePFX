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
using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Editors.Automation;
using FramePFX.Editors.ResourceManaging.Autoloading.Controls;
using FramePFX.Interactivity.Contexts;
using FramePFX.Tasks;
using FramePFX.Utils;

namespace FramePFX.Editors.Commands {
    public class OpenProjectCommand : Command {
        public override ExecutabilityState CanExecute(CommandEventArgs e) {
            return e.ContextData.ContainsKey(DataKeys.VideoEditorKey) ? ExecutabilityState.Executable : ExecutabilityState.Invalid;
        }

        public override async Task Execute(CommandEventArgs e) {
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

            await RunOpenProjectTask(editor, filePath);
        }

        public static ActivityTask RunOpenProjectTask(VideoEditor editor, string filePath) {
            return TaskManager.Instance.RunTask(async () => {
                IActivityProgress progress = TaskManager.Instance.CurrentTask.Progress;

                bool result;
                using (progress.PushCompletionRange(0.0, 0.5)) {
                    result = await CloseProjectCommand.CloseProjectInternal(editor, progress);
                }

                if (result) {
                    using (progress.PushCompletionRange(0.5, 1.0)) {
                        await OpenProjectAtInternal(editor, filePath, progress);
                    }
                }

            }, new DefaultProgressTracker());
        }

        public static async Task<bool> OpenProjectAtInternal(VideoEditor editor, string filePath, IActivityProgress progress) {
            Project project;

            if (progress == null)
                progress = EmptyActivityProgress.Instance;

            using (progress.PushCompletionRange(0.0, 0.3)) {
                progress.Text = "Reading project data from file";
                progress.OnProgress(0.5);
                try {
                    project = Project.ReadProjectAt(filePath);
                }
                catch (Exception ex) {
                    IoC.MessageService.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                    return false;
                }
                progress.OnProgress(0.5);
            }

            using (progress.PushCompletionRange(0.3, 0.6)) {
                progress.Text = "Loading project";
                progress.OnProgress(0.5);
                await IoC.Dispatcher.InvokeAsync(() => {
                    if (editor.Project != null)
                        editor.CloseProject();
                    editor.SetProject(project);
                });
                progress.OnProgress(0.5);
            }

            using (progress.PushCompletionRange(0.6, 0.9)) {
                progress.Text = "Loading resources";
                progress.OnProgress(0.5);

                bool reuslt = await IoC.Dispatcher.InvokeAsync(() => {
                    if (!ResourceLoaderDialog.TryLoadResources(project.ResourceManager.RootContainer)) {
                        try {
                            editor.CloseProject();
                        }
                        catch (Exception e) {
                            IoC.MessageService.ShowMessage("Close Error", "An exception occurred while closing the project", e.GetToString());
                        }

                        return false;
                    }

                    return true;
                });

                if (!reuslt) {
                    return false;
                }

                progress.OnProgress(0.5);
            }

            using (progress.PushCompletionRange(0.9, 1.0)) {
                progress.Text = "Updating automation and rendering";
                progress.OnProgress(0.5);

                await IoC.Dispatcher.InvokeAsync(() => {
                    project.SetUnModified();
                    AutomationEngine.UpdateValues(project.MainTimeline);
                    project.MainTimeline.RenderManager.InvalidateRender();
                    Debug.Assert(project.IsModified == false, "Expected automation update and render invalidation to not mark project as modified");
                });

                progress.OnProgress(0.5);
            }

            return true;
        }
    }
}