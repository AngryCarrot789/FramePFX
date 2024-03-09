//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System.Threading.Tasks;

namespace FramePFX.Utils.Commands
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