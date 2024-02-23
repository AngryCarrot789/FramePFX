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
using FramePFX.Interactivity.Contexts;

namespace FramePFX.CommandSystem.Usages {
    /// <summary>
    /// A command usage is a ui-place-specific usage of a command, e.g. a push or toggle button, a menu or context item.
    /// These accept a connected <see cref="DependencyObject"/>, in which events can be attached and detached in order to
    /// things like execute the command.
    /// <para>
    /// This class automatically listens for contextual data changes, which triggers the
    /// executability state to be re-queried from the command based on the new contextual data
    /// </para>
    /// </summary>
    public abstract class CommandUsage {
        public string CommandId { get; }

        public DependencyObject Control { get; private set; }

        protected CommandUsage(string commandId) {
            if (commandId == null)
                throw new Exception(nameof(commandId) + " cannot return null");
            if (string.IsNullOrWhiteSpace(commandId))
                throw new Exception(nameof(commandId) + " cannot return an empty string or consist of only whitespaces");
            this.CommandId = commandId;
        }

        /// <summary>
        /// Gets the current available context for our connected control. Returns null if disconnected
        /// </summary>
        /// <returns></returns>
        public IContextData GetContextData() {
            return this.Control != null ? DataManager.GetFullContextData(this.Control) : null;
        }

        public void Connect(DependencyObject control) {
            this.Control = control ?? throw new ArgumentNullException(nameof(control));
            DataManager.AddMergedContextInvalidatedHandler(control, this.OnInheritedContextChanged);
            this.OnConnected();
        }

        public void Disconnect() {
            DataManager.RemoveMergedContextInvalidatedHandler(this.Control, this.OnInheritedContextChanged);
            this.OnDisconnected();
            this.Control = null;
        }

        private void OnInheritedContextChanged(object sender, RoutedEventArgs e) {
            this.UpdateForContext(this.GetContextData());
        }

        protected virtual void OnConnected() {
            this.UpdateForContext(this.GetContextData());
        }

        protected virtual void OnDisconnected() {
            this.UpdateForContext(null);
        }

        protected virtual void UpdateForContext(IContextData context) {
            this.UpdateCanExecute(context);
        }

        protected void UpdateCanExecute() {
            this.UpdateCanExecute(this.GetContextData());
        }

        protected void UpdateCanExecute(IContextData context) {
            this.OnCanExecuteStateAvailable(context != null ? CommandManager.Instance.CanExecute(this.CommandId, context) : ExecutabilityState.Invalid);
        }

        protected virtual void OnCanExecuteStateAvailable(ExecutabilityState state) {

        }
    }
}