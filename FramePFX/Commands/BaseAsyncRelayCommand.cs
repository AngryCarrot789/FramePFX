using System.Threading;
using System.Threading.Tasks;

namespace FramePFX.Commands {
    /// <summary>
    /// <para>
    /// A base async relay command class which extends <see cref="BaseRelayCommand"/> and also implements a mechanism for
    /// tracking if the command is currently being run, and if so, ignores any attempt to execute (either via <see cref="Execute"/>,
    /// <see cref="ExecuteAsync"/> or <see cref="TryExecuteAsync"/>)
    /// </para>
    /// <para>
    /// <see cref="IsRunning"/> (and most likely <see cref="CanExecute"/> too) return false if the command is being run, however, this shouldn't
    /// be relied on due to the reality of multithreading; the command could finish just as another piece of code detects it's already running
    /// </para>
    /// </summary>
    public abstract class BaseAsyncRelayCommand : BaseRelayCommand, IAsyncRelayCommand {
        /// <summary>
        /// Because <see cref="Execute"/> is async void, it can be fired multiple
        /// times while the task that <see cref="execute"/> returns is still running. This
        /// is used to track if it's running or not
        /// </summary>
        private volatile int isRunningState;

        /// <summary>
        /// Whether or not this command is running. Realistically, this shouldn't be used to determine whether to execute the
        /// command or not, due to the fact that the command may finish as soon as this property is fetched.
        /// See <see cref="TryExecuteAsync"/> for a somewhat workaround
        /// </summary>
        public bool IsRunning => this.isRunningState == 1;

        protected BaseAsyncRelayCommand() {
        }

        /// <summary>
        /// Whether or not this async command can actually run or not. If it is already running, this will (typically) return false
        /// <para>
        /// <see cref="TryExecuteAsync"/> is recommended over the standard way of calling commands which is:
        /// <code>if (cmd.CanExecute(param)) cmd.Execute(param)</code>
        /// </para>
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        /// <returns>Whether or not this command can be executed or not</returns>
        public sealed override bool CanExecute(object parameter) {
            return this.isRunningState == 0 && base.CanExecute(parameter) && this.CanExecuteCore(parameter);
        }

        /// <summary>
        /// The core method for checking if the command implementation can execute or not. This is called by <see cref="CanExecute"/>
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        /// <returns>Whether or not this command can be executed or not</returns>
        protected virtual bool CanExecuteCore(object parameter) {
            return true;
        }

        /// <summary>
        /// Executes this async command. This is async void, so it may return before the actual
        /// command has finished executing. <see cref="ExecuteAsync"/> is recommended for that reason,
        /// because this function just calls that
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        public sealed override async void Execute(object parameter) {
            await this.ExecuteAsync(parameter);
        }

        // The 2 functions below have almost the same copied code in order to give a slightly cleaner
        // stack trace when debugging... and it probably also improves performance slightly

        /// <summary>
        /// Executes this async command, if it is not running. If the command is already running,
        /// then this method will return and the command will not be executed.
        /// <para>
        /// <see cref="TryExecuteAsync"/> should be used if you need to check if the command actually executed or not
        /// </para>
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        // Slight optimisation by not using async for ExecuteAsync, so that a state machine isn't needed
        public async Task ExecuteAsync(object parameter) {
            if (Interlocked.CompareExchange(ref this.isRunningState, 1, 0) == 0) {
                try {
                    this.RaiseCanExecuteChanged();
                    await this.ExecuteCoreAsync(parameter);
                }
                finally {
                    this.isRunningState = 0;
                }

                this.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Attempts to executing this async command. If the command is already running, then this method will return
        /// false and the command will not be executed. Otherwise, the command is executed and true is returned
        /// <para>
        /// This will query <see cref="CanExecute"/>
        /// </para>
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        public async Task<bool> TryExecuteAsync(object parameter) {
            if (this.CanExecute(parameter) && Interlocked.CompareExchange(ref this.isRunningState, 1, 0) == 0) {
                try {
                    this.RaiseCanExecuteChanged();
                    await this.ExecuteCoreAsync(parameter);
                }
                finally {
                    this.isRunningState = 0;
                }

                this.RaiseCanExecuteChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// The abstract function to be implemented by classes that extend <see cref="BaseAsyncRelayCommand"/> to actually "do" something
        /// </summary>
        /// <param name="parameter">The parameter passed to this command</param>
        /// <returns>A task...</returns>
        protected abstract Task ExecuteCoreAsync(object parameter);
    }
}