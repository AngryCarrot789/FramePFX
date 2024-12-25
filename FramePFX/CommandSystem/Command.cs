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
/// A class that represents something that can be executed. Commands are given contextual
/// information (see <see cref="CommandEventArgs.ContextData"/>) to do work.
/// Commands do their work in the <see cref="Execute"/> method, and can optionally specify
/// their executability via the <see cref="CanExecute"/> method
/// <para>
/// Commands are used primarily by the shortcut and advanced menu service to do
/// work, but they can also be used by things like buttons
/// </para>
/// <para>
/// These commands can be executed through the <see cref="CommandManager.Execute(string,FramePFX.CommandSystem.Command,FramePFX.Interactivity.Contexts.IContextData,bool)"/> function
/// </para>
/// </summary>
public abstract class Command {
    protected Command() { }

    // When focus changes, raise notification to update commands
    // Then fire ContextDataChanged for those command hooks or whatever, they can then disconnect
    // old event handlers and attach new ones

    /// <summary>
    /// Gets this command's executability state based on the given command event args context.
    /// This typically isn't checked before <see cref="Execute"/> is invoked, but instead is
    /// mainly used by the UI to determine if something like a button or menu item is actually clickable
    /// <para>
    /// This method should be quick to execute, as it may be called quite often
    /// </para>
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    /// <returns>
    /// True if executing this command would most likely result in success, otherwise false
    /// </returns>
    public virtual Executability CanExecute(CommandEventArgs e) => Executability.Valid;

    /// <summary>
    /// Executes this command with the given command event args. This is always called on the main application thread (AMT)
    /// </summary>
    /// <param name="e">The command event args, containing info about the current context</param>
    protected abstract void Execute(CommandEventArgs e);

    internal static void InternalExecute(string cmdId, Command command, CommandEventArgs e) {
        Application.Instance.Dispatcher.VerifyAccess();
        if (e.IsUserInitiated) {
            try {
                command.Execute(e);
            }
            catch (Exception ex) when (!Debugger.IsAttached) {
                IMessageDialogService.Instance.ShowMessage("Command execution exception", $"An exception occurred while executing '{CmdToString(cmdId, command)}'", ex.GetToString());
            }
        }
        else {
            command.Execute(e);
        }
    }

    private static string CmdToString(string cmdId, Command cmd) {
        if (cmdId != null && !string.IsNullOrWhiteSpace(cmdId)) {
            return $"{cmdId} ({cmd.GetType()})";
        }
        else {
            return cmd.GetType().ToString();
        }
    }
}