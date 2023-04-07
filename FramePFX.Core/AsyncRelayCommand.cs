using System;
using System.Threading;
using System.Threading.Tasks;

namespace MCNBTViewer.Core {
    /// <summary>
    /// A simple relay command, which does not take any parameters
    /// </summary>
    public class AsyncRelayCommand : BaseRelayCommand {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Because <see cref="Execute"/> is async void, it can be fired multiple
        /// times while the task that <see cref="execute"/> returns is still running. This
        /// is used to track if it's running or not
        /// </summary>
        private volatile bool isRunning; // maybe switch to atomic Interlocked?

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) {
            return base.CanExecute(parameter) && (this.canExecute == null || this.canExecute());
        }

        public override async void Execute(object parameter) {
            if (this.isRunning) {
                return;
            }

            await this.ExecuteAsync();
        }

        public async Task ExecuteAsync() {
            if (this.isRunning) {
                return;
            }

            this.isRunning = true;
            try {
                this.RaiseCanExecuteChanged();
                await this.execute();
            }
            finally {
                this.isRunning = false;
                this.RaiseCanExecuteChanged();
            }
        }
    }

    public class AsyncRelayCommand<T> : BaseRelayCommand {
        private readonly Func<T, Task> execute;
        private readonly Func<T, bool> canExecute;
        private volatile bool isRunning;

        public bool ConvertParameter { get; set; }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, bool convertParameter = false) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            this.ConvertParameter = convertParameter;
        }

        public override bool CanExecute(object parameter) {
            if (this.isRunning) {
                return false;
            }

            if (this.ConvertParameter) {
                parameter = GetConvertedParameter<T>(parameter);
            }

            return base.CanExecute(parameter) && (parameter == null || parameter is T) && this.canExecute((T) parameter);
        }

        public override async void Execute(object parameter) {
            if (this.isRunning) {
                return;
            }

            this.isRunning = true;
            if (this.ConvertParameter) {
                parameter = GetConvertedParameter<T>(parameter);
            }

            try {
                this.RaiseCanExecuteChanged();
                await this.execute((T) parameter);
            }
            finally {
                this.isRunning = false;
                this.RaiseCanExecuteChanged();
            }
        }
    }
}