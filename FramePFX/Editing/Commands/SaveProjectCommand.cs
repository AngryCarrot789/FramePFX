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

using PFXToolKitUI;
using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Tasks;

namespace FramePFX.Editing.Commands;

public class SaveProjectCommand : Command {
    protected override Executability CanExecuteCore(CommandEventArgs e) {
        return e.ContextData.ContainsKey(DataKeys.ProjectKey) ? Executability.Valid : Executability.Invalid;
    }

    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project? project)) {
            if (project.IsSaving) {
                await IMessageDialogService.Instance.ShowMessage("Already Saving", "Project is already saving!");
                return;
            }

            await ActivityManager.Instance.RunTask(async () => {
                IActivityProgress progress = ActivityManager.Instance.GetCurrentProgressOrEmpty();
                progress.Text = "Saving project...";

                await Application.Instance.Dispatcher.InvokeAsync(async () => {
                    await Project.SaveProject(project, progress);
                });
            });
        }
    }
}

public class SaveProjectAsCommand : SaveProjectCommand {
    protected override async Task ExecuteCommandAsync(CommandEventArgs e) {
        if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project? project)) {
            await ActivityManager.Instance.RunTask(async () => {
                IActivityProgress progress = ActivityManager.Instance.GetCurrentProgressOrEmpty();
                progress.Text = "Saving project as...";

                await Application.Instance.Dispatcher.InvokeAsync(() => Project.SaveProjectAs(project, progress)).Unwrap();
            });
        }
    }
}