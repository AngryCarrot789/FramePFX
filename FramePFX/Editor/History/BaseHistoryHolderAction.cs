using System.Threading.Tasks;
using FramePFX.History;

namespace FramePFX.Editor.History {
    /// <summary>
    /// A helper class for storing a "Holder" object (that implements <see cref="IHistoryHolder"/>), and updating the <see cref="IHistoryHolder.IsHistoryChanging"/> property of the holder before and after undoing/redoing
    /// </summary>
    /// <typeparam name="T">Type of holder object</typeparam>
    public abstract class BaseHistoryHolderAction<T> : HistoryAction where T : class, IHistoryHolder {
        public T Holder { get; }

        protected BaseHistoryHolderAction(T holder) {
            this.Holder = holder;
        }

        protected override async Task UndoAsyncCore() {
            this.Holder.IsHistoryChanging = true;
            try {
                await this.UndoAsyncForHolder();
            }
            finally {
                this.Holder.IsHistoryChanging = false;
            }
        }

        protected override async Task RedoAsyncCore() {
            this.Holder.IsHistoryChanging = true;
            try {
                await this.RedoAsyncForHolder();
            }
            finally {
                this.Holder.IsHistoryChanging = false;
            }
        }

        protected abstract Task UndoAsyncForHolder();

        protected abstract Task RedoAsyncForHolder();
    }
}