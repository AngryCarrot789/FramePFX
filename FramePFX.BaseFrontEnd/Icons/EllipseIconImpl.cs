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
using Avalonia.Media;
using FramePFX.BaseFrontEnd.Themes.BrushFactories;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Icons;
using FramePFX.Themes;
using SkiaSharp;

namespace FramePFX.BaseFrontEnd.Icons;

public class EllipseIconImpl : AbstractAvaloniaIcon {
    public readonly IColourBrush? TheFillBrush;
    public readonly IColourBrush? TheStrokeBrush;
    public readonly double StrokeThickness;
    public readonly double RadiusX;
    public readonly double RadiusY;

    private IBrush? myFillBrush, myPenBrush;
    private IPen? myPen;
    private readonly IDisposable? disposeFillBrush, disposeStrokeBrush;

    public EllipseIconImpl(string name, IColourBrush? fill, IColourBrush? stroke, double radiusX, double radiusY, double strokeThickness = 0) : base(name) {
        this.RadiusX = radiusX;
        this.RadiusY = radiusY;
        this.TheFillBrush = fill;
        this.TheStrokeBrush = stroke;
        this.StrokeThickness = strokeThickness;
        if (fill is DynamicAvaloniaColourBrush b) {
            this.disposeFillBrush = b.Subscribe(this.OnFillBrushInvalidated);
        }
        else if (fill != null) {
            this.myFillBrush = ((AvaloniaColourBrush) fill).Brush;
        }

        if (stroke is DynamicAvaloniaColourBrush s) {
            this.disposeStrokeBrush = s.Subscribe(this.OnStrokeBrushInvalidated);
        }
        else if (stroke != null) {
            this.myPenBrush = ((AvaloniaColourBrush) stroke).Brush;
        }
    }

    private void OnFillBrushInvalidated(IBrush? brush) {
        this.myFillBrush = brush;
        this.OnRenderInvalidated();
    }

    private void OnStrokeBrushInvalidated(IBrush? brush) {
        this.myPenBrush = brush;
        this.myPen = null;
        this.OnRenderInvalidated();
    }

    public override void Render(DrawingContext context, Rect size, SKMatrix transform) {
        if (this.myPen == null && this.myPenBrush != null) {
            this.myPen = new Pen(this.myPenBrush, this.StrokeThickness);
        }

        using DrawingContext.PushedState? state = transform != SKMatrix.Identity ? context.PushTransform(transform.ToAvMatrix()) : null;
        context.DrawEllipse(this.myFillBrush, this.myPen, new Point(size.Width / 2.0, size.Height / 2.0), this.RadiusX, this.RadiusY);
    }

    public Rect GetBounds() {
        return new Rect(0, 0, this.RadiusX * 2.0, this.RadiusY * 2.0);
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretch) {
        return (this.GetBounds().Size, SKMatrix.Identity);
    }
}