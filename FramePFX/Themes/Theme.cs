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

using SkiaSharp;

namespace FramePFX.Themes;

public delegate void ThemeColourChangedEventHandler(Theme theme, string key, SKColor newColour);

/// <summary>
/// Represents a theme 
/// </summary>
public abstract class Theme {
    /// <summary>
    /// Gets the theme manager associated with this theme
    /// </summary>
    public abstract ThemeManager ThemeManager { get; }
    
    /// <summary>
    /// Gets the name of this theme
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the theme keys for this theme. This may get modified by <see cref="SetThemeColour"/>
    /// </summary>
    public abstract IEnumerable<string> ThemeKeys { get; }

    /// <summary>
    /// Sets a theme colour to the given value. This method may
    /// trigger UI rendering if the theme key is in use
    /// </summary>
    /// <param name="key">The key</param>
    /// <param name="colour">The new colour</param>
    public abstract void SetThemeColour(string key, SKColor colour);
}