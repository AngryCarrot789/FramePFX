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

using System.Windows;

namespace FramePFX.Services.Messages {
    public interface IMessageDialogService {
        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="message">The main message content</param>
        /// <param name="buttons">The buttons to show</param>
        /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
        MessageBoxResult ShowMessage(string caption, string message, MessageBoxButton buttons = MessageBoxButton.OK);

        /// <summary>
        /// Shows a dialog message, blocking until it closes. This must be called on the application main thread
        /// </summary>
        /// <param name="caption">The window titlebar message</param>
        /// <param name="header">A message presented in bold above the message, a less concise caption but still short</param>
        /// <param name="message">The main message content</param>
        /// <param name="buttons">The buttons to show</param>
        /// <returns>The button that was clicked or none if they clicked esc or something bad happened</returns>
        MessageBoxResult ShowMessage(string caption, string header, string message, MessageBoxButton buttons = MessageBoxButton.OK);
    }
}