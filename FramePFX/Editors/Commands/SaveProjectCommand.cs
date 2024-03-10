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

using System.Threading.Tasks;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Tasks;

namespace FramePFX.Editors.Commands
{
    public class SaveProjectCommand : AsyncCommand
    {
        protected override Executability CanExecuteCore(CommandEventArgs e)
        {
            return e.ContextData.ContainsKey(DataKeys.ProjectKey) ? Executability.Valid : Executability.Invalid;
        }

        protected override async Task ExecuteAsync(CommandEventArgs e)
        {
            if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project project))
            {
                if (project.IsSaving)
                {
                    IoC.MessageService.ShowMessage("Already Saving", "Project is already saving!");
                    return;
                }

                await TaskManager.Instance.RunTask(async () =>
                {
                    IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
                    progress.Text = "Saving project...";

                    await IoC.Dispatcher.InvokeAsync(() =>
                    {
                        Project.SaveProject(project, progress);
                    });
                });
            }
        }
    }

    public class SaveProjectAsCommand : SaveProjectCommand
    {
        protected override async Task ExecuteAsync(CommandEventArgs e)
        {
            if (DataKeys.ProjectKey.TryGetContext(e.ContextData, out Project project))
            {
                await TaskManager.Instance.RunTask(async () =>
                {
                    IActivityProgress progress = TaskManager.Instance.GetCurrentProgressOrEmpty();
                    progress.Text = "Saving project as...";

                    await IoC.Dispatcher.InvokeAsync(() =>
                    {
                        Project.SaveProjectAs(project, progress);
                    });
                });
            }
        }
    }
}