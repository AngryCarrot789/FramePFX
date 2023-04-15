using System.Threading;
using System.Threading.Tasks;

namespace SharpPadV2.Core {
    public abstract class BaseAsyncRelayCommand : BaseRelayCommand {
        /// <summary>
        /// Because <see cref="Execute"/> is async void, it can be fired multiple
        /// times while the task that <see cref="execute"/> returns is still running. This
        /// is used to track if it's running or not
        /// </summary>
        private volatile int isRunningState; // maybe switch to atomic Interlocked?

        public override bool CanExecute(object parameter) {
            return this.isRunningState == 0 && base.CanExecute(parameter);
        }

        public override async void Execute(object parameter) {
            if (this.isRunningState == 1) {
                return;
            }

            await this.ExecuteAsyncCore(parameter);
        }

        public async Task ExecuteAsyncCore(object parameter) {
            if (Interlocked.CompareExchange(ref this.isRunningState, 1, 0) == 1) {
                return;
            }

            this.RaiseCanExecuteChanged();
            try {
                await this.ExecuteAsync(parameter);
            }
            finally {
                this.isRunningState = 0;
            }
            this.RaiseCanExecuteChanged();
        }

        protected abstract Task ExecuteAsync(object parameter);
    }
}