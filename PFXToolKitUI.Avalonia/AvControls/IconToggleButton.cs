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
using PFXToolKitUI.Icons;

namespace PFXToolKitUI.Avalonia.AvControls;

/// <summary>
/// A button that uses a <see cref="PFXToolKitUI.Icons.Icon"/> to present an icon for the button contents
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

    public double? IconMaxWidth {
        get => this.iconW;
        set {
            this.iconW = value;
            IconButtonHelper.SetMaxWidth(this.PART_IconControl, value);
        }
    }

    public double? IconMaxHeight {
        get => this.iconH;
        set {
            this.iconH = value;
            IconButtonHelper.SetMaxHeight(this.PART_IconControl, value);
        }
    }

    private IconControl? PART_IconControl;

    public IconToggleButton() {
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e) {
        base.OnApplyTemplate(e);
        IconButtonHelper.ApplyTemplate(this, e, ref this.PART_IconControl);
    }
}