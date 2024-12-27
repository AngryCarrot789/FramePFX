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

using FramePFX.BaseFrontEnd.Interactivity;
using FramePFX.CommandSystem;

namespace FramePFX.BaseFrontEnd.CommandUsages;

public class BasicButtonCommandUsage : CommandUsage {
    private ClickableControlHelper? button;

    public BasicButtonCommandUsage(string commandId) : base(commandId) { }

    protected override void OnConnected() {
        base.OnConnected();
        this.button = ClickableControlHelper.Create(this.Control, this.OnButtonClick);
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        this.button?.Dispose();
        this.button = null;
    }

    protected virtual void OnButtonClick() {
        this.UpdateCanExecute();
        CommandManager.Instance.TryExecute(this.CommandId, () => DataManager.GetFullContextData(this.Control!));

        // If the command does things in the background (e.g. async command),
        // then it may not be executable anymore, so update the UI
        this.UpdateCanExecute();
    }

    protected override void OnUpdateForCanExecuteState(Executability state) {
        this.button!.IsEnabled = state == Executability.Valid;
    }
}