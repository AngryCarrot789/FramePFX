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

namespace FramePFX.Services.Messaging;

/// <summary>
/// An enum for the result of a simple message box
/// </summary>
public enum MessageBoxResult {
    /// <summary>
    /// Nothing was clicked (user force closed the window, e.g. by clicking the close button or pressing Esc)
    /// </summary>
    None = 0,

    /// <summary>
    /// User clicked OK
    /// </summary>
    OK = 1,

    /// <summary>
    /// User clicked Cancel
    /// </summary>
    Cancel = 2,

    /// <summary>
    /// User clicked Yes
    /// </summary>
    Yes = 3,

    /// <summary>
    /// User clicked No
    /// </summary>
    No = 4,
}