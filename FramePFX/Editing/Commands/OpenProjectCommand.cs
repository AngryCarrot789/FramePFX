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
using FramePFX.Editing.Automation;
using FramePFX.Editing.ResourceManaging;
using FramePFX.Services.FilePicking;
using FramePFX.Services.Messaging;
using FramePFX.Tasks;
using FramePFX.Utils;
using DataKeys = FramePFX.Interactivity.Contexts.DataKeys;

namespace FramePFX.Editing.Commands;

public class OpenProjectCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return DataKeys.VideoEditorKey.GetExecutabilityForPresence(e.ContextData);
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor? editor)) {
            return;
        }

        string? filePath = await IFilePickDialogService.Instance.OpenFile("Open a project file (.fpfx)", Filters.ListProjectTypeAndAll);
        if (filePath == null) {
            return;
        }

        if (!File.Exists(filePath)) {
            await IMessageDialogService.Instance.ShowMessage("No such file", "That project file does not exist");
            return;
        }

        await RunOpenProjectTask(editor, filePath);
    }

    public static ActivityTask<bool> RunOpenProjectTask(VideoEditor editor, string filePath) {
        return TaskManager.Instance.RunTask<bool>(async () => {
            IActivityProgress progress = TaskManager.Instance.CurrentTask.Progress;

            bool result;
            using (progress.CompletionState.PushCompletionRange(0.0, 0.5)) {
                result = await CloseProjectCommand.CloseProjectBGT(editor, progress);
            }

            if (result) {
                using (progress.CompletionState.PushCompletionRange(0.5, 1.0)) {
                    return await OpenProjectAtBGT(editor, filePath, progress);
                }
            }

            return false;
        }, new DefaultProgressTracker());
    }

    public static async Task<bool> OpenProjectAtBGT(VideoEditor editor, string filePath, IActivityProgress? progress) {
        Project project;

        if (progress == null)
            progress = EmptyActivityProgress.Instance;

        using (progress.CompletionState.PushCompletionRange(0.0, 0.4)) {
            progress.Text = "Reading project data from file";
            progress.CompletionState.OnProgress(0.5);

            try {
                project = Project.ReadProjectAt(filePath);
            }
            catch (Exception ex) {
                await IMessageDialogService.Instance.ShowMessage("Read Error", "An exception occurred while reading the project", ex.GetToString());
                return false;
            }

            progress.CompletionState.OnProgress(0.5);
        }

        using (progress.CompletionState.PushCompletionRange(0.4, 0.6)) {
            progress.Text = "Loading project";
            progress.CompletionState.OnProgress(0.5);
            await Application.Instance.Dispatcher.InvokeAsync(() => {
                if (editor.Project != null)
                    editor.CloseProject();
                editor.SetProject(project);
            });
            progress.CompletionState.OnProgress(0.5);
        }

        using (progress.CompletionState.PushCompletionRange(0.6, 0.9)) {
            progress.Text = "Loading resources";
            progress.CompletionState.OnProgress(0.5);

            bool result = await await Application.Instance.Dispatcher.InvokeAsync(async () => {
                if (!await IResourceLoaderDialogService.Instance.TryLoadResource(project.ResourceManager.RootContainer)) {
                    try {
                        editor.CloseProject();
                    }
                    catch (Exception e) {
                        await IMessageDialogService.Instance.ShowMessage("Close Error", "An exception occurred while closing the project", e.GetToString());
                    }

                    return false;
                }

                return true;
            }, DispatchPriority.Input);

            if (!result)
                return false;

            progress.CompletionState.OnProgress(0.5);
        }

        using (progress.CompletionState.PushCompletionRange(0.9, 1.0)) {
            progress.Text = "Updating automation and rendering";
            progress.CompletionState.OnProgress(0.5);

            try {
                await Application.Instance.Dispatcher.InvokeAsync(() => {
                    project.SetUnModified();
                    AutomationEngine.UpdateValues(project.ActiveTimeline);
                    project.MainTimeline.RenderManager.InvalidateRender();
                }, DispatchPriority.Input);
            }
            catch (Exception e) {
                await IMessageDialogService.Instance.ShowMessage("Error updating automation", e.GetToString());
            }

            if (project.IsModified) {
                await IMessageDialogService.Instance.ShowMessage("Warning", "Issue: project was marked modified during automation update, which should not happen");
            }

            progress.CompletionState.OnProgress(0.5);
        }

        await Task.Delay(100);

        return true;
    }
}