using System;
using System.Threading.Tasks;

namespace FramePFX.Commands
{
    /// <summary>
    /// A simple async relay command, which does not take any parameters
    /// </summary>
    public class AsyncRelayCommand : BaseAsyncRelayCommand
    {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        protected override bool CanExecuteCore(object parameter)
        {
            return this.canExecute == null || this.canExecute();
        }

        protected override Task ExecuteCoreAsync(object parameter)
        {
            return this.execute();
        }
    }

    /// <summary>
    /// A simple async relay command, which may take a parameter
    /// </summary>
    /// <typeparam name="T">The type of parameter</typeparam>
    public class AsyncRelayCommand<T> : BaseAsyncRelayCommand
    {
        private readonly Func<T, Task> execute;
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// Whether or not to convert the parameter to <see cref="T"/> (e.g. if T is a boolean and the parameter is a string, it is easily convertible)
        /// </summary>
        public bool ConvertParameter { get; set; }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, bool convertParameter = true)
        {
            if (execute == null)
            {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            this.ConvertParameter = convertParameter;
        }

        protected override bool CanExecuteCore(object parameter)
        {
            if (this.ConvertParameter)
            {
                parameter = GetConvertedParameter<T>(parameter);
            }

            return this.canExecute == null ||
                   parameter == null && this.canExecute(default) ||
                   parameter is T t && this.canExecute(t);
        }

        protected override Task ExecuteCoreAsync(object parameter)
        {
            if (this.ConvertParameter)
            {
                parameter = GetConvertedParameter<T>(parameter);
            }

            if (parameter == null)
            {
                return this.execute(default);
            }
            else if (parameter is T value)
            {
                return this.execute(value);
            }
            else
            {
                return Task.CompletedTask;
            }
        }
    }
}