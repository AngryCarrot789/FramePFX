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

using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Interactivity;
using FramePFX.CommandSystem;

namespace FramePFX.Avalonia.CommandSystem.Usages;

public class BasicButtonCommandUsage : CommandUsage {
    public BasicButtonCommandUsage(string commandId) : base(commandId) { }

    protected override void OnConnected() {
        base.OnConnected();
        if (!(this.Control is Button))
            throw new InvalidOperationException("Cannot connect to non-button");
        ((Button) this.Control).Click += this.OnButtonClick;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        ((Button) this.Control!).Click -= this.OnButtonClick;
    }

    protected virtual void OnButtonClick(object? sender, RoutedEventArgs e) {
        this.UpdateCanExecute();
        CommandManager.Instance.TryExecute(this.CommandId, () => DataManager.GetFullContextData((Button) sender!));

        // We update after running, just in case the command is async which affects the CanExecute method
        this.UpdateCanExecute();
    }

    protected override void OnUpdateForCanExecuteState(Executability state) {
        ((Button) this.Control!).IsEnabled = state == Executability.Valid;
    }
}