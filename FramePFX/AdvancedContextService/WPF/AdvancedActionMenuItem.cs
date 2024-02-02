using System;
using System.Windows;
using System.Windows.Threading;
using FramePFX.Actions;
using FramePFX.Interactivity.DataContexts;
using FramePFX.Shortcuts.WPF.Converters;

namespace FramePFX.AdvancedContextService.WPF {
    public class AdvancedActionMenuItem : AdvancedMenuItem {
        public bool IsExecuting { get; private set; }

        private bool canExecute;

        protected bool CanExecute {
            get => this.canExecute;
            set {
                this.canExecute = value;

                // Causes IsEnableCore to be fetched, which returns false if we are executing something or
                // we have no valid action, causing this menu item to be "disabled"
                this.CoerceValue(IsEnabledProperty);
            }
        }

        public new ActionContextEntry Entry => (ActionContextEntry) base.Entry;

        protected override bool IsEnabledCore => base.IsEnabledCore && this.CanExecute;

        public AdvancedActionMenuItem() {
            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.UpdateCanExecuteVisual();
            ActionContextEntry entry = this.Entry;
            if (entry == null) {
                return;
            }

            AnAction action = ActionManager.Instance.GetAction(entry.ActionId);
            if (action != null) {
                if (ActionIdToGestureConverter.ActionIdToGesture(entry.ActionId, null, out string value)) {
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
                AdvancedContextMenu parent = this.Menu;
                DataContext context = parent?.ContextOnMenuOpen;
                string id = this.Entry.ActionId;
                this.CanExecute = context != null && !string.IsNullOrWhiteSpace(id) && ActionManager.Instance.CanExecute(id, context);
            }
        }

        protected override void OnClick() {
            // Originally used a binding to bind this menu item's command to an ActionContextEntry's
            // internal command, but you lose the ability to access Keyboard.FocusedElement, so it's
            // better to just handle the click manually
            // context should not be an instance of CommandContextEntry... but just in case
            // if (this.DataContext is CommandContextEntry || this.DataContext is ActionContextEntry) {
            //     base.OnClick(); // clicking is handled in the entry
            //     return;
            // }

            if (this.IsExecuting) {
                this.CanExecute = false;
                return;
            }

            this.IsExecuting = true;
            string id = this.Entry.ActionId;
            if (string.IsNullOrWhiteSpace(id)) {
                base.OnClick();
                this.IsExecuting = false;
                this.UpdateCanExecuteVisual();
                return;
            }

            // disable execution while executing action
            this.CanExecute = false;
            base.OnClick();
            this.DispatchAction(id);
        }

        private void DispatchAction(string id) {
            AdvancedContextMenu parent = this.Menu;
            DataContext context = parent?.ContextOnMenuOpen;
            if (context == null) {
                return;
            }

            this.Dispatcher.BeginInvoke((Action) (() => this.ExecuteAction(id, context)), DispatcherPriority.Render);
        }

        private async void ExecuteAction(string id, DataContext context) {
            try {
                if (!string.IsNullOrWhiteSpace(id) && context != null)
                    await ActionManager.Instance.Execute(id, context);
            }
#if !DEBUG
            catch (Exception e) {
                IoC.MessageService.ShowMessage(
                    "Error",
                    "An unexpected error occurred while processing action. " +
                    "FramePFX may or may not crash now, but you should probably restart and save just in case",
                    e.GetToString());
            }
#endif
            finally {
                this.IsExecuting = false;
                this.UpdateCanExecuteVisual();
            }
        }
    }
}