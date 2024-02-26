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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using FramePFX.CommandSystem;
using FramePFX.CommandSystem.Usages;
using FramePFX.Interactivity.Contexts;
using CommandManager = FramePFX.CommandSystem.CommandManager;

namespace FramePFX.Editors.Controls.Timelines.CommandContexts {
    /// <summary>
    /// A command usage for a <see cref="ICommandSource"/> control and that uses an <see cref="ICommand"/> to execute the underlying command
    /// </summary>
    public class CommandSourceCommandUsage : CommandUsage {
        private CommandImpl commandImpl;

        public ICommand Command => this.commandImpl ?? (this.commandImpl = new CommandImpl(this));

        public CommandSourceCommandUsage(string commandId) : base(commandId) {
        }

        private static void SetCommand(DependencyObject control, ICommand cmd) {
            switch (control) {
                case ButtonBase btnBase:
                    btnBase.Command = cmd;
                    break;
                case MenuItem menuItem:
                    menuItem.Command = cmd;
                    break;
                case Hyperlink hyperlink:
                    hyperlink.Command = cmd;
                    break;
                default: throw new InvalidOperationException("Invalid control");
            }
        }

        protected override void OnConnected() {
            base.OnConnected();
            if (!(this.Control is ICommandSource))
                throw new InvalidOperationException("Cannot connect to non-ICommandSource");

            SetCommand(this.Control, this.Command);
        }

        protected override void OnDisconnected() {
            base.OnDisconnected();
            SetCommand(this.Control, null);
        }

        protected override void UpdateCanExecute() {
            this.commandImpl?.RaiseCanExecuteChanged();
        }

        private class CommandImpl : ICommand {
            private readonly CommandSourceCommandUsage usage;
            private bool isExecuting;

            public event EventHandler CanExecuteChanged;

            public CommandImpl(CommandSourceCommandUsage usage) {
                this.usage = usage;
            }

            public bool CanExecute(object parameter) {
                if (this.isExecuting) {
                    return false;
                }

                IContextData ctx = this.usage.GetContextData();
                if (ctx == null)
                    return false;
                ExecutabilityState state = CommandManager.Instance.CanExecute(this.usage.CommandId, ctx);
                return state == ExecutabilityState.Executable;
            }

            public async void Execute(object parameter) {
                if (!this.isExecuting) {
                    this.isExecuting = true;
                    try {
                        await CommandManager.Instance.TryExecute(this.usage.CommandId, () => this.usage.GetContextData() ?? EmptyContext.Instance);
                    }
                    finally {
                        this.isExecuting = false;
                    }
                }
            }

            public void RaiseCanExecuteChanged() {
                this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}