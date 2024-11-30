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
using Avalonia;
using Avalonia.Interactivity;
using FramePFX.Avalonia.Interactivity;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils;
using FramePFX.Utils.RDA;

namespace FramePFX.Avalonia.CommandSystem.Usages;

/// <summary>
/// A command usage is a ui-place-specific usage of a command, e.g. a push or toggle button, a menu or context item.
/// These accept a connected <see cref="AvaloniaObject"/>, in which events can be attached and detached in order to
/// things like execute the command.
/// <para>
/// This class automatically listens for contextual data changes, which triggers the
/// executability state to be re-queried from the command based on the new contextual data
/// </para>
/// </summary>
public abstract class CommandUsage {
    // Since its invoke method is only called from the main thread,
    // there's no need for the extended version
    private RapidDispatchAction? delayedContextUpdate;

    /// <summary>
    /// Gets the target command ID for this usage instance. This is not null, not empty and does not consist of whitespaces only
    /// </summary>
    public string CommandId { get; }

    public AvaloniaObject? Control { get; private set; }

    protected CommandUsage(string commandId) {
        Validate.NotNullOrWhiteSpaces(commandId);
        this.CommandId = commandId;
    }

    /// <summary>
    /// Gets the current available context for our connected control. Returns null if disconnected
    /// </summary>
    /// <returns>The context data</returns>
    public IContextData? GetContextData() => this.Control != null ? DataManager.GetFullContextData(this.Control) : null;

    /// <summary>
    /// Connects to the given object control
    /// </summary>
    /// <param name="control">Control to connect to</param>
    /// <exception cref="ArgumentNullException">Control is null</exception>
    public void Connect(AvaloniaObject control) {
        this.Control = control ?? throw new ArgumentNullException(nameof(control));
        DataManager.AddInheritedContextChangedHandler(control, this.OnInheritedContextChanged);
        this.OnConnected();
    }

    /// <summary>
    /// Disconnects from this control
    /// </summary>
    /// <exception cref="InvalidCastException">Not connected</exception>
    public void Disconnect() {
        if (this.Control == null)
            throw new InvalidCastException("Not connected");

        DataManager.RemoveInheritedContextChangedHandler(this.Control, this.OnInheritedContextChanged);
        this.OnDisconnected();
        this.Control = null;
    }

    private void OnInheritedContextChanged(object sender, RoutedEventArgs e) {
        this.OnContextChanged();
    }

    protected virtual void OnConnected() => this.OnContextChanged();

    protected virtual void OnDisconnected() => this.OnContextChanged();

    protected virtual void OnContextChanged() {
        this.UpdateCanExecuteLater();
    }

    /// <summary>
    /// Schedules the <see cref="UpdateCanExecute"/> method to be invoked later. This is called by <see cref="OnContextChanged"/>
    /// </summary>
    public void UpdateCanExecuteLater() {
        RapidDispatchAction guard = this.delayedContextUpdate ??= new RapidDispatchAction(this.UpdateCanExecute, DispatchPriority.Loaded, "UpdateCanExecute");
        guard.InvokeAsync();
    }

    protected virtual void UpdateCanExecute() {
        IContextData? ctx = this.GetContextData();
        this.OnUpdateForCanExecuteState(ctx != null ? CommandManager.Instance.CanExecute(this.CommandId, ctx) : Executability.Invalid);
    }

    protected virtual void OnUpdateForCanExecuteState(Executability state) { }
}