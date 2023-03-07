using System;

namespace FrameControl.Core {
    /// <summary>
    /// A simple relay command, which does not take any parameters
    /// </summary>
    public class RelayCommand : BaseRelayCommand {
        /// <summary>
        /// Creates a new command that can always execute
        /// </summary>
        private readonly Action execute;

        /// <summary>
        /// True if command is executing, false otherwise
        /// </summary>
        private readonly Func<bool> canExecute;

        /// <summary>
        /// Initializes a new instance of <see cref="RelayCommand"/>
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        /// <param name="canExecute">The execution status logic</param>
        public RelayCommand(Action execute, Func<bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether this <see cref="BaseRelayCommand"/> can execute in its current state
        /// </summary>
        /// <param name="parameter">
        /// Data used by the command. This may be null, if the command does not require a parameter
        /// </param>
        /// <returns>
        /// True if the command can be executed, otherwise false if it cannot be executed
        /// </returns>
        public override bool CanExecute(object parameter) {
            return base.CanExecute(parameter) && (this.canExecute == null || this.canExecute());
        }

        /// <summary>
        /// Executes the <see cref="RelayCommand"/> on the current command target
        /// </summary>
        /// <param name="parameter">
        /// Extra data as the command's parameter. This can be null
        /// </param>
        public override void Execute(object parameter) {
            this.execute();
        }
    }
}