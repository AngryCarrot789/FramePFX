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

using FramePFX.Icons;
using FramePFX.Interactivity.Contexts;
using FramePFX.Utils.Commands;

namespace FramePFX.Toolbars;

public delegate void ButtonContextInvalidatedEventHandler(IButtonElement button);

/// <summary>
/// A UI button. Do not implement this class directly, that is the front end's job
/// </summary>
public interface IButtonElement {
    /// <summary>
    /// Gets this button's current available full context data
    /// </summary>
    IContextData ContextData { get; }

    /// <summary>
    /// Gets or sets the command that this button will execute
    /// </summary>
    AsyncRelayCommand? Command { get; set; }

    /// <summary>
    /// Gets or sets this button's tooltip
    /// </summary>
    string? ToolTip { get; set; }
    
    /// <summary>
    /// Gets or sets if this button can be clicked
    /// </summary>
    bool IsEnabled { get; set; }
    
    /// <summary>
    /// Gets or sets if this button is visible or not. When false, it does not take up UI space
    /// </summary>
    bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the text displayed in this button. Setting this may do nothing as some buttons might not support text, only icons.
    /// </summary>
    string? Text { get; set; }
    
    /// <summary>
    /// Gets or sets the button's icon. Seting this may do nothing as some buttons might not support icons, only text.
    /// </summary>
    Icon? Icon { get; set; }

    /// <summary>
    /// An event fired when the effective context data of this button changes
    /// </summary>
    event ButtonContextInvalidatedEventHandler? ContextInvalidated;
}