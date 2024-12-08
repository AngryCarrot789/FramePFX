// 
// Copyright (c) 2024-2024 REghZy
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

namespace FramePFX.Services.UserInputs;

public interface IUserInputDialogService
{
    /// <summary>
    /// Shows an input dialog with a single input field
    /// </summary>
    /// <param name="info">The information to present in the dialog</param>
    /// <returns>
    /// An async boolean. True when closed successfully (you can accept the result, and trust the
    /// validation function was run), False when validation fails or the text field is empty and
    /// empty is disabled, or Null when the dialog closed unexpectedly</returns>
    Task<bool?> ShowInputDialogAsync(SingleUserInputInfo info);

    /// <summary>
    /// Shows an input dialog with two input fields
    /// </summary>
    /// <param name="info">The information to present in the dialog</param>
    /// <returns>
    /// An async boolean. True when closed successfully (you can accept the results, and trust the
    /// validation function was run), False when validation fails or the text field is empty and
    /// empty is disabled, or Null when the dialog closed unexpectedly</returns>
    Task<bool?> ShowInputDialogAsync(DoubleUserInputInfo info);
}