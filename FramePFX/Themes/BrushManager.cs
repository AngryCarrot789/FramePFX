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

/// <summary>
/// A factory used to create brushes
/// </summary>
public abstract class BrushManager {
    public static BrushManager Instance => Application.Instance.ServiceManager.GetService<BrushManager>();
    
    /// <summary>
    /// Creates a brush whose underlying colour does not change
    /// </summary>
    /// <param name="colour">The colour</param>
    /// <returns>The brush</returns>
    public abstract IStaticColourBrush CreateConstant(SKColor colour);

    /// <summary>
    /// Gets a known theme brush that may change at any time
    /// </summary>
    /// <param name="themeKey">The key</param>
    /// <returns>The brush</returns>
    public abstract IDynamicColourBrush GetDynamicThemeBrush(string themeKey);
    
    /// <summary>
    /// Gets a known theme brush as a constant (the returned value's underlying UI brush does not change)
    /// </summary>
    /// <param name="themeKey"></param>
    /// <returns></returns>
    public abstract IStaticColourBrush GetStaticThemeBrush(string themeKey);
}