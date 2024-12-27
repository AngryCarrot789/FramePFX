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
using Avalonia.Controls;
using Avalonia.Media;
using FramePFX.BaseFrontEnd.Icons;
using FramePFX.Icons;

namespace FramePFX.BaseFrontEnd.AvControls;

/// <summary>
/// A control which presents an <see cref="FramePFX.Icons.Icon"/>
/// </summary>
public class IconControl : Control {
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconControl, Icon?>(nameof(Icon));

    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }
    
    public IconControl() {
    }

    static IconControl() {
        IconProperty.Changed.AddClassHandler<IconControl, Icon?>((d, e) => d.OnIconChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        AffectsRender<IconControl>(IconProperty);
    }

    private void OnIconChanged(Icon? oldValue, Icon? newValue) {
    }

    public override void Render(DrawingContext context) {
        base.Render(context);

        if (this.Icon is AbstractAvaloniaIcon icon) {
            icon.Render(context, this.Bounds);
        }
    }
}