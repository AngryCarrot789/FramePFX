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

using System;
using System.Windows.Input;
using Avalonia.Input;
using FramePFX.Avalonia.CommandSystem.Usages;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;

namespace FramePFX.Avalonia.Editing.CommandContexts;

/// <summary>
/// A command usage for a <see cref="ICommandSource"/> control and that uses an <see cref="ICommand"/> to execute the underlying command
/// </summary>
public class CommandSourceCommandUsage : CommandUsage
{
    private CoreCommandICommand? command;
    private ButtonHelper? button;

    public ICommand Command => this.command ??= new CoreCommandICommand(this);

    public CommandSourceCommandUsage(string commandId) : base(commandId) { }

    protected override void OnConnected()
    {
        base.OnConnected();
        if (!(this.Control is ICommandSource))
            throw new InvalidOperationException("Cannot connect to non-ICommandSource");

        this.button = ButtonHelper.Create(this.Control);
        this.button.Command = this.Command;
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        this.button?.Dispose();
        this.button = null;
    }

    protected override void UpdateCanExecute() => this.command?.RaiseCanExecuteChanged();

    private class CoreCommandICommand : ICommand
    {
        private readonly CommandSourceCommandUsage usage;

        public event EventHandler? CanExecuteChanged;

        public CoreCommandICommand(CommandSourceCommandUsage usage)
        {
            this.usage = usage;
        }

        public bool CanExecute(object? parameter)
        {
            if (!this.usage.IsConnected)
                return false;

            return CommandManager.Instance.CanExecute(this.usage.CommandId, this.usage.GetContextData()!) == Executability.Valid;
        }

        public void Execute(object? parameter)
        {
            CommandManager.Instance.TryExecute(this.usage.CommandId, () => this.usage.GetContextData() ?? EmptyContext.Instance);
        }

        public void RaiseCanExecuteChanged()
        {
            this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}