//
// Copyright (c) 2023-2024 REghZy
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

namespace FramePFX.Editing;

public static class TimelineUtils {
    public const double MinUnitZoom = 0.1d;
    public const double MaxUnitZoom = 200d;
    public static readonly object ZeroDoubleBox = 0d;
    public static readonly object ZeroLongBox = 0L;
    public static readonly object MinUnitZoomDoubleBox = MinUnitZoom;
    public static readonly object MaxUnitZoomDoubleBox = MaxUnitZoom;

    // use object and double based functions to slightly help performance. not like
    // it will be measurable but oh well. WPF uses an internal BoolBox class so...

    public static object ClampUnit(object value) {
        double dval = (double) value;
        if (dval < MinUnitZoom) {
            return MinUnitZoomDoubleBox;
        }
        else if (dval > MaxUnitZoom) {
            return MaxUnitZoomDoubleBox;
        }
        else {
            return value;
        }
    }

    public static double ClampUnit(double value) {
        if (value < MinUnitZoom) {
            return MinUnitZoom;
        }
        else if (value > MaxUnitZoom) {
            return MaxUnitZoom;
        }
        else {
            return value;
        }
    }

    public static long PixelToFrame(double pixels, double zoom, bool round = false) {
        return (long) (round ? Math.Round(pixels / zoom) : (pixels / zoom));
    }

    public static double FrameToPixel(long pixels, double zoom) {
        return pixels * zoom;
    }
}