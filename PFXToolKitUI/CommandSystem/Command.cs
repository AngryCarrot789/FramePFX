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
using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Logging;
using PFXToolKitUI.Services.Messaging;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// A class that represents something that can be executed. Commands are given contextual
/// information (see <see cref="CommandEventArgs.ContextData"/>) to do work.
/// Commands do their work in the <see cref="ExecuteCommand_NotAsync"/> method, and can optionally specify
/// their executability via the <see cref="CanExecuteCore"/> method
/// <para>
/// Commands are used primarily by the shortcut and advanced menu service to do
/// work, but they can also be used by things like buttons
/// </para>
/// <para>
/// These commands can be executed through the <see cref="CommandManager.Execute(string,Command,IContextData,bool)"/> function
/// </para>
/// </summary>
public abstract class Command {
    private bool isExecuting;
    private Task? myTask;

    /// <summary>
    /// An event fired when this command's executing state changes. This is fired on the main thread
    /// </summary>
    public event AsyncCommandEventHandler? ExecutingChanged;

    public bool AllowMultipleExecutions { get; }

    /// <summary>
    /// Gets the task that is executing. Only set when <see cref="AllowMultipleExecutions"/> is false
    /// </summary>
    public Task? CurrentTask => this.myTask;

    public bool IsExecuting => this.myTask != null && !this.myTask.IsCompleted;

    protected Command(bool allowMultipleExecutions = false) {
        this.AllowMultipleExecutions = allowMultipleExecutions;
    }

    // When focus changes, raise notification to update commands
    // Then fire ContextDataChanged for those command hooks or whatever, they can then disconnect
    // old event handlers and attach new ones

    public Executability CanExecute(CommandEventArgs e) {
        Executability result = this.CanExecuteCore(e);

        // Prevent ValidButCannotExecute being used first
        if (result == Executability.Invalid)
            return result;

        return this.isExecuting ? Executability.ValidButCannotExecute : result;
    }

    /// <summary>
    /// Gets this command's executability state based on the given command event args context.
    /// This typically isn't checked before <see cref="CanExecuteCore"/> is invoked, but instead is
    /// mainly used by the UI to determine if something like a button or menu item is actually clickable
    /// <para>
    /// This method should be quick to execute, as it may be called quite often
    /// </para>
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <returns>
    /// True if executing this command would most likely result in success, otherwise false
    /// </returns>
    protected virtual Executability CanExecuteCore(CommandEventArgs e) => Executability.Valid;

    /// <summary>
    /// Executes this command with the given command event args. This is always called on the main application thread (AMT)
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    protected abstract Task ExecuteCommandAsync(CommandEventArgs e);

    internal static Task InternalExecute(string? cmdId, Command command, CommandEventArgs e) {
        return command.ExecuteImpl(cmdId, e);
    }

    private async Task ExecuteImpl(string? cmdId, CommandEventArgs args) {
        ApplicationPFX.Instance.Dispatcher.VerifyAccess();

        if (!this.AllowMultipleExecutions && this.isExecuting) {
            try {
                await this.OnAlreadyExecuting(args);
            }
            catch {
                AppLogger.Instance.WriteLine($"Exception invoking {nameof(this.OnAlreadyExecuting)} for command '{CmdToString(cmdId, this)}'");
            }

            return;
        }

        this.isExecuting = true;

        try {
            this.ExecutingChanged?.Invoke(this);
        }
        catch {
            AppLogger.Instance.WriteLine($"Exception raising {nameof(this.ExecutingChanged)} for command '{CmdToString(cmdId, this)}'");
        }

        Task task;
        try {
            task = this.ExecuteCommandAsync(args) ?? Task.CompletedTask;
        }
        catch (Exception e) when (!Debugger.IsAttached) {
            try {
                await this.OnExecutionException(args, e);
            }
            catch {
                AppLogger.Instance.WriteLine($"Exception invoking {nameof(this.OnExecutionException)} for command '{CmdToString(cmdId, this)}'");
            }

            task = Task.CompletedTask;
        }

        if (!task.IsCompleted) {
            if (!this.AllowMultipleExecutions) {
                this.myTask = task;
            }

            try {
                await task;
            }
            catch (Exception e) when (!Debugger.IsAttached) {
                try {
                    await this.OnExecutionException(args, e);
                }
                catch {
                    AppLogger.Instance.WriteLine($"Exception invoking {nameof(this.OnExecutionException)} for command '{CmdToString(cmdId, this)}'");
                }
            }
        }

        this.isExecuting = false;
        if (!this.AllowMultipleExecutions) {
            this.myTask = null;
        }

        try {
            this.ExecutingChanged?.Invoke(this);
        }
        catch {
            AppLogger.Instance.WriteLine($"Exception raising {nameof(this.ExecutingChanged)} for command '{CmdToString(cmdId, this)}'");
        }
    }

    private static string CmdToString(string? cmdId, Command cmd) {
        if (cmdId != null && !string.IsNullOrWhiteSpace(cmdId)) {
            return $"{cmdId} ({cmd.GetType()})";
        }
        else {
            return cmd.GetType().ToString();
        }
    }

    /// <summary>
    /// Invoked when this command is already running, but <see cref="ExecuteCommand_NotAsync"/> was called again.
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