using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FramePFX.Commands;
using FramePFX.History;
using FramePFX.History.ViewModels;
using FramePFX.Utils;

namespace FramePFX.Editor.PropertyEditors {
    /// <summary>
    /// A command that helps manage a begin and end edit state, and also pushing changes to the history
    /// </summary>
    public class EditStateCommand : BaseAsyncRelayCommand {
        private readonly Func<HistoryAction> historyFunction;
        private HistoryAction historyAction;

        public bool IsEditing { get; private set; }

        public string HistoryMessage { get; }

        public HistoryAction HistoryAction => this.historyAction;

        public EditStateCommand(Func<HistoryAction> historyFunction, string historyMessage = "Unnamed action") {
            this.HistoryMessage = historyMessage ?? throw new ArgumentNullException(nameof(historyMessage));
            this.historyFunction = historyFunction;
        }

        protected override bool CanExecuteCore(object parameter) {
            if (!(parameter is ValueModState)) {
                throw new Exception("Parameter must be " + nameof(ValueModState));
            }

            return true;
        }

        protected override async Task ExecuteCoreAsync(object parameter) {
            if (!(parameter is ValueModState state)) {
                throw new Exception("Parameter must be " + nameof(ValueModState));
            }

            HistoryAction history;
            switch (state) {
                case ValueModState.Begin: {
                    if (this.historyAction != null) { // shouldn't be true... but just in case
                        HistoryManagerViewModel.Instance.AddAction(this.historyAction, this.HistoryMessage);
                        Debug.WriteLine("Begin was called excessively");
                    }

                    this.historyAction = this.historyFunction();
                    this.IsEditing = true;
                    break;
                }
                case ValueModState.Finish: {
                    this.IsEditing = false;
                    if (Helper.Exchange(ref this.historyAction, null, out history))
                        HistoryManagerViewModel.Instance.AddAction(history, this.HistoryMessage);
                    break;
                }
                case ValueModState.Cancelled: {
                    this.IsEditing = false;
                    if (Helper.Exchange(ref this.historyAction, null, out history))
                        await history.UndoAsync();
                    break;
                }
            }
        }

        public void OnReset() {
            this.IsEditing = true;
            if (Helper.Exchange(ref this.historyAction, null, out HistoryAction history)) {
                HistoryManagerViewModel.Instance.AddAction(history, this.HistoryMessage);
            }
        }
    }
}