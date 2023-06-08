using System;
using System.Threading.Tasks;
using FramePFX.Core.Utils;

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

        public async Task UndoAsync() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    await this.Action.UndoAsync();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to undo action", e));
                }

                try {
                    this.Undo?.Invoke(this);
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to fire model's undo event", e));
                }
            }
        }

        public async Task RedoAsync() {
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    await this.Action.RedoAsync();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to redo action", e));
                }

                try {
                    this.Redo?.Invoke(this);
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to fire model's redo event", e));
                }
            }
        }

        public void OnRemoved() {
            this.IsRemoved = true;
            using (ExceptionStack stack = new ExceptionStack()) {
                try {
                    this.Action.OnRemoved();
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to remove action", e));
                }

                try {
                    this.Removed?.Invoke(this);
                }
                catch (Exception e) {
                    stack.Push(new Exception("Failed to fire model's removed event", e));
                }
            }
        }
    }
}