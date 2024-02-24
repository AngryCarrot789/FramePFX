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

using System;

namespace FramePFX.Services.Messages {
    public interface IUserInputDialogService {
        /// <summary>
        /// Shows a dialog which accepts a general text input, optionally with a validation predicate which
        /// prevents the dialog closing successfully if the value fails the validation
        /// </summary>
        /// <param name="caption">The window titlebar</param>
        /// <param name="message">A message to present to the user above the text input</param>
        /// <param name="defaultInput">
        /// The text that is placed in the text input by default. Default is null aka empty string
        /// </param>
        /// <param name="validate">
        /// A validator predicate. Default is null, meaning any value is allowed.
        /// This predicate should be fast, as it will be executed whenever the user types something
        /// </param>
        /// <param name="allowEmptyString">
        /// Allows this method to return an empty string if the text input is empty. Default is false
        /// </param>
        /// <returns>
        /// The text in the input area. Null if the input was empty, the user clicked cancel or force
        /// closed the dialog or the dialog somehow mysteriously closed
        /// </returns>
        string ShowSingleInputDialog(string caption, string message, string defaultInput = null, Predicate<string> validate = null, bool allowEmptyString = false);
    }
}