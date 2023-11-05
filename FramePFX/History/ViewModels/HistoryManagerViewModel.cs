using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.Notifications;
using FramePFX.TaskSystem;
using FramePFX.Views.Dialogs.Progression;
using FramePFX.Views.Dialogs.UserInputs;

namespace FramePFX.History.ViewModels {
    public class HistoryManagerViewModel : BaseViewModel, IHistoryManager {
        public static HistoryManagerViewModel Instance { get; } = new HistoryManagerViewModel(new HistoryManager());

        // Despite the fact there is a separation between history manager and the view model,
        // the history manager itself really should not be modified outside of the view model's control,
        // as is the case with most viewmodel-model combos. This is to maintain the state of the view model (obviously)

        private readonly HistoryManager manager;
        private NotificationPanelViewModel notificationPanel;
        private HistoryNotification notification;

        public AsyncRelayCommand UndoCommand { get; }
        public AsyncRelayCommand RedoCommand { get; }
        public AsyncRelayCommand ClearCommand { get; }
        public AsyncRelayCommand EditMaxUndoCommand { get; }
        public AsyncRelayCommand EditMaxRedoCommand { get; }

        public int MaxUndo => this.manager.MaxUndo;
        public int MaxRedo => this.manager.MaxRedo;
        public bool HasUndoActions => this.manager.HasUndoActions;
        public bool HasRedoActions => this.manager.HasRedoActions;

        public bool IsUndoing => this.manager.IsUndoing;
        public bool IsRedoing => this.manager.IsRedoing;
        public bool IsOperationActive => this.manager.IsOperationActive;

        private HistoryNotification Notification {
            get {
                if (this.notificationPanel == null)
                    return null;
                if (this.notification != null && !this.notification.IsHidden)
                    return this.notification;
                if (this.notification == null)
                    this.notification = new HistoryNotification();
                this.notificationPanel.PushNotification(this.notification, false);
                return this.notification;
            }
        }

        /// <summary>
        /// An optional notification panel that can be used to push history notifications
        /// </summary>
        public NotificationPanelViewModel NotificationPanel {
            get => this.notificationPanel;
            set => this.RaisePropertyChanged(ref this.notificationPanel, value);
        }

        private readonly Stack<List<HistoryAction>> mergeList;

        public HistoryManagerViewModel(HistoryManager model) {
            this.manager = model ?? throw new ArgumentNullException(nameof(model));
            this.UndoCommand = new AsyncRelayCommand(this.UndoAction, () => !this.manager.IsOperationActive && this.manager.HasUndoActions);
            this.RedoCommand = new AsyncRelayCommand(this.RedoAction, () => !this.manager.IsOperationActive && this.manager.HasRedoActions);
            this.ClearCommand = new AsyncRelayCommand(this.ClearAction, () => !this.manager.IsOperationActive && (this.manager.HasRedoActions || this.manager.HasUndoActions));
            this.EditMaxUndoCommand = new AsyncRelayCommand(this.SetMaxUndoAction, () => !this.manager.IsOperationActive);
            this.EditMaxRedoCommand = new AsyncRelayCommand(this.SetMaxRedoAction, () => !this.manager.IsOperationActive);
            this.mergeList = new Stack<List<HistoryAction>>();
        }

        private class MergeContext : IDisposable {
            private readonly HistoryManagerViewModel manager;
            private bool isDisposed;

            public MergeContext(HistoryManagerViewModel manager) {
                this.manager = manager;
                manager.mergeList.Push(new List<HistoryAction>());
            }

            public void Dispose() {
                if (this.isDisposed) {
                    return;
                }

                this.isDisposed = true;
                List<HistoryAction> myList = this.manager.mergeList.Pop();
                if (myList.Count > 0) {
                    this.manager.AddAction(myList.Count == 1 ? myList[0] : new MultiHistoryAction(myList.ToList()), "Multi action");
                }
            }
        }

        /// <summary>
        /// Sets up the history manager for merging multiple actions into a single action
        /// </summary>
        /// <returns>
        /// A disposable object which, when disposes, combines all actions (added after
        /// the call to this method) into a single <see cref="MultiHistoryAction"/>
        /// </returns>
        public IDisposable PushMergeContext() => new MergeContext(this);

        public async Task SetMaxUndoAction() {
            if (await this.IsActionActiveHelper("Cannot set maximum undo count")) {
                return;
            }

            if (await this.GetInput("Set max undo", "Input a new maximum undo count:", this.manager.MaxUndo) is int value) {
                this.manager.SetMaxUndoAsync(value);
                this.RaisePropertyChanged(nameof(this.MaxUndo));
            }
        }

        public async Task SetMaxRedoAction() {
            if (await this.IsActionActiveHelper("Cannot set maximum redo count")) {
                return;
            }

            if (await this.GetInput("Set max redo", "Input a new maximum redo count:", this.manager.MaxRedo) is int value) {
                this.manager.SetMaxRedo(value);
                this.RaisePropertyChanged(nameof(this.MaxRedo));
            }
        }

        public void AddAction(HistoryAction action, string information = null) {
            if (this.mergeList.Count < 1) {
                // no more lists in stack; add history to manager
                this.manager.AddAction(action ?? throw new ArgumentNullException(nameof(action)));
            }
            else {
                this.mergeList.Peek().Add(action);
            }
        }

        public async Task UndoAction() {
            if (await this.IsActionActiveHelper("Cannot perform undo")) {
                return;
            }

            if (this.HasUndoActions) {
                await this.manager.OnUndoAsync();
                this.RaisePropertyChanged(nameof(this.HasUndoActions));
                this.RaisePropertyChanged(nameof(this.HasRedoActions));
                this.Notification.OnUndo();
            }
        }

        public async Task RedoAction() {
            if (await this.IsActionActiveHelper("Cannot perform redo")) {
                return;
            }

            if (this.HasRedoActions) {
                await this.manager.OnRedoAsync();
                this.RaisePropertyChanged(nameof(this.HasUndoActions));
                this.RaisePropertyChanged(nameof(this.HasRedoActions));
                this.Notification.OnRedo();
            }
        }

        public async Task ClearAction() {
            if (await this.IsActionActiveHelper("Cannot clear actions")) {
                return;
            }

            this.manager.Clear();
            this.Notification.OnUndo();
        }

        private async Task<bool> IsActionActiveHelper(string message) {
            if (this.manager.IsUndoing) {
                await IoC.DialogService.ShowMessageAsync("Undo already active", message + ". An undo operation is already in progress");
            }
            else if (this.manager.IsRedoing) {
                await IoC.DialogService.ShowMessageAsync("Redo already active", message + ". A redo operation is already in progress");
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
                    await IoC.DialogService.ShowMessageAsync("Invalid value", "Value must be more than 0");
                    return null;
                }

                return integer;
            }

            await IoC.DialogService.ShowMessageAsync("Invalid value", "Value is not an integer: " + value);
            return null;
        }

        public async Task ResetAsync() {
            if (this.manager.IsUndoing || this.manager.IsRedoing) {
                await TaskManager.Instance.RunAsync(new TaskAction(async (p) => {
                    p.IsIndeterminate = true;
                    p.HeaderText = "Clearing History";
                    p.FooterText = "Waiting for a history action to complete...";
                    do {
                        await Task.Delay(250);
                        if (p.IsCancelled) {
                            this.manager.UnsafeReset();
                            p.FooterText = "Forcefully cancelled. App may be in an unstable state";
                            this.RaiseReset();
                            return;
                        }
                    } while (this.manager.IsUndoing || this.manager.IsRedoing);
                    p.FooterText = "All tasks finished";
                    this.RaiseReset();
                }));
            }

            this.manager.Reset();
            this.RaiseReset();
        }

        private void RaiseReset() {
            this.RaisePropertyChanged(nameof(this.MaxUndo));
            this.RaisePropertyChanged(nameof(this.MaxRedo));
            this.RaisePropertyChanged(nameof(this.HasUndoActions));
            this.RaisePropertyChanged(nameof(this.HasRedoActions));
        }
    }
}