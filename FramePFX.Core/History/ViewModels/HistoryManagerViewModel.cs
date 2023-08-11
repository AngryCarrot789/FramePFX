using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.Notifications;
using FramePFX.Core.Views.Dialogs.Progression;
using FramePFX.Core.Views.Dialogs.UserInputs;

namespace FramePFX.Core.History.ViewModels {
    public class HistoryManagerViewModel : BaseViewModel, IHistoryManager {
        // Despite the fact there is a separation between history manager and the view model,
        // the history manager itself really should not be modified outside of the view model's control,
        // as is the case with most viewmodel-model combos. This is to maintain the state of the view model (obviously)

        private readonly HistoryManager manager;

        public AsyncRelayCommand UndoCommand { get; }
        public AsyncRelayCommand RedoCommand { get; }
        public AsyncRelayCommand ClearCommand { get; }
        public AsyncRelayCommand EditMaxUndoCommand { get; }
        public AsyncRelayCommand EditMaxRedoCommand { get; }

        public int MaxUndo => this.manager.MaxUndo;
        public int MaxRedo => this.manager.MaxRedo;
        public bool CanUndo => this.manager.CanUndo;
        public bool CanRedo => this.manager.CanRedo;

        private HistoryNotification notification;

        private HistoryNotification Notification {
            get {
                if (this.notification != null && !this.notification.IsHidden)
                    return this.notification;
                if (this.notification == null)
                    this.notification = new HistoryNotification();
                this.NotificationPanel.PushNotification(this.notification, false);
                return this.notification;
            }
        }

        public NotificationPanelViewModel NotificationPanel { get; }

        private readonly List<IHistoryAction> mergeList;
        private int mergeListCount;

        public HistoryManagerViewModel(NotificationPanelViewModel panel, HistoryManager model) {
            this.manager = model ?? throw new ArgumentNullException(nameof(model));
            this.NotificationPanel = panel ?? throw new ArgumentNullException(nameof(panel));
            this.UndoCommand = new AsyncRelayCommand(this.UndoAction, () => !this.manager.IsActionActive && this.manager.CanUndo);
            this.RedoCommand = new AsyncRelayCommand(this.RedoAction, () => !this.manager.IsActionActive && this.manager.CanRedo);
            this.ClearCommand = new AsyncRelayCommand(this.ClearAction, () => !this.manager.IsActionActive && (this.manager.CanRedo || this.manager.CanUndo));
            this.EditMaxUndoCommand = new AsyncRelayCommand(this.SetMaxUndoAction, () => !this.manager.IsActionActive);
            this.EditMaxRedoCommand = new AsyncRelayCommand(this.SetMaxRedoAction, () => !this.manager.IsActionActive);
            this.mergeList = new List<IHistoryAction>();
        }

        private class MergeContext : IDisposable {
            private readonly HistoryManagerViewModel manager;
            private bool isDisposed;

            public MergeContext(HistoryManagerViewModel manager) {
                this.manager = manager;
                this.manager.mergeListCount++;
            }

            public void Dispose() {
                if (this.isDisposed) {
                    throw new ObjectDisposedException(nameof(IDisposable));
                }

                this.isDisposed = true;
                if (--this.manager.mergeListCount == 0) {
                    int count = this.manager.mergeList.Count;
                    if (count > 1) {
                        this.manager.AddAction(new MultiHistoryAction(new List<IHistoryAction>(this.manager.mergeList)), "Multi action");
                    }
                    else if (count == 1) {
                        this.manager.AddAction(this.manager.mergeList[0], "Multi action");
                    }

                    this.manager.mergeList.Clear();
                }
            }
        }

        /// <summary>
        /// Sets up the history manager for merging multiple actions into a single action
        /// </summary>
        /// <returns></returns>
        public IDisposable OpenMerge() => new MergeContext(this);

        public async Task SetMaxUndoAction() {
            if (await this.IsActionActive("Cannot set maximum undo count")) {
                return;
            }

            if (await this.GetInput("Set max undo", "Input a new maximum undo count:", this.manager.MaxUndo) is int value) {
                this.manager.SetMaxUndoAsync(value);
                this.RaisePropertyChanged(nameof(this.MaxUndo));
            }
        }

        public async Task SetMaxRedoAction() {
            if (await this.IsActionActive("Cannot set maximum redo count")) {
                return;
            }

            if (await this.GetInput("Set max redo", "Input a new maximum redo count:", this.manager.MaxRedo) is int value) {
                this.manager.SetMaxRedo(value);
                this.RaisePropertyChanged(nameof(this.MaxRedo));
            }
        }

        public void AddAction(IHistoryAction action, string information = null) {
            if (this.mergeListCount == 0) {
                this.manager.AddAction(action ?? throw new ArgumentNullException(nameof(action)));
            }
            else {
                this.mergeList.Add(action);
            }
        }

        public async Task UndoAction() {
            if (await this.IsActionActive("Cannot perform undo")) {
                return;
            }

            if (this.CanUndo) {
                await this.manager.OnUndoAsync();
                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
                this.Notification.OnUndo();
            }
        }

        public async Task RedoAction() {
            if (await this.IsActionActive("Cannot perform redo")) {
                return;
            }

            if (this.CanRedo) {
                await this.manager.OnRedoAsync();
                this.RaisePropertyChanged(nameof(this.CanUndo));
                this.RaisePropertyChanged(nameof(this.CanRedo));
                this.Notification.OnRedo();
            }
        }

        public async Task ClearAction() {
            if (await this.IsActionActive("Cannot clear actions")) {
                return;
            }

            this.manager.Clear();
            this.Notification.OnUndo();
        }

        private async Task<bool> IsActionActive(string message) {
            if (this.manager.IsUndoing) {
                await IoC.MessageDialogs.ShowMessageAsync("Undo active", message + ". Undo is in progress");
            }
            else if (this.manager.IsRedoing) {
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

        public async Task ResetAsync() {
            if (this.manager.IsUndoing || this.manager.IsRedoing) {
                IndeterminateProgressViewModel progress = new IndeterminateProgressViewModel(true) {
                    Message = "Waiting for a history action to complete...",
                    Titlebar = "Clearing history"
                };

                await IoC.ProgressionDialogs.ShowIndeterminateAsync(progress);
                do {
                    await Task.Delay(250);
                    if (progress.IsCancelled) {
                        this.manager.UnsafeReset();
                        goto end;
                    }
                } while (this.manager.IsUndoing || this.manager.IsRedoing);
            }

            this.manager.Reset();

            end:
            this.RaisePropertyChanged(nameof(this.MaxUndo));
            this.RaisePropertyChanged(nameof(this.MaxRedo));
            this.RaisePropertyChanged(nameof(this.CanUndo));
            this.RaisePropertyChanged(nameof(this.CanRedo));
        }
    }
}