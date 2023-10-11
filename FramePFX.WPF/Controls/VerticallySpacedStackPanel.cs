using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using FramePFX.WPF.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.WPF.Controls {
    public class VerticallySpacedStackPanel : Panel, IScrollInfo {
        private ScrollData _scrollData;

        public static readonly DependencyProperty GapHeightProperty = DependencyProperty.Register("GapHeight", typeof(double), typeof(VerticallySpacedStackPanel), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

        public bool CanHorizontallyScroll {
            get => this._scrollData != null && this._scrollData._allowHorizontal;
            set {
                this.EnsureScrollData();
                if (this._scrollData._allowHorizontal == value)
                    return;
                this._scrollData._allowHorizontal = value;
                this.InvalidateMeasure();
            }
        }

        public bool CanVerticallyScroll {
            get => this._scrollData != null && this._scrollData._allowVertical;
            set {
                this.EnsureScrollData();
                if (this._scrollData._allowVertical == value)
                    return;
                this._scrollData._allowVertical = value;
                this.InvalidateMeasure();
            }
        }

        public double ExtentWidth => this._scrollData?._extent.Width ?? 0.0;

        public double ExtentHeight => this._scrollData?._extent.Height ?? 0.0;

        public double ViewportWidth => this._scrollData?._viewport.Width ?? 0.0;

        public double ViewportHeight => this._scrollData?._viewport.Height ?? 0.0;

        public double HorizontalOffset => this._scrollData?._computedOffset.X ?? 0.0;

        public double VerticalOffset => this._scrollData?._computedOffset.Y ?? 0.0;

        public ScrollViewer ScrollOwner {
            get {
                this.EnsureScrollData();
                return this._scrollData._scrollOwner;
            }
            set {
                this.EnsureScrollData();
                if (value == this._scrollData._scrollOwner)
                    return;
                ResetScrolling(this);
                this._scrollData._scrollOwner = value;
            }
        }

        public double GapHeight {
            get => (double) this.GetValue(GapHeightProperty);
            set => this.SetValue(GapHeightProperty, value);
        }

        public VerticallySpacedStackPanel() {
        }

        #region IScrollInfo

        internal static double CoerceOffset(double offset, double extent, double viewport) {
            if (offset > extent - viewport)
                offset = extent - viewport;
            if (offset < 0.0)
                offset = 0.0;
            return offset;
        }

        private static void VerifyScrollingData(VerticallySpacedStackPanel measureElement, ScrollData scrollData, Size viewport, Size extent, Vector offset) {
            bool areClose = true & DoubleUtils.AreClose(viewport, scrollData.Viewport) & DoubleUtils.AreClose(extent, scrollData.Extent) & DoubleUtils.AreClose(offset, scrollData.ComputedOffset);
            scrollData.Offset = offset;
            if (areClose)
                return;
            scrollData.Viewport = viewport;
            scrollData.Extent = extent;
            scrollData.ComputedOffset = offset;
            measureElement.OnScrollChange();
        }

        private static double ComputePhysicalFromLogicalOffset(VerticallySpacedStackPanel arrangeElement, double logicalOffset) {
            double num = 0.0;
            UIElementCollection items = arrangeElement.InternalChildren;
            for (int index = 0; index < logicalOffset; ++index) {
                num -= items[index].DesiredSize.Height;
            }

            num += logicalOffset * (double) arrangeElement.GetValue(GapHeightProperty);
            return num;
        }

        internal static double ComputeScrollOffsetWithMinimalScroll(double topView, double bottomView, double topChild, double bottomChild) {
            bool a = DoubleUtils.LessThan(topChild, topView) && DoubleUtils.LessThan(bottomChild, bottomView);
            bool b = DoubleUtils.GreaterThan(bottomChild, bottomView) && DoubleUtils.GreaterThan(topChild, topView);
            bool c = bottomChild - topChild > bottomView - topView;
            if (((!a || c ? b & c ? 1 : 0 : 1) | 0) != 0)
                return topChild;
            if (!(a | b | false))
                return topView;
            return bottomChild - (bottomView - topView);
        }

        public void LineUp() => this.SetVerticalOffset(this.VerticalOffset - 1.0);

        public void LineDown() => this.SetVerticalOffset(this.VerticalOffset + 1.0);

        public void LineLeft() => this.SetHorizontalOffset(this.HorizontalOffset - 16.0);

        public void LineRight() => this.SetHorizontalOffset(this.HorizontalOffset + 16.0);

        public void PageUp() => this.SetVerticalOffset(this.VerticalOffset - this.ViewportHeight);

        public void PageDown() => this.SetVerticalOffset(this.VerticalOffset + this.ViewportHeight);

        public void PageLeft() => this.SetHorizontalOffset(this.HorizontalOffset - this.ViewportWidth);

        public void PageRight() => this.SetHorizontalOffset(this.HorizontalOffset + this.ViewportWidth);

        public void MouseWheelUp() {
            if (CanMouseWheelVerticallyScroll)
                this.SetVerticalOffset(this.VerticalOffset - SystemParameters.WheelScrollLines);
            else
                this.PageUp();
        }

        public void MouseWheelDown() {
            if (CanMouseWheelVerticallyScroll)
                this.SetVerticalOffset(this.VerticalOffset + SystemParameters.WheelScrollLines);
            else
                this.PageDown();
        }

        public void MouseWheelLeft() => this.SetHorizontalOffset(this.HorizontalOffset - 48D);

        public void MouseWheelRight() => this.SetHorizontalOffset(this.HorizontalOffset + 48D);

        public void SetHorizontalOffset(double offset) {
            this.EnsureScrollData();
            if (DoubleUtils.AreClose(offset, this._scrollData._offset.X))
                return;
            this._scrollData._offset.X = offset;
            this.InvalidateMeasure();
        }

        public void SetVerticalOffset(double offset) {
            this.EnsureScrollData();
            if (DoubleUtils.AreClose(offset, this._scrollData._offset.Y))
                return;
            this._scrollData._offset.Y = offset;
            this.InvalidateMeasure();
        }

        public Rect MakeVisible(Visual visual, Rect rectangle) {
            Vector newOffset = new Vector();
            Rect newRect = new Rect();
            if (rectangle.IsEmpty || visual == null || (visual == this || !this.IsAncestorOf(visual)))
                return Rect.Empty;
            rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
            if (!this.IsScrolling)
                return rectangle;
            this.MakeVisiblePhysicalHelper(rectangle, ref newOffset, ref newRect);
            this.MakeVisibleLogicalHelper(this.FindChildIndexThatParentsVisual(visual), ref newOffset, ref newRect);
            newOffset.X = CoerceOffset(newOffset.X, this._scrollData._extent.Width, this._scrollData._viewport.Width);
            newOffset.Y = CoerceOffset(newOffset.Y, this._scrollData._extent.Height, this._scrollData._viewport.Height);
            if (!DoubleUtils.AreClose(newOffset, this._scrollData._offset)) {
                this._scrollData._offset = newOffset;
                this.InvalidateMeasure();
                this.OnScrollChange();
            }

            return newRect;
        }

        #endregion

        protected override Size MeasureOverride(Size constraint) {
            Size size = new Size();
            UIElementCollection items = this.InternalChildren;
            Size availableSize = constraint;
            int num1 = -1;
            availableSize.Height = double.PositiveInfinity;
            if (this.IsScrolling && this.CanHorizontallyScroll)
                availableSize.Width = double.PositiveInfinity;
            int currScrollingOffset = this.IsScrolling ? CoerceOffsetToInteger(this._scrollData.Offset.Y, items.Count) : 0;
            double height = constraint.Height;
            int count = items.Count;
            for (int i = 0; i < count; ++i) {
                UIElement element = items[i];
                if (element != null) {
                    element.Measure(availableSize);
                    Size elemSize = element.DesiredSize;
                    size.Width = Math.Max(size.Width, elemSize.Width);
                    size.Height += elemSize.Height;
                    double num4 = elemSize.Height;
                    if (this.IsScrolling && num1 == -1 && i >= currScrollingOffset) {
                        height -= num4;
                        if (DoubleUtils.LessThanOrClose(height, 0.0))
                            num1 = i;
                    }
                }
            }

            if (count > 1) {
                size.Height += (count - 1) * this.GapHeight;
            }

            if (this.IsScrolling) {
                Size viewport = constraint;
                Size extent = size;
                Vector offset = this._scrollData.Offset;
                if (num1 == -1)
                    num1 = items.Count - 1;
                while (currScrollingOffset > 0) {
                    double num4 = height;
                    double num5 = num4 - items[currScrollingOffset - 1].DesiredSize.Height;
                    if (!DoubleUtils.LessThan(num5, 0.0)) {
                        --currScrollingOffset;
                        height = num5;
                    }
                    else
                        break;
                }

                int num6 = num1 - currScrollingOffset;
                if (num6 == 0 || DoubleUtils.GreaterThanOrClose(height, 0.0))
                    ++num6;
                this._scrollData.SetPhysicalViewport(viewport.Height);
                viewport.Height = num6;
                extent.Height = count;
                offset.Y = currScrollingOffset;
                offset.X = Math.Max(0.0, Math.Min(offset.X, extent.Width - viewport.Width));
                size.Width = Math.Min(size.Width, constraint.Width);
                size.Height = Math.Min(size.Height, constraint.Height);
                VerifyScrollingData(this, this._scrollData, viewport, extent, offset);
            }

            return size;
        }

        protected override Size ArrangeOverride(Size arrangeSize) {
            UIElementCollection internalChildren = this.InternalChildren;
            Rect finalRect = new Rect(arrangeSize);
            double num = 0d;
            double gap = this.GapHeight;
            if (this.IsScrolling) {
                finalRect.X = -1.0 * this._scrollData.ComputedOffset.X;
                finalRect.Y = ComputePhysicalFromLogicalOffset(this, this._scrollData.ComputedOffset.Y);
            }

            for (int i = 0, count = internalChildren.Count; i < count; ++i) {
                UIElement uiElement = internalChildren[i];
                if (uiElement != null) {
                    finalRect.Y += num;
                    num = uiElement.DesiredSize.Height;
                    finalRect.Height = num;
                    finalRect.Width = Math.Max(arrangeSize.Width, uiElement.DesiredSize.Width);
                    uiElement.Arrange(finalRect);
                    finalRect.Y += gap;
                }
            }

            return arrangeSize;
        }

        private void EnsureScrollData() {
            if (this._scrollData != null)
                return;
            this._scrollData = new ScrollData();
        }

        private static void ResetScrolling(VerticallySpacedStackPanel element) {
            element.InvalidateMeasure();
            if (!element.IsScrolling)
                return;
            element._scrollData.ClearLayout();
        }

        private void OnScrollChange() {
            if (this.ScrollOwner == null)
                return;
            this.ScrollOwner.InvalidateScrollInfo();
        }

        private int FindChildIndexThatParentsVisual(Visual child) {
            DependencyObject reference = child;
            DependencyObject parent = VisualTreeHelper.GetParent(child);
            while (parent != this) {
                reference = parent;
                parent = VisualTreeHelper.GetParent(reference);
                if (parent == null)
                    throw new Exception("Visual is in a different visual tree");
            }

            return this.Children.IndexOf((UIElement) reference);
        }

        private void MakeVisiblePhysicalHelper(Rect r, ref Vector newOffset, ref Rect newRect) {
            double topView = this._scrollData._computedOffset.X;
            double num1 = this.ViewportWidth;
            double num2 = r.X;
            double num3 = r.Width;

            double num4 = num2 + topView;
            double withMinimalScroll = ComputeScrollOffsetWithMinimalScroll(topView, topView + num1, num4, num4 + num3);
            double num5 = Math.Max(num4, withMinimalScroll);
            double num6 = Math.Max(Math.Min(num3 + num4, withMinimalScroll + num1) - num5, 0.0);
            double num7 = num5 - topView;
            newOffset.X = withMinimalScroll;
            newRect.X = num7;
            newRect.Width = num6;
        }

        private void MakeVisibleLogicalHelper(int childIndex, ref Vector newOffset, ref Rect newRect) {
            double num1 = 0.0;
            int num2 = (int) this._scrollData._computedOffset.Y;
            int num3 = (int) this._scrollData._viewport.Height;
            int num4 = num2;
            if (childIndex < num2) {
                num4 = childIndex;
            }
            else if (childIndex > num2 + num3 - 1) {
                Size desiredSize = this.InternalChildren[childIndex].DesiredSize;
                double num5 = desiredSize.Height;
                double num6 = this._scrollData._physicalViewport - num5;
                int index;
                for (index = childIndex; index > 0 && DoubleUtils.GreaterThanOrClose(num6, 0.0); num6 -= num5) {
                    --index;
                    desiredSize = this.InternalChildren[index].DesiredSize;
                    num5 = desiredSize.Height;
                    num1 += num5;
                }

                if (index != childIndex && DoubleUtils.LessThan(num6, 0.0)) {
                    num1 -= num5;
                    ++index;
                }

                num4 = index;
            }

            newOffset.Y = num4;
            newRect.Y = num1;
            newRect.Height = this.InternalChildren[childIndex].DesiredSize.Height;
        }

        private static int CoerceOffsetToInteger(double offset, int numberOfItems) {
            int num;
            if (double.IsNegativeInfinity(offset))
                num = 0;
            else if (double.IsPositiveInfinity(offset)) {
                num = numberOfItems - 1;
            }
            else {
                int val2 = (int) offset;
                num = Math.Max(Math.Min(numberOfItems - 1, val2), 0);
            }

            return num;
        }

        private bool IsScrolling => this._scrollData?._scrollOwner != null;

        private static bool CanMouseWheelVerticallyScroll => SystemParameters.WheelScrollLines > 0;

        internal class ScrollData {
            internal bool _allowHorizontal;
            internal bool _allowVertical;
            internal Vector _offset;
            internal Vector _computedOffset = new Vector(0.0, 0.0);
            internal Size _viewport;
            internal Size _extent;
            internal double _physicalViewport;
            internal ScrollViewer _scrollOwner;

            internal void ClearLayout() {
                this._offset = new Vector();
                this._viewport = this._extent = new Size();
                this._physicalViewport = 0.0;
            }

            public Vector Offset {
                get => this._offset;
                set => this._offset = value;
            }

            public Size Viewport {
                get => this._viewport;
                set => this._viewport = value;
            }

            public Size Extent {
                get => this._extent;
                set => this._extent = value;
            }

            public Vector ComputedOffset {
                get => this._computedOffset;
                set => this._computedOffset = value;
            }

            public void SetPhysicalViewport(double value) => this._physicalViewport = value;
        }
    }
}