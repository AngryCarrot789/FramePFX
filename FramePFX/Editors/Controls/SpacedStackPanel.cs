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

namespace FramePFX.Editors.Controls
{
    public class SpacedStackPanel : Panel
    {
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                nameof(Orientation),
                typeof(Orientation),
                typeof(SpacedStackPanel),
                new FrameworkPropertyMetadata(Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsMeasure, OnOrientationChanged),
                o =>
                {
                    Orientation orientation = (Orientation) o;
                    return orientation == Orientation.Horizontal || orientation == Orientation.Vertical;
                });

        public static readonly DependencyProperty InterElementGapProperty = DependencyProperty.Register("InterElementGap", typeof(double), typeof(SpacedStackPanel), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public Orientation Orientation {
            get => (Orientation) this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public double InterElementGap {
            get => (double) this.GetValue(InterElementGapProperty);
            set => this.SetValue(InterElementGapProperty, value);
        }

        protected override bool HasLogicalOrientation => true;

        protected override Orientation LogicalOrientation => this.Orientation;

        public SpacedStackPanel()
        {
        }

        protected override Size MeasureOverride(Size constraint)
        {
            Size totalSize = new Size();
            UIElementCollection items = this.InternalChildren;
            int itemCount = items.Count;
            if (itemCount < 1)
            {
                return totalSize;
            }

            Size availableSize = constraint;
            bool isHorizontal = this.Orientation == Orientation.Horizontal;
            if (isHorizontal)
            {
                availableSize.Width = double.PositiveInfinity;
            }
            else
            {
                availableSize.Height = double.PositiveInfinity;
            }

            double offset = 0.0, theGap = this.InterElementGap;
            for (int i = 0; i < itemCount; ++i)
            {
                UIElement element = items[i];
                if (element == null || element.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                element.Measure(availableSize);
                Size desiredSize = element.DesiredSize;
                if (isHorizontal)
                {
                    totalSize.Width += desiredSize.Width + offset;
                    totalSize.Height = Math.Max(totalSize.Height, desiredSize.Height);
                }
                else
                {
                    totalSize.Width = Math.Max(totalSize.Width, desiredSize.Width);
                    totalSize.Height += desiredSize.Height + offset;
                }

                offset = theGap;
            }

            return totalSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElementCollection items = this.InternalChildren;
            int count = items.Count;
            bool isHorizontal = this.Orientation == Orientation.Horizontal;
            Rect finalRect = new Rect(arrangeSize);
            double number = 0.0;
            double offset = 0.0, theGap = this.InterElementGap;
            for (int i = 0; i < count; ++i)
            {
                UIElement element = items[i];
                if (element == null || element.Visibility == Visibility.Collapsed)
                {
                    continue;
                }

                if (isHorizontal)
                {
                    finalRect.X += number + offset;
                    number = element.DesiredSize.Width;
                    finalRect.Width = number;
                    finalRect.Height = Math.Max(arrangeSize.Height, element.DesiredSize.Height);
                }
                else
                {
                    finalRect.Y += number + offset;
                    number = element.DesiredSize.Height;
                    finalRect.Height = number;
                    finalRect.Width = Math.Max(arrangeSize.Width, element.DesiredSize.Width);
                }

                element.Arrange(finalRect);
                offset = theGap;
            }

            return arrangeSize;
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) => ((SpacedStackPanel) d).InvalidateMeasure();
    }
}