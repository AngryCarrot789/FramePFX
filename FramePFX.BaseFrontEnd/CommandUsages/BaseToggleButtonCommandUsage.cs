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

using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.BaseFrontEnd.CommandUsages;

/// <summary>
/// A base usage class for a command that is called by a toggle button, where the command is expected to
/// update some sort of model boolean state that the toggle button should reflect
/// </summary>
public abstract class BaseToggleButtonCommandUsage : CommandUsage {
    private bool ignoreCheckChanged;

    protected BaseToggleButtonCommandUsage(string commandId) : base(commandId) {
    }

    protected override void OnConnected() {
        base.OnConnected();
        if (!(this.Control is ToggleButton btn))
            throw new InvalidOperationException("Cannot connect to non-toggle-button");
        btn.Checked += this.OnCheckChanged;
        btn.Unchecked += this.OnCheckChanged;
    }

    protected override void OnDisconnected() {
        base.OnDisconnected();
        if (!(this.Control is ToggleButton btn))
            throw new InvalidOperationException("Fatal error");
        btn.Checked -= this.OnCheckChanged;
        btn.Unchecked -= this.OnCheckChanged;
    }

    protected override void OnUpdateForCanExecuteState(Executability state) {
        ((ToggleButton) this.Control).IsEnabled = state == Executability.Valid;
    }

    private void OnCheckChanged(object sender, RoutedEventArgs e) {
        if (this.ignoreCheckChanged || !this.IsConnected) {
            return;
        }

        this.UpdateCanExecute();
        CommandManager.Instance.TryExecute(this.CommandId, () => this.GetContextData() ?? EmptyContext.Instance);

        // We update after running, just in case the command is async which affects the CanExecute method
        this.UpdateIsChecked();
        this.UpdateCanExecute();
    }

    /// <summary>
    /// Updates the <see cref="ToggleButton.IsChecked"/> property based on what <see cref="GetRealIsChecked"/> returns
    /// </summary>
    public void UpdateIsChecked() {
        this.ignoreCheckChanged = true;
        bool isChecked = this.GetRealIsChecked();
        ToggleButton btn = (ToggleButton) this.Control;

        if (isChecked != btn.IsChecked) {
            btn.IsChecked = isChecked;
        }

        this.ignoreCheckChanged = false;
    }

    /// <summary>
    /// Calls <see cref="UpdateIsChecked"/> and <see cref="CommandUsage.UpdateCanExecuteLater"/>
    /// </summary>
    public void UpdateIsCheckedAndCanExecute() {
        this.UpdateIsChecked();
        this.UpdateCanExecuteLater();
    }

    /// <summary>
    /// Gets the actual read/live checked state that the toggle button's state should be at.
    /// The returned value might be different from the toggle button's actual check state,
    /// in which case, the toggle button should be updated with the this method's return value
    /// </summary>
    /// <returns></returns>
    public abstract bool GetRealIsChecked();
}