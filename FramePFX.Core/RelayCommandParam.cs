using System;

namespace FramePFX.Core {
    /// <summary>
    /// A relay command, that allows passing a parameter to the command
    /// </summary>
    public class RelayCommandParam<T> : BaseRelayCommand {
        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        private readonly Action<T> execute;

        /// <summary>
        /// True if command is executing, false otherwise
        /// </summary>
        private readonly Func<T, bool> canExecute;

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic.</param>
        public RelayCommandParam(Action<T> execute, Func<T, bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) {
            return base.CanExecute(parameter) && (this.canExecute == null || (parameter == null || parameter is T) && this.canExecute((T) parameter));
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand"/> on the current command target
        /// </summary>
        /// <param name="parameter">
        /// Extra data as the command's parameter. Can be null
        /// </param>
        public override void Execute(object parameter) {
            if (parameter == null || parameter is T) {
                this.execute((T) parameter);
            }
            else {
                throw new InvalidCastException($"Parameter type ({parameter.GetType()}) cannot be used for the callback method (which requires type {typeof(T).Name})");
            }
        }
    }
}