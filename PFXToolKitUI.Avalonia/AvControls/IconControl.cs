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

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using PFXToolKitUI.Avalonia.Icons;
using PFXToolKitUI.Icons;
using SkiaSharp;

namespace PFXToolKitUI.Avalonia.AvControls;

/// <summary>
/// A control which presents an <see cref="PFXToolKitUI.Icons.Icon"/>
/// </summary>
public class IconControl : Control {
    public static readonly StyledProperty<Icon?> IconProperty = AvaloniaProperty.Register<IconControl, Icon?>(nameof(Icon));
    public static readonly StyledProperty<Stretch> StretchProperty = Shape.StretchProperty.AddOwner<IconControl>(new StyledPropertyMetadata<Stretch>(Stretch.Uniform));

    /// <summary>
    /// Gets or sets the icon we use for drawing this control
    /// </summary>
    public Icon? Icon {
        get => this.GetValue(IconProperty);
        set => this.SetValue(IconProperty, value);
    }

    public Stretch Stretch {
        get => this.GetValue(StretchProperty);
        set => this.SetValue(StretchProperty, value);
    }

    private bool isAttachedToVt;
    private AbstractAvaloniaIcon? attachedIcon;
    private SKMatrix myTransform;

    public IconControl() {
    }

    static IconControl() {
        IconProperty.Changed.AddClassHandler<IconControl, Icon?>((d, e) => d.OnIconChanged(e.OldValue.GetValueOrDefault(), e.NewValue.GetValueOrDefault()));
        AffectsMeasure<IconControl>(IconProperty, StretchProperty);
        AffectsRender<IconControl>(IconProperty, StretchProperty);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnAttachedToVisualTree(e);
        if (this.attachedIcon != null) { // ??? how...
            this.attachedIcon.RenderInvalidated -= this.OnIconInvalidated;
            this.attachedIcon = null;
        }

        if (this.Icon is AbstractAvaloniaIcon icon) {
            this.attachedIcon = icon;
            icon.RenderInvalidated += this.OnIconInvalidated;
        }

        this.isAttachedToVt = true;
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e) {
        base.OnDetachedFromVisualTree(e);
        if (this.attachedIcon != null) {
            this.attachedIcon.RenderInvalidated -= this.OnIconInvalidated;
            this.attachedIcon = null;
        }

        this.isAttachedToVt = false;
    }

    private void OnIconChanged(Icon? oldValue, Icon? newValue) {
        Debug.Assert(oldValue == null || !this.isAttachedToVt || ReferenceEquals(oldValue, this.attachedIcon));
        if (this.attachedIcon != null) {
            this.attachedIcon.RenderInvalidated -= this.OnIconInvalidated;
            this.attachedIcon = null;
        }

        if (newValue is AbstractAvaloniaIcon newIcon && this.isAttachedToVt) {
            this.attachedIcon = newIcon;
            newIcon.RenderInvalidated += this.OnIconInvalidated;
        }
    }

    private void OnIconInvalidated(object? sender, EventArgs e) {
        this.InvalidateVisual();
    }

    public override void Render(DrawingContext context) {
        base.Render(context);

        if (this.Icon is AbstractAvaloniaIcon icon) {
            DrawingContext.PushedState? renderOptions = null;
            if (!IIconPreferences.Instance.UseAntiAliasing) {
                renderOptions = context.PushRenderOptions(new RenderOptions() { EdgeMode = EdgeMode.Aliased, BitmapInterpolationMode = BitmapInterpolationMode.HighQuality });
            }

            using (renderOptions) {
                icon.Render(context, this.Bounds, this.myTransform);
            }
        }
    }

    protected override Size MeasureOverride(Size availableSize) {
        if (this.Icon is AbstractAvaloniaIcon icon) {
            (Size Size, SKMatrix Transform) m = icon.Measure(availableSize, (StretchMode) (int) this.Stretch);
            this.myTransform = m.Transform;
            return m.Size;
            // Size size = icon.GetSize();
            // return this.ScaleIcon ? availableSize.Constrain(size) : size;
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize) {
        return finalSize;
    }
}