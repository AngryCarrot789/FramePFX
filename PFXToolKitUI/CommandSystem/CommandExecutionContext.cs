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

using PFXToolKitUI.Interactivity.Contexts;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.CommandSystem;

/// <summary>
/// This class is a helper for executing commands when awaiting is not necessarily possible
/// </summary>
public class CommandExecutionContext {
    private List<Delegate>? onCompleted;

    public CommandManager CommandManager { get; }

    public Command Command { get; }

    public string CommandId { get; }

    public IContextData ContextData { get; }

    public bool IsUserInitiated { get; }

    public CommandExecutionContext(string commandId, Command command, CommandManager manager, IContextData context, bool isUserInitiated) {
        Validate.NotNullOrWhiteSpaces(commandId);
        Validate.NotNull(command);
        Validate.NotNull(manager);
        this.CommandManager = manager;
        this.Command = command;
        this.CommandId = commandId;
        this.ContextData = context;
        this.IsUserInitiated = isUserInitiated;
    }

    public void AddCompletionAction(Action completionAction) {
        (this.onCompleted ??= new List<Delegate>()).Add(completionAction);
    }

    public async void Execute() {
        try {
            await this.CommandManager.Execute(this.CommandId, this.Command, this.ContextData, this.IsUserInitiated);
        }
        catch (Exception e) {
            Application.Instance.Dispatcher.Post(() => throw e);
        }

        if (this.onCompleted != null) {
            using ErrorList list = new ErrorList("One or more errors occurred while running command completion callback", false);
            foreach (Delegate action in this.onCompleted) {
                try {
                    ((Action) action)();
                }
                catch (Exception e) {
                    list.Add(e);
                }
            }

            this.onCompleted = null;

            if (list.TryGetException(out Exception? exception)) {
                Application.Instance.Dispatcher.Post(() => throw exception);
            }
        }
    }
}