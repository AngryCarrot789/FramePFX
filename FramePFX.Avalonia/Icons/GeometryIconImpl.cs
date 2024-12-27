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

using System;
using Avalonia;
using Avalonia.Media;
using FramePFX.Logging;

namespace FramePFX.Avalonia.Icons;

public class GeometryIconImpl : AbstractAvaloniaIcon {
    public string[] Elements { get; }

    public IBrush? Brush { get; }
    
    public IPen? Pen { get; }
    
    private Geometry?[]? geometries;
    
    public GeometryIconImpl(string name, IBrush? brush, IPen? pen, string[] svgElements) : base(name) {
        this.Elements = svgElements;
        this.Brush = brush;
        this.Pen = pen;
    }

    public override void Render(DrawingContext context, Rect rect) {
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

        foreach (Geometry? geometry in this.geometries) {
            if (geometry != null)
                context.DrawGeometry(this.Brush, this.Pen, geometry);
        }
    }
}