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

namespace FramePFX.Toolbars;

/// <summary>
/// A <see cref="IButtonElement"/> that has toggle features (that are purely visual)
/// </summary>
public interface IToggleButtonElement : IButtonElement {
    /// <summary>
    /// Gets or sets the checked state. This does not cause any underlying "click" button events.
    /// <para>
    /// The states are True or False, and Indeterminate when <see cref="IsThreeState"/> is true
    /// </para>
    /// </summary>
    bool? IsChecked { get; set; }

    /// <summary>
    /// Gets or sets if this toggle button has three states instead of two
    /// </summary>
    bool IsThreeState { get; set; }
}