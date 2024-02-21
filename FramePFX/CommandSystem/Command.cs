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

using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem {
    /// <summary>
    /// Represents some sort of action that can be executed. Commands use provided contextual
    /// information (see <see cref="CommandEventArgs.ContextData"/>) to do work. Commands do
    /// their work in the <see cref="Execute"/> method, and can optionally specify their
    /// executability via the <see cref="CanExecute"/> method
    /// <para>
    /// Commands are the primary things used by the shortcut system to do some work. They
    /// can also be used by things like context menus
    /// </para>
    /// <para>
    /// These commands can be executed through the <see cref="CommandManager.Execute(string, Command, IContextData, bool)"/> function
    /// </para>
    /// </summary>
    public abstract class Command {
        protected Command() {
        }

        // When focus changes, raise notification to update commands
        // Then fire ContextDataChanged for those command hooks or whatever, they can then disconnect
        // old event handlers and attach new ones

        public virtual void UpdateUsage(CommandUsage usage, CommandEventArgs e) {

        }

        /// <summary>
        /// Get the command context Checks if this command can actually be executed. This typically isn't checked before
        /// <see cref="Execute"/> is invoked; this is mainly used by the UI to determine if
        /// something like a button or menu item is actually clickable
        /// <para>
        /// This method should be quick to execute, as it may be called quite often
        /// </para>
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        /// <returns>
        /// True if executing this command would most likely result in success, otherwise false
        /// </returns>
        public virtual ExecutabilityState CanExecute(CommandEventArgs e) {
            return ExecutabilityState.Executable;
        }

        /// <summary>
        /// Executes this specific command with the given command event args
        /// </summary>
        /// <param name="e">The command event args, containing info about the current context</param>
        public abstract void Execute(CommandEventArgs e);
    }
}