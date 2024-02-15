using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Commands;
using FramePFX.Interactivity.DataContexts;
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
            if (!this.IsLoaded) {
                return;
            }

            if (this.IsExecuting) {
                this.CanExecute = false;
            }
            else {
                DataContext context = this.Menu?.ContextOnMenuOpen;
                string id = this.Entry.CommandId;
                this.CanExecute = context != null && !string.IsNullOrWhiteSpace(id) && CommandManager.Instance.CanExecute(id, context);
            }
        }

        protected override void OnClick() {
            // Originally used a binding to bind this menu item's command to an CommandContextEntry's
            // internal command, but you lose the ability to access Keyboard.FocusedElement, so it's
            // better to just handle the click manually
            // context should not be an instance of CommandContextEntry... but just in case
            // if (this.DataContext is CommandContextEntry || this.DataContext is CommandContextEntry) {
            //     base.OnClick(); // clicking is handled in the entry
            //     return;
            // }

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
            DataContext context = this.Menu?.ContextOnMenuOpen;
            if (context != null) {
                this.Dispatcher.BeginInvoke((Action) (() => this.ExecuteCommand(cmdId, context)), DispatcherPriority.Render);
            }
        }

        private void ExecuteCommand(string cmdId, DataContext context) {
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