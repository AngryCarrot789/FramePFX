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

    SKMatrix Transpose(SKMatrix matrix)
    {
        return new SKMatrix
        {
            ScaleX = matrix.ScaleX, // row 1, col 1 stays the same
            SkewX = matrix.SkewY,  // row 1, col 2 becomes row 2, col 1
            TransX = matrix.Persp0, // row 1, col 3 becomes row 3, col 1

            SkewY = matrix.SkewX,  // row 2, col 1 becomes row 1, col 2
            ScaleY = matrix.ScaleY, // row 2, col 2 stays the same
            TransY = matrix.Persp1, // row 2, col 3 becomes row 3, col 2

            Persp0 = matrix.TransX, // row 3, col 1 becomes row 1, col 3
            Persp1 = matrix.TransY, // row 3, col 2 becomes row 2, col 3
            Persp2 = matrix.Persp2  // row 3, col 3 stays the same
        };
    }
    
    internal static Matrix ToAvaloniaMatrix(SKMatrix m)
    {
        return new Matrix(m.ScaleX, m.SkewY, m.Persp0, m.SkewX, m.ScaleY, m.Persp1, m.TransX, m.TransY, m.Persp2);
    }
    
    public override void Render(DrawingContext context, Rect size, SKMatrix transform) {
        // DrawingContext.PushedState? x = null;
        // if (scale) {
        //     Rect geometryBounds = this.GetBounds();
        //     Vector theScale = new Vector(size.Width / geometryBounds.Width, size.Height / geometryBounds.Height);
        //     x = context.PushTransform(Matrix.CreateScale(theScale));
        // }

        Matrix theMat = ToAvaloniaMatrix(transform);
        foreach (Geometry? geometry in this.Geometries) {
            if (geometry != null) {
                if (this.myPen == null && this.myPenBrush != null) {
                    this.myPen = new Pen(this.myPenBrush, this.StrokeThickness);
                }

                Geometry theGeo;
                if (theMat == Matrix.Identity) {
                    theGeo = geometry;
                }
                else {
                    theGeo = geometry.Clone();
                    theGeo.Transform = theGeo.Transform == null || theGeo.Transform.Value == Matrix.Identity ? new MatrixTransform(theMat) : (Transform) new MatrixTransform(theGeo.Transform.Value * theMat);
                }

                context.DrawGeometry(this.myFillBrush, this.myPen, theGeo);
            }
        }
    }

    public Rect GetBounds() {
        double l = double.MaxValue, t = double.MaxValue, r = double.MinValue, b = double.MinValue;
        foreach (Geometry? g in this.Geometries) {
            if (g == null)
                continue;

            Rect a = g.Bounds;
            l = Math.Min(a.Left, l);
            t = Math.Min(a.Top, t);
            r = Math.Max(a.Right, r);
            b = Math.Max(a.Bottom, b);
        }

        return new Rect(l, t, r - l, b - t);
    }

    public override (Size Size, SKMatrix Transform) Measure(Size availableSize, StretchMode stretch) {
        (Size size, Matrix t) = CalculateSizeAndTransform(availableSize, this.GetBounds(), (Stretch) this.Stretch);
        return (size, t.ToSKMatrix());
    }

    internal static (Size size, Matrix transform) CalculateSizeAndTransform(Size availableSize, Rect shapeBounds, Stretch Stretch) {
        Size size = new Size(shapeBounds.Right, shapeBounds.Bottom);
        Matrix matrix1 = Matrix.Identity;
        double width = availableSize.Width;
        double height = availableSize.Height;
        double num1 = 0.0;
        double num2 = 0.0;
        if (Stretch != Stretch.None) {
            size = shapeBounds.Size;
            matrix1 = Matrix.CreateTranslation(-(Vector) shapeBounds.Position);
        }

        if (double.IsInfinity(availableSize.Width))
            width = size.Width;
        if (double.IsInfinity(availableSize.Height))
            height = size.Height;
        if (shapeBounds.Width > 0.0)
            num1 = width / size.Width;
        if (shapeBounds.Height > 0.0)
            num2 = height / size.Height;
        if (double.IsInfinity(availableSize.Width))
            num1 = num2;
        if (double.IsInfinity(availableSize.Height))
            num2 = num1;
        switch (Stretch) {
            case Stretch.Fill:
                if (double.IsInfinity(availableSize.Width))
                    num1 = 1.0;
                if (double.IsInfinity(availableSize.Height)) {
                    num2 = 1.0;
                    break;
                }

            break;
            case Stretch.Uniform:       num1 = num2 = Math.Min(num1, num2); break;
            case Stretch.UniformToFill: num1 = num2 = Math.Max(num1, num2); break;
            default:                    num1 = num2 = 1.0; break;
        }

        Matrix matrix2 = matrix1 * Matrix.CreateScale(num1, num2);
        return (new Size(size.Width * num1, size.Height * num2), matrix2);
    }
}