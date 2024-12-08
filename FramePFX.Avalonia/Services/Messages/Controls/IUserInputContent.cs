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

using FramePFX.Services.UserInputs;

namespace FramePFX.Avalonia.Services.Messages.Controls;

/// <summary>
/// An interface for user input content controls
/// </summary>
public interface IUserInputContent
{
    void Connect(UserInputDialog dialog, UserInputInfo info);

    void Disconnect();

    /// <summary>
    /// Try to focus the primary input field. If nothing exists to focus, this
    /// returns false which usually results in the confirm or cancel button being focused
    /// </summary>
    /// <returns></returns>
    bool FocusPrimaryInput();
}