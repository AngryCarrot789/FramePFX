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

using Avalonia.Media;
using FramePFX.AdvancedMenuService;

namespace FramePFX.BaseFrontEnd.Windows;

/// <summary>
/// The base interface for any window or popup
/// </summary>
public interface IWindowBase {
    /// <summary>
    /// Gets or sets the main menu registry
    /// </summary>
    ContextRegistry ContextRegistry { get; set; }
    
    /// <summary>
    /// Gets or sets the text alignment used for the titlebar
    /// </summary>
    TextAlignment TitleBarAlignment { get; set; }

    /// <summary>
    /// Gets or sets the window's title/caption
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Gets whether this window is visible or not. Returns true when shown, and false when closed.
    /// </summary>
    bool IsVisible { get; }

    /// <summary>
    /// Gets or sets the minimum width this window can be
    /// </summary>
    double MinWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the minimum height this window can be
    /// </summary>
    double MinHeight { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum width this window can be
    /// </summary>
    double MaxWidth { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum height this window can be
    /// </summary>
    double MaxHeight { get; set; }
    
    /// <summary>
    /// Gets or sets the current width of this window
    /// </summary>
    double Width { get; set; }
    
    /// <summary>
    /// Gets or sets the current height of this window
    /// </summary>
    double Height { get; set; }

    /// <summary>
    /// Closes this window or dialog
    /// </summary>
    void Close();
}