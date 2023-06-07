using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FramePFX.Core.History {
    public class HistoryManager {
        private readonly LinkedList<HistoryActionModel> undoList;
        private readonly LinkedList<HistoryActionModel> redoList;

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

        /// <summary>
        /// Returns the action that is next to be undone
        /// </summary>
        public HistoryActionModel NextUndo => this.undoList.Last?.Value;

        /// <summary>
        /// Returns the action that is next to be redone
        /// </summary>
        public HistoryActionModel NextRedo => this.redoList.Last?.Value;

        public HistoryManager(int maxUndo = 200, int maxRedo = 200) {
            this.undoList = new LinkedList<HistoryActionModel>();
            this.redoList = new LinkedList<HistoryActionModel>();
            this.SetMaxUndo(maxUndo);
            this.SetMaxRedo(maxRedo);
        }

        private static HistoryActionModel RemoveFirst(LinkedList<HistoryActionModel> list) {
            HistoryActionModel model = list.First.Value;
            model.OnRemoved();
            list.RemoveFirst();
            return model;
        }

        private static HistoryActionModel RemoveLast(LinkedList<HistoryActionModel> list) {
            HistoryActionModel model = list.Last.Value;
            model.OnRemoved();
            list.RemoveLast();
            return model;
        }

        public void SetMaxUndo(int maxUndo) {
            if (maxUndo < 1) {
                throw new ArgumentOutOfRangeException(nameof(maxUndo), "Value must be greater than 0");
            }

            if (maxUndo < this.MaxUndo) {
                int count = this.undoList.Count - maxUndo;
                for (int i = 0; i < count; i++) {
                    RemoveFirst(this.undoList);
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
                    RemoveFirst(this.redoList);
                }
            }

            this.MaxRedo = maxRedo;
        }

        public HistoryActionModel AddAction(IHistoryAction action) {
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            HistoryActionModel model = new HistoryActionModel(this, action);

            foreach (HistoryActionModel item in this.redoList)
                item.OnRemoved();

            this.redoList.Clear();
            this.undoList.AddLast(model);
            this.RemoveExcessiveFirstUndo();
            return model;
        }

        public void Clear() {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");

            foreach (HistoryActionModel item in this.undoList)
                item.OnRemoved();
            foreach (HistoryActionModel item in this.redoList)
                item.OnRemoved();

            this.undoList.Clear();
            this.redoList.Clear();
        }

        /// <summary>
        /// Undoes the last action or last redone action
        /// </summary>
        /// <returns>A task containing the undone action</returns>
        /// <exception cref="Exception">Undo or redo is in progress</exception>
        /// <exception cref="InvalidOperationException">Nothing to undo</exception>
        public async Task<HistoryActionModel> OnUndoAsync() {
            if (this.IsUndoing)
                throw new Exception("Undo is already in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is in progress");
            if (this.undoList.Count < 1)
                throw new InvalidOperationException("Nothing to undo");

            HistoryActionModel action = this.undoList.Last.Value;
            this.undoList.RemoveLast();

            try {
                this.IsUndoing = true;
                await action.Action.UndoAsync();
            }
            finally {
                action.OnUndo();
                this.IsUndoing = false;
                this.redoList.AddLast(action);
                this.RemoveExcessiveFirstRedo();
            }

            return action;
        }

        /// <summary>
        /// Redoes the last undone action
        /// </summary>
        /// <returns>A task containing the redone action</returns>
        /// <exception cref="Exception">Undo or redo is in progress</exception>
        /// <exception cref="InvalidOperationException">Nothing to redo</exception>
        public async Task<HistoryActionModel> OnRedoAsync() {
            if (this.IsUndoing)
                throw new Exception("Undo is in progress");
            if (this.IsRedoing)
                throw new Exception("Redo is already in progress");
            if (this.redoList.Count < 1)
                throw new InvalidOperationException("Nothing to redo");

            HistoryActionModel action = this.redoList.Last.Value;
            this.redoList.RemoveLast();

            try {
                this.IsRedoing = true;
                await action.Action.RedoAsync();
            }
            finally {
                action.OnRedo();
                this.IsRedoing = false;
                this.undoList.AddLast(action);
                this.RemoveExcessiveFirstUndo();
            }

            return action;
        }

        private void RemoveExcessiveFirstUndo() {
            while (this.undoList.Count > this.MaxUndo) { // loop just in case
                RemoveFirst(this.undoList);
            }
        }

        private void RemoveExcessiveFirstRedo() {
            while (this.redoList.Count > this.MaxUndo) { // loop just in case
                RemoveFirst(this.redoList);
            }
        }
    }
}