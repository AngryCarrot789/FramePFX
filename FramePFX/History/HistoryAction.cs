using System;
using System.Threading.Tasks;
using FramePFX.History.Exceptions;

namespace FramePFX.History {
    /// <summary>
    /// An action that can be undone and then also redone (but only after being undone)
    /// </summary>
    public abstract class HistoryAction {
        private bool isRedoNext;

        /// <summary>
        /// Undoes the action. E.g. you created a file. Calling this will then delete that file
        /// </summary>
        /// <returns>A task to await for the undo to fully complete</returns>
        /// <exception cref="Exception">Undo was already called but redo was not; undo twice sequentially is prohibited</exception>
        public Task UndoAsync() {
            if (this.isRedoNext) {
                throw InvalidHistoryOrderException.MultiUndo();
            }

            this.isRedoNext = true;
            return this.UndoAsyncCore();
        }

        /// <summary>
        /// Redoes an action that was undone. E.g. you created a file and then deleted
        /// it, redoing will then re-create that file (as if you never un-did anything)
        /// </summary>
        /// <returns>A task to await for the redo to fully complete</returns>
        /// <exception cref="Exception">
        /// Undo was never called or redo was invoked last but undo was not; undo twice sequentially is prohibited
        /// </exception>
        public Task RedoAsync() {
            if (!this.isRedoNext) {
                throw InvalidHistoryOrderException.RedoBeforeUndo();
            }

            this.isRedoNext = false;
            return this.RedoAsyncCore();
        }

        /// <summary>
        /// Undoes the action. E.g. you created a file. Calling this will then delete that file
        /// </summary>
        /// <returns></returns>
        protected abstract Task UndoAsyncCore();

        /// <summary>
        /// Redoes an action that was undone. E.g. you created a file and then deleted
        /// it, redoing will then re-create that file as if you never undid anything
        /// </summary>
        /// <returns></returns>
        protected abstract Task RedoAsyncCore();

        /// <summary>
        /// Called once this history action is no longer reachable, e.g. it is removed from the history queue because there were too many actions to undo/redo
        /// <para>
        /// This should clean up any resources that, for example, implement <see cref="IDisposable"/>. This method should also not throw an exception
        /// </para>
        /// </summary>
        /// <returns></returns>
        public virtual void OnRemoved() {
        }
    }
}