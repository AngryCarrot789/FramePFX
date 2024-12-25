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

using System;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;

namespace FramePFX.Avalonia.AvControls;

/// <summary>
/// A helper class for using the Path or Polygon controls in a panel or a standalone one.
/// For some reason they don't measure correctly and get clipped so we need to do some manual labour to get them to work
/// </summary>
public static class PathHelper {
    public static bool Arrange(TemplatedControl control, Layoutable? thing, Size finalSize, out Size arrange) {
        if (thing == null) {
            arrange = default;
            return false;
        }

        Size size = finalSize.Deflate(control.Padding);
        double sX = size.Width / thing.Width, sY = size.Height / thing.Height;
        thing.RenderTransform = new ScaleTransform(Math.Min(sX, sY), Math.Min(sX, sY));
        thing.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height).CenterRect(new Rect(0, 0, size.Width, size.Height)));
        arrange = finalSize;
        return true;
    }
}