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
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Icons;

namespace FramePFX.BaseFrontEnd.AvControls;

/// <summary>
/// A button that uses a <see cref="FramePFX.Icons.Icon"/> to present an icon for the button contents
/// </summary>
public class IconToggleButton : ToggleButton, IIconButton {
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconToggleButton, Icon?>(nameof(Icon));
    public static readonly StyledProperty<Stretch> StretchProperty = AvaloniaProperty.Register<IconToggleButton, Stretch>(nameof(Stretch));

    private double? iconW, iconH;

    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    public Stretch Stretch {
        get => this.GetValue(StretchProperty);
        set => this.SetValue(StretchProperty, value);
    }

    public double? IconWidth {
        get => this.iconW;
        set {
            this.iconW = value;
            if (this.PART_IconControl != null) {
                this.PART_IconControl.Width = value ?? double.NaN;
            }
        }
    }

    public double? IconHeight {
        get => this.iconH;
        set {
            this.iconH = value;
            if (this.PART_IconControl != null) {
                this.PART_IconControl.Height = value ?? double.NaN;
            }
        }
    }

    private IconControl? PART_IconControl;
    
    public IconToggleButton() {
        
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        this.PART_IconControl = e.NameScope.GetTemplateChild<IconControl>("PART_IconControl");
        if (this.iconW.HasValue)
            this.PART_IconControl.Width = this.iconW.Value;
        if (this.iconH.HasValue)
            this.PART_IconControl.Height = this.iconH.Value;
    }
}