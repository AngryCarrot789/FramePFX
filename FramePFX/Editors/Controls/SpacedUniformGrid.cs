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

using System.Windows.Controls;
using System.Windows;

namespace FramePFX.Editors.Controls
{
    public class SpacedUniformGrid : Panel
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SpacedUniformGrid), new PropertyMetadata(Orientation.Horizontal));
        public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register("Spacing", typeof(double), typeof(SpacedUniformGrid), new PropertyMetadata(0.0d));

        public Orientation Orientation {
            get => (Orientation) this.GetValue(OrientationProperty);
            set => this.SetValue(OrientationProperty, value);
        }

        public double Spacing {
            get => (double) this.GetValue(SpacingProperty);
            set => this.SetValue(SpacingProperty, value);
        }

        public SpacedUniformGrid()
        {
        }

        private static double GetTotalGap(int numElements, double spacing) => numElements < 2 ? 0d : ((numElements - 1) * spacing);

        private static Size GetSlotSizePerElement(Size constraint, Orientation orientation, int numVisible, double totalGap)
        {
            return orientation == Orientation.Horizontal ? new Size((constraint.Width - totalGap) / numVisible, constraint.Height) : new Size(constraint.Width, (constraint.Height - totalGap) / numVisible);
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.ComputeVisible(out int numVisible);
            Size availableSize = GetSlotSizePerElement(constraint, this.Orientation, numVisible, GetTotalGap(numVisible, this.Spacing));
            Size totalSize = new Size();
            UIElementCollection children = this.InternalChildren;
            for (int i = 0, count = children.Count; i < count; i++)
            {
                UIElement child = children[i];
                child.Measure(availableSize);
                Size desiredSize = child.DesiredSize;
                if (totalSize.Width < desiredSize.Width)
                    totalSize.Width = desiredSize.Width;
                if (totalSize.Height < desiredSize.Height)
                    totalSize.Height = desiredSize.Height;
            }

            return totalSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            this.ComputeVisible(out int numVisible);
            double spacing = this.Spacing;
            Orientation orientation = this.Orientation;
            Rect finalRect = new Rect(new Point(), GetSlotSizePerElement(arrangeSize, orientation, numVisible, GetTotalGap(numVisible, spacing)));
            UIElementCollection children = this.InternalChildren;
            for (int i = 0, count = children.Count; i < count; i++)
            {
                // when a child is collapsed, it may glitch the rendering if this panel
                // doesn't get re-arranged for some reason when a child's visibility changes
                UIElement child = this.InternalChildren[i];
                child.Arrange(finalRect);
                if (child.Visibility != Visibility.Collapsed)
                {
                    if (orientation == Orientation.Horizontal)
                    {
                        finalRect.X += finalRect.Width + spacing;
                    }
                    else
                    {
                        finalRect.Y += finalRect.Height + spacing;
                    }
                }
            }

            return arrangeSize;
        }

        private void ComputeVisible(out int count)
        {
            int visibleCount = 0;
            UIElementCollection children = this.InternalChildren;
            for (int i = 0, num = children.Count; i < num; i++)
            {
                if (children[i].Visibility != Visibility.Collapsed)
                {
                    ++visibleCount;
                }
            }

            count = visibleCount;
        }
    }
}