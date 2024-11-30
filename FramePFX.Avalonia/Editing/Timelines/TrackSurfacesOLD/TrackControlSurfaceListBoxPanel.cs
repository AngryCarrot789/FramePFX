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

using System;
using Avalonia;
using Avalonia.Controls;

namespace FramePFX.Avalonia.Editing.Timelines.TrackSurfacesOLD;

public class TrackControlSurfaceListBoxPanel : Panel {
    public TrackControlSurfaceListBoxPanel() {
    }

    protected override Size MeasureOverride(Size availableSize) {
        Size size = new Size();
        Controls items = this.Children;
        int count = items.Count;
        for (int i = 0; i < count; i++) {
            Control element = items[i];
            element.Measure(availableSize);
            Size elemSz = element.DesiredSize;
            size = new Size(Math.Max(size.Width, elemSz.Width), size.Height + elemSz.Height);
        }

        if (count > 1) {
            size = size.WithHeight(size.Height + (count - 1));
        }

        return size;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        Controls items = this.Children;
        int count = items.Count;
        double rectY = 0.0;
        double num = 0.0;
        for (int i = 0; i < count; ++i) {
            Control element = items[i];
            rectY += num;
            num = element.DesiredSize.Height;
            element.Arrange(new Rect(0, rectY, Math.Max(finalSize.Width, element.DesiredSize.Width), num));
            rectY += 1;
        }

        return finalSize;
    }
}