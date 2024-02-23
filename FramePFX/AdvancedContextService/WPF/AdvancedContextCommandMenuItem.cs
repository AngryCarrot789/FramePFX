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

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using FramePFX.CommandSystem;
using FramePFX.Interactivity.Contexts;
using FramePFX.Shortcuts.WPF.Converters;
using FramePFX.Utils;

namespace FramePFX.AdvancedContextService.WPF {
    public class AdvancedContextCommandMenuItem : AdvancedContextMenuItem {
        public bool IsExecuting { get; private set; }

        private bool canExecute;

        protected bool CanExecute {
            get => this.canExecute;
            set {
                this.canExecute = value;

                // Causes IsEnableCore to be fetched, which returns false if we are executing something or
                // we have no valid command, causing this menu item to be "disabled"
                this.CoerceValue(IsEnabledProperty);
            }
        }

        public new CommandContextEntry Entry => (CommandContextEntry) base.Entry;

        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        public AdvancedContextCommandMenuItem() {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.UpdateCanExecuteVisual();
            CommandContextEntry entry = this.Entry;
            if (entry == null) {
                return;
            }

            Command cmd = CommandManager.Instance.GetCommandById(entry.CommandId);
            if (cmd != null) {
                if (CommandIdToGestureConverter.CommandIdToGesture(entry.CommandId, null, out string value)) {
                    this.SetCurrentValue(InputGestureTextProperty, value);
                }
            }
        }

        public void UpdateCanExecuteVisual() {
            this.UpdateCanExecuteVisual(this.Menu?.ContextOnMenuOpen);
        }

        public void UpdateCanExecuteVisual(IContextData ctx) {
            if (!this.IsLoaded) {
                return;
            }

            if (this.IsExecuting) {
                this.CanExecute = false;
            }
            else {
                string cmdId = this.Entry.CommandId;
                ExecutabilityState state = !string.IsNullOrWhiteSpace(cmdId) ? CommandManager.Instance.CanExecute(cmdId, ctx, true) : ExecutabilityState.Invalid;
                this.CanExecute = state == ExecutabilityState.Executable;
            }
        }

        protected override void OnClick() {
            if (this.IsExecuting) {
                this.CanExecute = false;
                return;
            }

            this.IsExecuting = true;
            string id = this.Entry.CommandId;
            if (string.IsNullOrWhiteSpace(id)) {
                base.OnClick();
                this.IsExecuting = false;
                this.UpdateCanExecuteVisual();
                return;
            }

            // disable execution while executing command
            this.CanExecute = false;
            base.OnClick();
            this.DispatchCommand(id);
        }

        private void DispatchCommand(string cmdId) {
            IContextData context = this.Menu?.ContextOnMenuOpen;
            if (context != null) {
                this.Dispatcher.BeginInvoke((Action) (() => this.ExecuteCommand(cmdId, context)), DispatcherPriority.Render);
            }
        }

        private void ExecuteCommand(string cmdId, IContextData context) {
            try {
                if (!string.IsNullOrWhiteSpace(cmdId) && context != null)
                    CommandManager.Instance.Execute(cmdId, context);
            }
            catch (Exception e) {
                if (!Debugger.IsAttached) {
                    IoC.MessageService.ShowMessage(
                        "Error",
                        "An unexpected error occurred while processing command. " +
                        "FramePFX may or may not crash now, but you should probably restart and save just in case",
                        e.GetToString());
                }
            }
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecuteVisual();
            }
        }
    }
}