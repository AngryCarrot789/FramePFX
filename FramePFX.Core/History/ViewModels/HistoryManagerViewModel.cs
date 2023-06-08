using System;
using System.Threading.Tasks;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.History.ViewModels {
    public class HistoryManagerViewModel : BaseViewModel {
        public HistoryManager Model { get; }

        public AsyncRelayCommand UndoCommand { get; }
        public AsyncRelayCommand RedoCommand { get; }
        public AsyncRelayCommand ClearCommand { get; }
        public AsyncRelayCommand EditMaxUndoCommand { get; }
        public AsyncRelayCommand EditMaxRedoCommand { get; }

        public int MaxUndo => this.Model.MaxUndo;
        public int MaxRedo => this.Model.MaxRedo;

        public bool CanUndo => this.Model.CanUndo;
        public bool CanRedo => this.Model.CanRedo;

        public HistoryManagerViewModel(HistoryManager model) {
            this.Model = model ?? throw new ArgumentNullException();
            this.UndoCommand = new AsyncRelayCommand(this.UndoAction, () => !this.Model.IsActionActive && this.Model.CanUndo);
            this.RedoCommand = new AsyncRelayCommand(this.RedoAction, () => !this.Model.IsActionActive && this.Model.CanRedo);
            this.ClearCommand = new AsyncRelayCommand(this.ClearAction, () => !this.Model.IsActionActive && (this.Model.CanRedo || this.Model.CanUndo));
            this.EditMaxUndoCommand = new AsyncRelayCommand(this.SetMaxUndoAction, () => !this.Model.IsActionActive);
            this.EditMaxRedoCommand = new AsyncRelayCommand(this.SetMaxRedoAction, () => !this.Model.IsActionActive);
        }

        public async Task SetMaxUndoAction() {
            if (await this.IsActionActive("Cannot set maximum undo count")) {
                return;
            }

            if (await this.GetInput("Set max undo", "Input a new maximum undo count:", this.Model.MaxUndo) is int value) {
                this.Model.SetMaxUndoAsync(value);
                this.RaisePropertyChanged(nameof(this.MaxUndo));
            }
        }

        public async Task SetMaxRedoAction() {
            if (await this.IsActionActive("Cannot set maximum redo count")) {
                return;
            }

            if (await this.GetInput("Set max redo", "Input a new maximum redo count:", this.Model.MaxRedo) is int value) {
                this.Model.SetMaxRedo(value);
                this.RaisePropertyChanged(nameof(this.MaxRedo));
            }
        }

        public HistoryActionViewModel AddAction(IHistoryAction action, string information = null) {
            HistoryActionModel model = this.Model.AddAction(action ?? throw new ArgumentNullException(nameof(action)));
            return new HistoryActionViewModel(this, model, action, information);
        }

        public async Task UndoAction() {
            if (await this.IsActionActive("Cannot perform undo")) {
                return;
            }

            if (this.CanUndo) {
                await this.Model.OnUndoAsync();
                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
            }
        }

        public async Task RedoAction() {
            if (await this.IsActionActive("Cannot perform redo")) {
                return;
            }

            if (this.CanRedo) {
                await this.Model.OnRedoAsync();
                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
            }
        }

        public async Task ClearAction() {
            if (await this.IsActionActive("Cannot clear actions")) {
                return;
            }

            this.Model.Clear();
        }

        private async Task<bool> IsActionActive(string message) {
            if (this.Model.IsUndoing) {
                await IoC.MessageDialogs.ShowMessageAsync("Undo active", message + ". Undo is in progress");
            }
            else if (this.Model.IsRedoing) {
                await IoC.MessageDialogs.ShowMessageAsync("Redo active", message + ". Redo is in progress");
            }
            else {
                return false;
            }

            return true;
        }

        private async Task<int?> GetInput(string caption, string message, int def = 1) {
            InputValidator validator = InputValidator.FromFunc((x) => {
                if (int.TryParse(x, out int val))
                    return val > 0 ? null : "Value must be above 0";
                return "Value is not an integer";
            });

            string value = await IoC.UserInput.ShowSingleInputDialogAsync(caption, message, def.ToString(), validator);
            if (value == null) {
                return null;
            }

            if (int.TryParse(value, out int integer)) {
                if (integer < 1) {
                    await IoC.MessageDialogs.ShowMessageAsync("Invalid value", "Value must be more than 0");
                    return null;
                }

                return integer;
            }

            await IoC.MessageDialogs.ShowMessageAsync("Invalid value", "Value is not an integer: " + value);
            return null;
        }
    }
}