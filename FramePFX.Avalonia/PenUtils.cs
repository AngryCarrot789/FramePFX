// 
// Copyright (c) 2026-2026 REghZy
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

using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace FramePFX.Avalonia;

public class PenUtils {
    public static bool TryModifyOrCreate(ref IPen? pen, IBrush? brush, double thickness) {
        IPen? previousPen = pen;
        if (brush != null) {
            if (brush is IImmutableBrush immutableBrush) {
                pen = new ImmutablePen(immutableBrush, thickness);
                return true;
            }

            Pen mutablePen = previousPen as Pen ?? new Pen();
            mutablePen.Brush = brush;
            mutablePen.Thickness = thickness;
            pen = mutablePen;
            return !Equals(previousPen, pen);
        }

        pen = null;
        return previousPen != null;
    }
    
    public static bool TryModifyOrCreate(ref IPen? pen, IBrush? brush, double thickness, IList<double>? strokeDashArray, double strokeDaskOffset = default, PenLineCap lineCap = PenLineCap.Flat, PenLineJoin lineJoin = PenLineJoin.Miter, double miterLimit = 10.0) {
        IPen? previousPen = pen;
        if (brush is null) {
            pen = null;
            return previousPen != null;
        }

        IDashStyle? dashStyle = null;
        if (strokeDashArray is { Count: > 0 }) {
            // strokeDashArray can be IList (instead of AvaloniaList) in future
            // So, if it supports notification - create a mutable DashStyle
            dashStyle = strokeDashArray is INotifyCollectionChanged
                ? new DashStyle(strokeDashArray, strokeDaskOffset)
                : new ImmutableDashStyle(strokeDashArray, strokeDaskOffset);
        }

        if (brush is IImmutableBrush immutableBrush && dashStyle is null or ImmutableDashStyle) {
            pen = new ImmutablePen(immutableBrush, thickness, (ImmutableDashStyle?) dashStyle, lineCap, lineJoin, miterLimit);
            return true;
        }

        Pen mutablePen = previousPen as Pen ?? new Pen();
        mutablePen.Brush = brush;
        mutablePen.Thickness = thickness;
        mutablePen.LineCap = lineCap;
        mutablePen.LineJoin = lineJoin;
        mutablePen.DashStyle = dashStyle;
        mutablePen.MiterLimit = miterLimit;
        pen = mutablePen;
        return !Equals(previousPen, pen);
    }
}