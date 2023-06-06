using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Core.History {
    public class HistoryManager {
        private readonly LinkedList<IHistoryAction> undoList;
        private readonly LinkedList<IHistoryAction> redoList;

        public bool IsUndoing { get; private set; }
        public bool IsRedoing { get; private set; }

        /// <summary>
        /// Convenient property for checking if <see cref="IsUndoing"/> or <see cref="IsRedoing"/> is true
        /// </summary>
        public bool IsActionActive => this.IsUndoing || this.IsRedoing;

        public bool CanUndo => this.undoList.Count > 0;
        public bool CanRedo => this.redoList.Count > 0;

        public int MaxUndo { get; private set; }
        public int MaxRedo { get; private set; }

        public HistoryManager(int maxUndo = 200, int maxRedo = 200) {
            this.undoList = new LinkedList<IHistoryAction>();
            this.redoList = new LinkedList<IHistoryAction>();
            this.SetMaxUndo(maxUndo);
            this.SetMaxRedo(maxRedo);
        }

        public void SetMaxUndo(int maxUndo) {
            if (maxUndo < 1) {
                throw new ArgumentOutOfRangeException(nameof(maxUndo), "Value must be greater than 0");
            }

            if (maxUndo < this.MaxUndo) {
                int count = this.undoList.Count - maxUndo;
                for (int i = 0; i < count; i++) {
                    this.undoList.RemoveFirst();
                }
            }

            this.MaxUndo = maxUndo;
        }

        public void SetMaxRedo(int maxRedo) {
            if (maxRedo < 1) {
                throw new ArgumentOutOfRangeException(nameof(maxRedo), "Value must be greater than 0");
            }

            if (maxRedo < this.MaxRedo) {
                int count = this.redoList.Count - maxRedo;
                for (int i = 0; i < count; i++) {
                    this.redoList.RemoveFirst();
                }
            }

            this.MaxRedo = maxRedo;
        }

        public void AddAction(IHistoryAction action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            this.redoList.Clear();
            this.undoList.AddLast(action);
            while (this.undoList.Count > this.MaxUndo) { // loop just in case
                this.undoList.RemoveFirst();
            }
        }

        public void Clear() {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");
            this.undoList.Clear();
            this.redoList.Clear();
        }

        public async Task OnUndoAsync() {
            if (this.IsUndoing)
                throw new Exception("Undo is already in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            int index = this.undoList.Count - 1;
            if (index < 0) {
                throw new InvalidOperationException("Nothing to undo");
            }

            IHistoryAction action = this.undoList.Last.Value;
            this.undoList.RemoveLast();

            try {
                this.IsUndoing = true;
                await action.UndoAsync();
            }
            finally {
                this.IsUndoing = false;
                this.redoList.AddLast(action);
                while (this.redoList.Count > this.MaxRedo) { // loop just in case
                    this.redoList.RemoveFirst();
                }
            }
        }

        public async Task OnRedoAsync() {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is already in progress");

            int index = this.redoList.Count - 1;
            if (index < 0) {
                throw new InvalidOperationException("Nothing to redo");
            }

            IHistoryAction action = this.redoList.Last.Value;
            this.redoList.RemoveLast();

            try {
                this.IsRedoing = true;
                await action.RedoAsync();
            }
            finally {
                this.IsRedoing = false;
                this.undoList.AddLast(action);
                while (this.undoList.Count > this.MaxUndo) { // loop just in case
                    this.undoList.RemoveFirst();
                }
            }
        }
    }
}