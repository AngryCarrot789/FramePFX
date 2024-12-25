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
using FramePFX.Interactivity.Contexts;
using FramePFX.Services.Messaging;
using FramePFX.Tasks;
using DataKeys = FramePFX.Interactivity.Contexts.DataKeys;

namespace FramePFX.Editing.Commands;

public class SaveProjectCommand : AsyncCommand {
    protected override Executability CanExecuteOverride(CommandEventArgs e) {
        return e.ContextData.ContainsKey(DataKeys.ProjectKey) ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project? project)) {
            if (project.IsSaving) {
                await IMessageDialogService.Instance.ShowMessage("Already Saving", "Project is already saving!");
                return;
            }

            await TaskManager.Instance.RunTask(async () => {
                IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
                progress.Text = "Saving project...";

                await Application.Instance.Dispatcher.InvokeAsync(async () => {
                    await Project.SaveProject(project, progress);
                });
            });
        }
    }
}

public class SaveProjectAsCommand : SaveProjectCommand {
    protected override async Task ExecuteAsync(CommandEventArgs e) {
        if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project? project)) {
            await TaskManager.Instance.RunTask(async () => {
                IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
                progress.Text = "Saving project as...";

                await await Application.Instance.Dispatcher.InvokeAsync(async () => {
                    await Project.SaveProjectAs(project, progress);
                });
            });
        }
    }
}