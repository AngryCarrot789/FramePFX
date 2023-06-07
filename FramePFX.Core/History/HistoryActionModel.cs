namespace FramePFX.Core.History {
    public class HistoryActionModel {
        public delegate void UndoEventHandler(HistoryActionModel action);
        public delegate void RedoEventHandler(HistoryActionModel action);
        public delegate void RemovedEventHandler(HistoryActionModel action);

        public HistoryManager Manager { get; }

        public IHistoryAction Action { get; }

        public bool IsRemoved { get; set; }

        public event UndoEventHandler Undo;
        public event RedoEventHandler Redo;
        public event RemovedEventHandler Removed;

        public HistoryActionModel(HistoryManager manager, IHistoryAction action) {
            this.Manager = manager;
            this.Action = action;
        }

        public void OnUndo() {
            this.Undo?.Invoke(this);
        }

        public void OnRedo() {
            this.Redo?.Invoke(this);
        }

        public void OnRemoved() {
            this.IsRemoved = true;
            this.Removed?.Invoke(this);
        }
    }
}