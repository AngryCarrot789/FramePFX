using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FramePFX.Core.History;
using FramePFX.Core.Utils;

namespace FramePFX.Core.Editor.History {
    /// <summary>
    /// A helper class for storing a "Holder" object (that implements <see cref="IHistoryHolder"/>), and updating the <see cref="IHistoryHolder.IsHistoryChanging"/> property of the holder before and after undoing/redoing
    /// </summary>
    /// <typeparam name="T">Type of holder object</typeparam>
    public abstract class BaseHistoryMultiHolderAction<T> : IHistoryAction where T : class, IHistoryHolder {
        private readonly List<T> holders;

        private int state;

        public IReadOnlyList<T> Holders => this.holders;

        protected BaseHistoryMultiHolderAction(IEnumerable<T> holders) {
            this.holders = new List<T>(holders);
            this.state = 0;
        }

        public async Task UndoAsync() {
            if (this.state != 0) {
                throw new Exception("Action has not been re-done yet");
            }

            this.state = 1;
            using (ExceptionStack stack = new ExceptionStack()) {
                int index = 0;
                foreach (T holder in this.holders) {
                    holder.IsHistoryChanging = true;
                    try {
                        await this.UndoAsyncCore(holder, index++);
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
            }
        }

        public async Task RedoAsync() {
            if (this.state != 1) {
                throw new Exception("Action has not been un-done yet");
            }

            this.state = 0;
            using (ExceptionStack stack = new ExceptionStack()) {
                int index = 0;
                foreach (T holder in this.holders) {
                    holder.IsHistoryChanging = true;
                    try {
                        await this.RedoAsyncCore(holder, index++);
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
            }
        }

        protected abstract Task UndoAsyncCore(T holder, int i);

        protected abstract Task RedoAsyncCore(T holder, int i);

        public virtual void OnRemoved() {

        }
    }
}