using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Core {
    public abstract class BaseAsyncRelayCommand : BaseRelayCommand {
        /// <summary>
        /// <see cref="Execute"/> is async void, meaning it can be fired multiple times while the task
        /// that returns is still running. This is used to track if it's running or not
        /// </summary>
        private volatile int isRunningState;

        public override bool CanExecute(object parameter) {
            return this.isRunningState == 0 && base.CanExecute(parameter) && this.isRunningState == 0; // 2nd running check juuust in case...
        }

        public override async void Execute(object parameter) {
            if (this.isRunningState == 0) {
                await this.ExecuteAsyncCore(parameter);
            }
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