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
using Avalonia.Skia;
using FramePFX.BaseFrontEnd.Themes.BrushFactories;
using FramePFX.BaseFrontEnd.Utils;
using FramePFX.Icons;
using FramePFX.Logging;
using FramePFX.Themes;
using SkiaSharp;
using Vector = Avalonia.Vector;

namespace FramePFX.BaseFrontEnd.Icons;

public class GeometryIconImpl : AbstractAvaloniaIcon {
    public readonly string[] Elements;
    public readonly IColourBrush? TheFillBrush;
    public readonly IColourBrush? TheStrokeBrush;
    public readonly double StrokeThickness;
    public readonly StretchMode Stretch;

    private IBrush? myFillBrush, myPenBrush;
    private IPen? myPen;

    private Geometry?[]? geometries;
    private readonly IDisposable? disposeFillBrush, disposeStrokeBrush;

    public Geometry?[] Geometries {
        get {
            if (this.geometries == null) {
                this.geometries = new Geometry[this.Elements.Length];
                for (int i = 0; i < this.Elements.Length; i++) {
                    
                    
                    try {
                        this.geometries[i] = Geometry.Parse(this.Elements[i]);
                    }
                    catch (Exception e) {
                        AppLogger.Instance.WriteLine("Error parsing SVG for svg icon: \n" + e);
                    }
                }
            }

            return this.geometries!;
        }
    }

    public GeometryIconImpl(string name, IColourBrush? brush, IColourBrush? stroke, double strokeThickness, string[] svgElements, StretchMode stretch) : base(name) {
        this.Elements = svgElements;
        this.TheFillBrush = brush;
        this.TheStrokeBrush = stroke;
        this.StrokeThickness = strokeThickness;
        this.Stretch = stretch;

        if (brush is DynamicAvaloniaColourBrush b) {
            this.disposeFillBrush = b.Subscribe(this.OnFillBrushInvalidated);
        }
        else if (brush != null) {
            this.myFillBrush = ((AvaloniaColourBrush) brush).Brush;
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
        using DrawingContext.PushedState? state = transform != SKMatrix.Identity ? context.PushTransform(transform.ToAvMatrix()) : null;
        
        foreach (Geometry? geometry in this.Geometries) {
            if (geometry != null) {
                if (this.myPen == null && this.myPenBrush != null) {
                    this.myPen = new Pen(this.myPenBrush, this.StrokeThickness);
                }

                // Geometry theGeo;
                // if (theMat == Matrix.Identity) {
                //     theGeo = geometry;
                // }
                // else {
                //     theGeo = geometry.Clone();
                //     theGeo.Transform = theGeo.Transform == null || theGeo.Transform.Value == Matrix.Identity ? new MatrixTransform(theMat) : (Transform) new MatrixTransform(theGeo.Transform.Value * theMat);
                // }

                context.DrawGeometry(this.myFillBrush, this.myPen, geometry);
            }
        }
    }

    public Rect GetBounds() {
        int count = 0;
        double l = double.MaxValue, t = double.MaxValue, r = double.MinValue, b = double.MinValue;
        foreach (Geometry? g in this.Geometries) {
            if (g == null) {
                continue;
            }

            Rect a = g.Bounds;
            l = Math.Min(a.Left, l);
            t = Math.Min(a.Top, t);
            r = Math.Max(a.Right, r);
            b = Math.Max(a.Bottom, b);
            count++;
        }

        return count > 0 ? new Rect(l, t, r - l, b - t) : default;
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretch) {
        (Size size, Matrix t) = SkiaAvUtils.CalculateSizeAndTransform(availableSize, this.GetBounds(), (Stretch) this.Stretch);
        return (size, t.ToSKMatrix());
    }
}