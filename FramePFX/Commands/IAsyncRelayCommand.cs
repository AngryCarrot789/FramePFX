using System.Threading.Tasks;

namespace FramePFX.Commands
{
    public interface IAsyncRelayCommand : IRelayCommand
    {
        /// <summary>
        /// Gets whether or not this command is currently executing a task
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Executes this command asynchronously, if it is not already running
        /// </summary>
        /// <param name="parameter">A parameter to pass to the command</param>
        /// <returns>The command's work</returns>
        Task ExecuteAsync(object parameter);

        /// <summary>
        /// Executes this command if it is not already running. If it's running, this function returns false, otherwise true
        /// </summary>
        /// <param name="parameter">A parameter to pass to the command</param>
        /// <returns>The command's work</returns>
        Task<bool> TryExecuteAsync(object parameter);
    }
}