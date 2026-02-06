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

using System.Drawing;
using System.Numerics;
using SkiaSharp;

namespace FramePFX;

public static class SKUtils {
    public static SKSizeI Long2SizeI(ulong n) => new SKSizeI((int) (n >> 32), (int) (n & uint.MaxValue));
    public static ulong ToLong(this SKSizeI s) => ((ulong) s.Width << 32) | (uint) s.Height;

    public static SKPoint AsSkia(this Point point) {
        return new SKPoint((float) point.X, (float) point.Y);
    }

    public static SKRect ToRectWH(this Vector2 size, float x = 0F, float y = 0F) {
        return new SKRect(x, y, size.X + x, size.Y + y);
    }

    public static SKRect ToRect(this SKImageInfo imgInfo) {
        return new SKRect(0F, 0F, imgInfo.Width, imgInfo.Height);
    }

    public static SKRect ToRectRB(this SKImageInfo imgInfo, float left, float top) {
        return new SKRect(left, top, imgInfo.Width, imgInfo.Height);
    }

    public static SKRect ToRectWH(this SKImageInfo imgInfo, float x, float y) {
        return new SKRect(x, y, imgInfo.Width + x, imgInfo.Height + y);
    }

    public static SKRect FloorAndCeil(this SKRect rect) {
        return new SKRect((float) Math.Floor(rect.Left), (float) Math.Floor(rect.Top), (float) Math.Ceiling(rect.Right), (float) Math.Ceiling(rect.Bottom));
    }

    public static SKRect CeilAndFloor(this SKRect rect) {
        return new SKRect((float) Math.Ceiling(rect.Left), (float) Math.Ceiling(rect.Top), (float) Math.Floor(rect.Right), (float) Math.Floor(rect.Bottom));
    }

    public static SKRect Round(this SKRect rect) {
        return new SKRect((float) Math.Round(rect.Left), (float) Math.Round(rect.Top), (float) Math.Round(rect.Right), (float) Math.Round(rect.Bottom));
    }

    public static SKRectI FloorAndCeilI(this SKRect rect) {
        return new SKRectI((int) Math.Floor(rect.Left), (int) Math.Floor(rect.Top), (int) Math.Ceiling(rect.Right), (int) Math.Ceiling(rect.Bottom));
    }

    public static SKRectI CeilAndFloorI(this SKRect rect) {
        return new SKRectI((int) Math.Ceiling(rect.Left), (int) Math.Ceiling(rect.Top), (int) Math.Floor(rect.Right), (int) Math.Floor(rect.Bottom));
    }

    public static SKRectI RoundI(this SKRect rect) {
        return new SKRectI((int) Math.Round(rect.Left), (int) Math.Round(rect.Top), (int) Math.Round(rect.Right), (int) Math.Round(rect.Bottom));
    }

    public static SKRect ClampMinMax(this SKRect rect, SKRect max) {
        if (rect.Left < max.Left)
            rect.Left = max.Left;
        if (rect.Top < max.Top)
            rect.Top = max.Top;
        if (rect.Right > max.Right)
            rect.Right = max.Right;
        if (rect.Bottom > max.Bottom)
            rect.Bottom = max.Bottom;
        return rect;
    }
}