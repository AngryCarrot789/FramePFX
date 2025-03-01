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

using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace PFXToolKitUI.Avalonia.Themes.Controls;

/// <summary>
/// A headered content control which has a piece of text at the top left, and content below
/// </summary>
public class GroupBox : HeaderedContentControl {
    public static readonly StyledProperty<IBrush> HeaderBrushProperty = AvaloniaProperty.Register<GroupBox, IBrush>("HeaderBrush", Brushes.Transparent);
    public static readonly StyledProperty<double> HeaderContentGapProperty = AvaloniaProperty.Register<GroupBox, double>("HeaderContentGap", 1.0);

    public IBrush HeaderBrush {
        get => this.GetValue(HeaderBrushProperty);
        set => this.SetValue(HeaderBrushProperty, value);
    }

    public double HeaderContentGap {
        get => this.GetValue(HeaderContentGapProperty);
        set => this.SetValue(HeaderContentGapProperty, value);
    }

    public GroupBox() {
    }
}