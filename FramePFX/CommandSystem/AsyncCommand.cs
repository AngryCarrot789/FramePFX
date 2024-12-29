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
using FramePFX.Logging;
using FramePFX.Services.Messaging;
using FramePFX.Utils;

namespace FramePFX.CommandSystem;

public delegate void AsyncCommandEventHandler(AsyncCommand command);

/// <summary>
/// A command that has an async execute method, and tracks the completion of the task returned and
/// only allows the command to be executed once the previous task becomes completed
/// </summary>
public abstract class AsyncCommand : Command {
    protected readonly bool allowMultipleExecutions;
    private bool isExecuting, isInExecutingFrame;

    /// <summary>
    /// An event fired when this command's executing state changes. This is fired on the main thread
    /// </summary>
    public event AsyncCommandEventHandler? ExecutingChanged;
    
    public bool IsExecuting => this.isExecuting;
    
    public bool AllowMultipleExecutions => this.allowMultipleExecutions;
    
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

    protected sealed override void Execute(CommandEventArgs e) {
        this.isInExecutingFrame = true;
        try {
            this.ExecuteAsyncImpl(e);
        }
        finally {
            this.isInExecutingFrame = false;
        }
    }

    private async void ExecuteAsyncImpl(CommandEventArgs args) {
        // we need to handle exceptions here, because otherwise the application
        // would never catch it, and therefore the exception would be lost forever
        
        if (!this.allowMultipleExecutions && this.isExecuting) {
            try {
                await this.OnAlreadyExecuting(args);
            }
            catch {
                AppLogger.Instance.WriteLine($"Exception invoking {nameof(this.OnAlreadyExecuting)}");
            }
            
            return;
        }
        
        this.isExecuting = true;
        
        try {
            this.ExecutingChanged?.Invoke(this);
        }
        catch {
            AppLogger.Instance.WriteLine($"Exception raising {nameof(this.ExecutingChanged)}");
        }
        
        try {
            await (this.ExecuteAsync(args) ?? Task.CompletedTask);
        }
        catch (Exception e) when (!Debugger.IsAttached) {
            try {
                await this.OnExecutionException(args, e);
            }
            catch {
                AppLogger.Instance.WriteLine($"Exception invoking {nameof(this.OnExecutionException)}");
            }
        }

        if (this.isInExecutingFrame) {
            try {
                this.SetIsExecuting(false);
            }
            catch {
                AppLogger.Instance.WriteLine($"Exception raising {nameof(this.ExecutingChanged)}");
            }
        }
        else {
            Application.Instance.Dispatcher.Post(() => {
                this.SetIsExecuting(false);
            });
        }
    }

    private void SetIsExecuting(bool isExecuting) {
        this.isExecuting = isExecuting;
        this.ExecutingChanged?.Invoke(this);
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