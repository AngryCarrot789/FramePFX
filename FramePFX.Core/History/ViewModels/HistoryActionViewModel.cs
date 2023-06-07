using System;

namespace FramePFX.Core.History.ViewModels {
    public class HistoryActionViewModel : BaseViewModel {
        public HistoryManagerViewModel HistoryManager { get; }

        public HistoryActionModel Model { get; }

        public IHistoryAction Action { get; }

        /// <summary>
        /// Information about the action, like what it is about
        /// </summary>
        public string Information { get; }

        public HistoryActionViewModel(HistoryManagerViewModel manager, HistoryActionModel model, IHistoryAction action, string information) {
            this.HistoryManager = manager ?? throw new ArgumentNullException(nameof(manager));
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
            this.Action = action ?? throw new ArgumentNullException(nameof(action));
            this.Information = string.IsNullOrEmpty(information) ? $"Unknown action ({action.GetType().Name})" : information;
        }
    }
}