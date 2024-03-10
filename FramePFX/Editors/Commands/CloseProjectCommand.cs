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
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Threading.Tasks;
using System.Windows;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Tasks;

namespace FramePFX.Editors.Commands
{
    public class CloseProjectCommand : AsyncCommand
    {
        protected override Executability CanExecuteCore(CommandEventArgs e)
        {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor))
                return Executability.Invalid;
            return editor.Project == null ? Executability.ValidButCannotExecute : Executability.Valid;
        }

        protected override async Task ExecuteAsync(CommandEventArgs e)
        {
            if (!DataKeys.VideoEditorKey.TryGetContext(e.ContextData, out VideoEditor editor))
                return;

            await TaskManager.Instance.RunTask(async () =>
            {
                IActivityProgress prog = TaskManager.Instance.CurrentTask.Progress;
                prog.Text = "Closing project...";
                await CloseProjectBGT(editor, prog);
            });
        }

        public static async Task<bool> CloseProjectBGT(VideoEditor editor, IActivityProgress progress, string msgTitle = "Project is open", string message = "A project is open. Do you want to save it?")
        {
            Project oldProject = editor.Project;
            if (oldProject == null)
                return true;

            if (progress == null)
                progress = TaskManager.Instance.GetCurrentProgressOrEmpty();

            progress.Text = "Closing active project";
            progress.OnProgress(0.2);
            MessageBoxResult result = await IoC.Dispatcher.InvokeAsync(() => IoC.MessageService.ShowMessage(msgTitle, message, MessageBoxButton.YesNoCancel));
            switch (result)
            {
                case MessageBoxResult.Cancel: return false;
                case MessageBoxResult.Yes:
                {
                    bool? saveResult;
                    using (progress.PushCompletionRange(0.2, 0.5))
                    {
                        progress.Text = "Saving project...";
                        progress.OnProgress(0.5);
                        saveResult = await IoC.Dispatcher.InvokeAsync(() => Project.SaveProject(editor.Project, progress));
                        progress.OnProgress(0.5);
                    }

                    using (progress.PushCompletionRange(0.5, 0.8))
                    {
                        if (saveResult.HasValue)
                        {
                            progress.Text = "Closing project...";
                            progress.OnProgress(0.5);
                            await IoC.Dispatcher.InvokeAsync(editor.CloseProject);
                            progress.OnProgress(0.5);
                        }
                        else
                        {
                            progress.OnProgress(1.0);
                            return false;
                        }
                    }

                    break;
                }
                default:
                {
                    using (progress.PushCompletionRange(0.2, 0.8))
                    {
                        progress.Text = "Closing project...";
                        progress.OnProgress(0.5);
                        await IoC.Dispatcher.InvokeAsync(editor.CloseProject);
                        progress.OnProgress(0.5);
                    }

                    break;
                }
            }

            progress.OnProgress(0.2);
            return true;
        }
    }
}