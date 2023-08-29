using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.History {
    public class MultiHistoryAction : IHistoryAction {
        public List<IHistoryAction> Actions { get; }

        public MultiHistoryAction(List<IHistoryAction> actions) {
            this.Actions = actions ?? throw new ArgumentNullException(nameof(actions));
        }

        public async Task UndoAsync() {
            foreach (IHistoryAction action in this.Actions) {
                await action.UndoAsync();
            }
        }

        public async Task RedoAsync() {
            foreach (IHistoryAction action in this.Actions) {
                await action.RedoAsync();
            }
        }

        public void OnRemoved() {
            foreach (IHistoryAction action in this.Actions) {
                action.OnRemoved();
            }
        }
    }
}