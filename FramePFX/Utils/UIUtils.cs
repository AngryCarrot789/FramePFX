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

using System;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Utils {
    public static class UIUtils {
        public static Rect GetVisibleRect(ScrollViewer scroller, UIElement element) {
            Rect rect;
            Size size = element.RenderSize;
            if (scroller == null) {
                rect = new Rect(0, 0, size.Width, size.Height);
            }
            else {
                Point position = element.TranslatePoint(new Point(), scroller);
                double vpBeginX = scroller.HorizontalOffset;
                double vpBeginY = scroller.VerticalOffset;
                // bug: when you maximize a window, these 2 don't update in time
                // so ViewportWidth might be say 500 but the RenderSize width will be maybe 1920 :/
                double vpEndX = vpBeginX + scroller.ViewportWidth;
                double vpEndY = vpBeginY + scroller.ViewportHeight;
                double absPosX = vpBeginX + position.X;
                double absPosY = vpBeginY + position.Y;
                double absEndX = absPosX + size.Width;
                double absEndY = absPosY + size.Height;
                if (vpBeginX > absEndX || vpEndX < absPosX || vpBeginY > absEndY || vpEndY < absPosY) {
                    rect = new Rect();
                }
                else {
                    double x1 = Math.Max(vpBeginX, absPosX);
                    double y1 = Math.Max(vpBeginY, absPosY);
                    double x2 = Math.Min(vpEndX, absEndX);
                    double y2 = Math.Min(vpEndY, absEndY);
                    rect = new Rect(x1 - absPosX, y1 - absPosY, x2 - x1, y2 - y1);
                }
            }

            return rect;
        }

        public static Rect GetVisibleRect(ScrollViewer scroller, Point position, Size size) {
            Rect rect;
            double r1L = scroller.HorizontalOffset;
            double r1T = scroller.VerticalOffset;
            double r1R = r1L + scroller.ViewportWidth;
            double r1B = r1T + scroller.ViewportHeight;
            double r2L = r1L + position.X;
            double r2T = r1T + position.Y;
            double r2R = r2L + size.Width;
            double r2B = r2T + size.Height;
            if (r1L > r2R || r1R < r2L || r1T > r2B || r1B < r2T) {
                rect = new Rect();
            }
            else {
                double x1 = Math.Max(r1L, r2L);
                double y1 = Math.Max(r1T, r2T);
                double x2 = Math.Min(r1R, r2R);
                double y2 = Math.Min(r1B, r2B);
                rect = new Rect(x1 - r2L, y1 - r2T, x2 - x1, y2 - y1);
            }

            return rect;
        }

        // I think this one might throw an exception in rare cases, whereas the
        // method above checks for all possible error cases, and in those cases,
        // just returns a new blank Rect
        public static Rect GetVisibleRectUnsafe(ScrollViewer scroller, UIElement element) {
            Size size = element.RenderSize;
            if (scroller == null) {
                return new Rect(new Point(), size);
            }
            else {
                double width = Math.Min(scroller.ViewportWidth, size.Width);
                double height = Math.Min(scroller.ViewportHeight, size.Height);
                return new Rect(scroller.HorizontalOffset, scroller.VerticalOffset, width, height);
            }
        }
    }
}