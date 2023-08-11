using System.Threading.Tasks;
using FramePFX.Core.History;

namespace FramePFX.Core.Editor.History {
    /// <summary>
    /// A helper class for storing a "Holder" object (that implements <see cref="IHistoryHolder"/>), and updating the <see cref="IHistoryHolder.IsHistoryChanging"/> property of the holder before and after undoing/redoing
    /// </summary>
    /// <typeparam name="T">Type of holder object</typeparam>
    public abstract class BaseHistoryHolderAction<T> : IHistoryAction where T : class, IHistoryHolder {
        public T Holder { get; }

        protected BaseHistoryHolderAction(T holder) {
            this.Holder = holder;
        }

        public async Task UndoAsync() {
            this.Holder.IsHistoryChanging = true;
            try {
                await this.UndoAsyncCore();
            }
            finally {
                this.Holder.IsHistoryChanging = false;
            }
        }

        public async Task RedoAsync() {
            this.Holder.IsHistoryChanging = true;
            try {
                await this.RedoAsyncCore();
            }
            finally {
                this.Holder.IsHistoryChanging = false;
            }
        }

        protected abstract Task UndoAsyncCore();

        protected abstract Task RedoAsyncCore();

        public virtual void OnRemoved() {
        }
    }
}