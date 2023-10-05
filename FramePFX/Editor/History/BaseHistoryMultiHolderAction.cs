using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.History;
using FramePFX.Utils;

namespace FramePFX.Editor.History {
    /// <summary>
    /// A helper class for storing a "Holder" object (that implements <see cref="IHistoryHolder"/>), and updating the <see cref="IHistoryHolder.IsHistoryChanging"/> property of the holder before and after undoing/redoing
    /// </summary>
    /// <typeparam name="T">Type of holder object</typeparam>
    public abstract class BaseHistoryMultiHolderAction<T> : HistoryAction where T : class, IHistoryHolder {
        private readonly List<T> holders;
        private bool isRedoNext;

        public IReadOnlyList<T> Holders => this.holders;

        protected BaseHistoryMultiHolderAction(IEnumerable<T> holders) {
            this.holders = new List<T>(holders);
        }

        protected sealed override async Task UndoAsyncCore() {
            if (this.isRedoNext) {
                throw new Exception("Cannot undo action twice; it must be re-done");
            }

            this.isRedoNext = true;
            using (ErrorList stack = new ErrorList()) {
                int index = 0;
                foreach (T holder in this.holders) {
                    holder.IsHistoryChanging = true;
                    try {
                        await this.UndoAsync(holder, index++);
                    }
#if !DEBUG
                    catch (Exception e) {
                        stack.Add(e);
                    }
#endif
                    finally {
                        holder.IsHistoryChanging = false;
                    }
                }

                await this.OnUndoCompleteAsync(stack);
            }
        }

        protected sealed override async Task RedoAsyncCore() {
            if (!this.isRedoNext) {
                throw new Exception("Action has not been un-done yet; cannot redo before undo");
            }

            this.isRedoNext = false;
            using (ErrorList stack = new ErrorList()) {
                int index = 0;
                foreach (T holder in this.holders) {
                    holder.IsHistoryChanging = true;
                    try {
                        await this.RedoAsync(holder, index++);
                    }
#if !DEBUG
                    catch (Exception e) {
                        stack.Add(e);
                    }
#endif
                    finally {
                        holder.IsHistoryChanging = false;
                    }
                }

                await this.OnRedoCompleteAsync(stack);
            }
        }

        protected abstract Task UndoAsync(T holder, int i);

        protected abstract Task RedoAsync(T holder, int i);

        protected virtual Task OnUndoCompleteAsync(ErrorList errors) {
            return Task.CompletedTask;
        }

        protected virtual Task OnRedoCompleteAsync(ErrorList errors) {
            return Task.CompletedTask;
        }
    }
}