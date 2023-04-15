using System;
using System.Threading.Tasks;

namespace FramePFX.Core {
    /// <summary>
    /// A simple relay command, which does not take any parameters
    /// </summary>
    public class AsyncRelayCommand : BaseAsyncRelayCommand {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;

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

        protected override Task ExecuteAsync(object parameter) {
            return this.execute();
        }
    }

    public class AsyncRelayCommand<T> : BaseAsyncRelayCommand {
        private readonly Func<T, Task> execute;
        private readonly Func<T, bool> canExecute;

        public bool ConvertParameter { get; set; }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, bool convertParameter = true) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            this.ConvertParameter = convertParameter;
        }

        public override bool CanExecute(object parameter) {
            if (base.CanExecute(parameter)) {
                parameter = ImplicitConvertParameter<T>(parameter, this.ConvertParameter);
                return (parameter == null || parameter is T) && this.canExecute((T) parameter);
            }

            return false;
        }

        protected override Task ExecuteAsync(object parameter) {
            parameter = ImplicitConvertParameter<T>(parameter, this.ConvertParameter);
            if (parameter == null || parameter is T) {
                return this.execute((T) parameter);
            }
            else {
                return Task.CompletedTask;
            }
        }
    }
}