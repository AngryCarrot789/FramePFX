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
        /// Evaluates the contextual data for our <see cref="Control"/>. Returns null if disconnected
        /// </summary>
        /// <returns></returns>
        public IContextData EvaluateContextData() {
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
            this.UpdateForContext(this.EvaluateContextData());
        }

        protected virtual void OnConnected() {
            this.UpdateForContext(this.EvaluateContextData());
        }

        protected virtual void OnDisconnected() {
            this.UpdateForContext(null);
        }

        protected virtual void UpdateForContext(IContextData context) {
            this.UpdateCanExecute(context);
        }

        protected void UpdateCanExecute() {
            this.UpdateCanExecute(this.EvaluateContextData());
        }

        protected void UpdateCanExecute(IContextData context) {
            this.OnCanExecuteStateAvailable(context != null ? CommandManager.Instance.CanExecute(this.CommandId, context) : ExecutabilityState.Invalid);
        }

        protected virtual void OnCanExecuteStateAvailable(ExecutabilityState state) {

        }
    }
}