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

using FramePFX.Interactivity.DataContexts;

namespace FramePFX.CommandSystem {
    /// <summary>
    /// A class that represents the state of a single instance of a command being used. This manages its own executability state and handles application focus change
    /// </summary>
    public abstract class CommandUsageContext {
        /// <summary>
        /// Gets the command that this usage is linked to
        /// </summary>
        public Command Command => CommandManager.Instance.GetCommandById(this.CommandId);

        public string CommandId { get; private set; }

        protected CommandUsageContext() {
        }

        protected virtual void OnAttached() {
        }

        protected virtual void OnDetatched() {
        }

        /// <summary>
        /// Called when the application's focus changes
        /// </summary>
        public virtual void OnFocusChanged(IDataContext newFocus) {
            CommandManager.Instance.UpdateForFocusChange(this, newFocus);
        }

        /// <summary>
        /// Called when the executability state of this usage's command may have changed
        /// </summary>
        /// <param name="context">The context to use to evaluate the executability</param>
        public virtual void OnCanExecuteInvalidated(IDataContext context) {

        }

        internal static void OnRegisteredInternal(CommandUsageContext usage, string cmdId) {
            usage.CommandId = cmdId;
            usage.OnAttached();
        }

        internal static void OnUnregisteredInternal(CommandUsageContext usage) {
            if (usage.CommandId == null)
                return;
            try {
                usage.OnDetatched();
            }
            finally {
                usage.CommandId = null;
            }
        }
    }
}