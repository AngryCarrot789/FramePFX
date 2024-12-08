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

using System.Numerics;
using SkiaSharp;

namespace FramePFX.Utils;

public static class Vectors
{
    public static Vector2 Zero => default;
    public static Vector2 One => default;
    public static Vector2 MinValue => new Vector2(float.MinValue);
    public static Vector2 MaxValue => new Vector2(float.MaxValue);

    public static Vector2 Clamp(this Vector2 a, Vector2 min, Vector2 max) => Vector2.Clamp(a, min, max);
    public static Vector3 Clamp(this Vector3 a, Vector3 min, Vector3 max) => Vector3.Clamp(a, min, max);
    public static Vector4 Clamp(this Vector4 a, Vector4 min, Vector4 max) => Vector4.Clamp(a, min, max);

    public static Vector2 Floor(this Vector2 vector) => new Vector2((float) Math.Floor(vector.X), (float) Math.Floor(vector.Y));
    public static Vector2 Ceil(this Vector2 vector) => new Vector2((float) Math.Ceiling(vector.X), (float) Math.Ceiling(vector.Y));
    public static Vector2 Round(this Vector2 vector, int digits) => new Vector2((float) Math.Round(vector.X, digits), (float) Math.Round(vector.Y, digits));

    public static Vector3 Round(this Vector3 vector, int digits) => new Vector3((float) Math.Round(vector.X, digits), (float) Math.Round(vector.Y, digits), (float) Math.Round(vector.Z, digits));

    public static bool IsPositiveInfinityX(in this Vector2 vector)
    {
        return float.IsPositiveInfinity(vector.X);
    }

    public static bool IsPositiveInfinityY(in this Vector2 vector)
    {
        return float.IsPositiveInfinity(vector.Y);
    }

    public static bool IsNegativeInfinityX(in this Vector2 vector)
    {
        return float.IsNegativeInfinity(vector.X);
    }

    public static bool IsNegativeInfinityY(in this Vector2 vector)
    {
        return float.IsNegativeInfinity(vector.Y);
    }

    public static Vector2 Lerp(in this Vector2 a, in Vector2 b, float blend)
    {
        return new Vector2(blend * (b.X - a.X) + a.X, blend * (b.Y - a.Y) + a.Y);
    }

    public static SKRect ToRectAsSize(in this Vector2 a, float left, float top)
    {
        return new SKRect(left, top, left + a.X, top + a.Y);
    }

    public static bool IsLessThan(in this Vector2 a, Vector2 b)
    {
        return a.X < b.X || a.Y < b.Y;
    }

    public static bool IsGreaterThan(in this Vector2 a, Vector2 b)
    {
        return a.X > b.X || a.Y > b.Y;
    }
}