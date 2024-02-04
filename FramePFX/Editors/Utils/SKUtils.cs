using System;
using System.Numerics;
using System.Windows;
using SkiaSharp;

namespace FramePFX.Editors.Utils {
    public static class SKUtils {
        public static SKPoint AsSkia(this Point point) {
            return new SKPoint((float) point.X, (float) point.Y);
        }

        public static SKRect AsSkia(this Rect rect) {
            return new SKRect((float) rect.Left, (float) rect.Top, (float) rect.Right, (float) rect.Bottom);
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
    }
}