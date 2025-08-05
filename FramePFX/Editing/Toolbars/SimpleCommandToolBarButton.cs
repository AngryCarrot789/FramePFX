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

using PFXToolKitUI.CommandSystem;
using PFXToolKitUI.Toolbars;

namespace FramePFX.Editing.Toolbars;

public class SimpleCommandToolBarButton : ToolBarButton {
    /// <summary>
    /// Gets the command ID that this button executes
    /// </summary>
    public string CommandId { get; }

    public SimpleCommandToolBarButton(string commandId, IButtonElement button) : base(button) {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(commandId);
        this.CommandId = commandId;
    }

    public override Executability CanExecute() {
        return CommandManager.Instance.CanExecute(this.CommandId, this.ContextData, true);
    }

    protected override async Task OnClickedAsync() {
        if (CommandManager.Instance.TryFindCommandById(this.CommandId, out Command? command)) {
            await CommandManager.Instance.Execute(command, this.ContextData, true);
        }
    }
}