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
using FramePFX.Logging;
using FramePFX.Themes;

namespace FramePFX.BaseFrontEnd.Icons;

public class GeometryIconImpl : AbstractAvaloniaIcon {
    public string[] Elements { get; }

    public IColourBrush? TheFillBrush { get; }
    public IColourBrush? TheStrokeBrush { get; }

    private IBrush? myFillBrush, myPenBrush;
    private IPen? myPen;
    
    public double StrokeThickness { get; set; }
    
    private Geometry?[]? geometries;
    private readonly IDisposable disposeFillBrush, disposeStrokeBrush;

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
    
    public GeometryIconImpl(string name, IColourBrush? brush, IColourBrush? stroke, double strokeThickness, string[] svgElements) : base(name) {
        this.Elements = svgElements;
        this.TheFillBrush = brush;
        this.TheStrokeBrush = stroke;
        this.StrokeThickness = strokeThickness;
        
        if (brush is DynamicResourceAvaloniaColourBrush b) {
            this.disposeFillBrush = b.Subscribe(this.OnFillBrushInvalidated);
        }
        else if (brush != null) {
            this.myFillBrush = ((ImmutableAvaloniaColourBrush) brush).Brush;
        }
        
        if (stroke is DynamicResourceAvaloniaColourBrush s) {
            this.disposeFillBrush = s.Subscribe(this.OnStrokeBrushInvalidated);
        }
        else if (stroke != null) {
            this.myFillBrush = ((ImmutableAvaloniaColourBrush) stroke).Brush;
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

    public override void Render(DrawingContext context, Rect rect) {
        foreach (Geometry? geometry in this.Geometries) {
            if (geometry != null) {
                if (this.myPen == null && this.myPenBrush != null) {
                    this.myPen = new Pen(this.myPenBrush, this.StrokeThickness);
                }
                
                context.DrawGeometry(this.myFillBrush, this.myPen, geometry);
            }
        }
    }

    public override Size GetSize(Size availableSize) {
        Rect b = new Rect();
        foreach (Geometry? g in this.Geometries) {
            if (g == null)
                continue;
            
            Rect a = g.Bounds;
            double le = Math.Min(a.Left, b.Left), to = Math.Min(a.Top, b.Top);
            double ri = Math.Max(a.Right, b.Right), bo = Math.Max(a.Bottom, b.Bottom);
            b = new Rect(le, to, ri - le, bo - to);
        }

        return availableSize.Constrain(b.Size);
    }
}