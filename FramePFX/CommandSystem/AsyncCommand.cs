// 
// Copyright (c) 2024-2024 REghZy
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

using System.Diagnostics;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.CommandSystem;

/// <summary>
/// A command that has an async execute method, and tracks the completion of the task returned and
/// only allows the command to be executed once the previous task becomes completed
/// </summary>
public abstract class AsyncCommand : Command {
    protected readonly bool allowMultipleExecutions;
    private bool isExecuting;

    /// <summary>
    /// Constructor for the async command
    /// </summary>
    /// <param name="allowMultipleExecutions">
    /// True to allow this command to be executed multiple times even if it was executed previously
    /// and the task has not completed, e.g. downloading a file.
    /// False to disallow execution while the previous task is still running. This is the default value
    /// </param>
    protected AsyncCommand(bool allowMultipleExecutions = false) {
        this.allowMultipleExecutions = allowMultipleExecutions;
    }

    public sealed override Executability CanExecute(CommandEventArgs e) {
        Executability result = this.CanExecuteOverride(e);

        // Prevent ValidButCannotExecute being used first
        if (result == Executability.Invalid)
            return result;

        return this.isExecuting ? Executability.ValidButCannotExecute : result;
    }

    protected virtual Executability CanExecuteOverride(CommandEventArgs e) {
        return Executability.Valid;
    }

    protected sealed override void Execute(CommandEventArgs e) => this.ExecuteAsyncImpl(e);

    private async void ExecuteAsyncImpl(CommandEventArgs args) {
        try {
            if (!this.allowMultipleExecutions && this.isExecuting) {
                await this.OnAlreadyExecuting(args);
                return;
            }

            this.isExecuting = true;
            await (this.ExecuteAsync(args) ?? Task.CompletedTask);
        }
        catch (Exception e) when (!Debugger.IsAttached) {
            // we need to handle the exception here, because otherwise the application
            // would never catch it, and therefore the exception would be lost forever
            await this.OnExecutionException(args, e);
        }
        finally {
            this.isExecuting = false;
        }
    }

    protected abstract Task ExecuteAsync(CommandEventArgs e);

    /// <summary>
    /// Invoked when this command is already running, but <see cref="Execute"/> was called again.
    /// By default, this shows a message box
    /// </summary>
    /// <param name="args">Command event args</param>
    protected virtual Task OnAlreadyExecuting(CommandEventArgs args) {
        if (args.IsUserInitiated)
            return IMessageDialogService.Instance.ShowMessage("Already running", "This command is already running");

        return Task.CompletedTask;
    }

    protected virtual Task OnExecutionException(CommandEventArgs args, Exception e) {
        return IMessageDialogService.Instance.ShowMessage("Command Error", "An exception occurred while executing command", e.GetToString());
    }
}