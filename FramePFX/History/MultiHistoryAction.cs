using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.History {
    public class MultiHistoryAction : HistoryAction {
        public List<HistoryAction> Actions { get; }

        public MultiHistoryAction(List<HistoryAction> actions) {
            this.Actions = actions ?? throw new ArgumentNullException(nameof(actions));
        }

        protected override async Task UndoAsyncCore() {
            foreach (HistoryAction action in this.Actions) {
                await action.UndoAsync();
            }
        }

        protected override async Task RedoAsyncCore() {
            foreach (HistoryAction action in this.Actions) {
                await action.RedoAsync();
            }
        }

        public override void OnRemoved() {
            foreach (HistoryAction action in this.Actions) {
                action.OnRemoved();
            }
        }
    }
}