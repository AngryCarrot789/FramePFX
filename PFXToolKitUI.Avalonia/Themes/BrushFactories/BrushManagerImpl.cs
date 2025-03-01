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

using Avalonia.Media;
using PFXToolKitUI.Themes;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.Themes.BrushFactories;

public class BrushManagerImpl : BrushManager {
    private Dictionary<string, DynamicAvaloniaColourBrush>? cachedBrushes;

    public override ConstantAvaloniaColourBrush CreateConstant(SKColor colour) {
        // Not really any point to caching an immutable brush
        return new ConstantAvaloniaColourBrush(colour);
    }

    public override DynamicAvaloniaColourBrush GetDynamicThemeBrush(string themeKey) {
        if (this.cachedBrushes == null) {
            this.cachedBrushes = new Dictionary<string, DynamicAvaloniaColourBrush>();
        }
        else if (this.cachedBrushes.TryGetValue(themeKey, out DynamicAvaloniaColourBrush? existingBrush)) {
            return existingBrush;
        }

        // Since these brushes can be quite expensive to listen for changes, we want to try and always cache them
        DynamicAvaloniaColourBrush brush = new DynamicAvaloniaColourBrush(themeKey);
        this.cachedBrushes[themeKey] = brush;
        return brush;
    }

    public override IStaticColourBrush GetStaticThemeBrush(string themeKey) {
        if (ThemeManagerImpl.TryFindBrushInApplicationResources(themeKey, out IBrush? brush)) {
            return new StaticAvaloniaColourBrush(themeKey, brush.ToImmutable());
        }

        return new StaticAvaloniaColourBrush(themeKey, null);
    }
}