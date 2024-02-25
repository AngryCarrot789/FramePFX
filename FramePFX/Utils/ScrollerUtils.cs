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
using FramePFX.Utils.Visuals;
using Rect = System.Windows.Rect;

namespace FramePFX.Utils {
    public static class ScrollerUtils {
        /// <summary>
        /// Calculates the amount of drawing space the element should use to draw
        /// </summary>
        /// <param name="element">The element which is being drawn</param>
        /// <param name="relativeTo">An element higher in the sub-tree (a parent of element). If this is a scroll viewer, more calculations are done using the offsets</param>
        /// <returns>A rectangle of space that is visible on screen, relative to the <paramref name="relativeTo"/> parameter</returns>
        public static Rect GetDrawingBounds(this FrameworkElement element, UIElement relativeTo) {
            if (relativeTo == null) {
                if ((relativeTo = VisualTreeUtils.GetParent<ScrollViewer>(element)) == null) {
                    return new Rect(new Point(), element.RenderSize);
                }
            }

            Vector location = new Point() - element.TranslatePoint(new Point(), relativeTo);
            if (relativeTo is ScrollViewer scrollViewer) {
                Rect viewport = new Rect(scrollViewer.HorizontalOffset, scrollViewer.VerticalOffset, scrollViewer.ViewportWidth, scrollViewer.ViewportHeight);
                double left = Maths.Clamp(viewport.Left, location.X, viewport.Right);
                double top = Maths.Clamp(viewport.Top, location.Y, viewport.Bottom);
                double right = Math.Min(viewport.Right, location.X + element.ActualWidth);
                double bottom = Math.Min(viewport.Bottom, location.Y + element.ActualHeight);
                return new Rect(left, top, Math.Max(0, right - left), Math.Max(0, bottom - top));
            }
            else {
                return new Rect((Point) location, element.RenderSize);
            }
        }
    }
}