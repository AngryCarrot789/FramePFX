using System.Windows.Input;

namespace FramePFX.Commands
{
    /// <summary>
    /// An interface for general relay commands
    /// </summary>
    public interface IRelayCommand : ICommand
    {
        /// <summary>
        /// Whether or not this relay command is enabled and can be executed. Affects the result of <see cref="ICommand.CanExecute"/>
        /// </summary>
        bool IsEnabled { get; set; }
    }
}