using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Core.Utils;
using FramePFX.Shortcuts;
using Rect = System.Windows.Rect;

namespace FramePFX.Controls {
    public class Ruler : Control {
        private ScrollViewer scroller;

        public Ruler() {
            this.Loaded += (sender, args) => {
                this.scroller = VisualTreeUtils.FindVisualParent<ScrollViewer>(this);
                if (this.scroller == null) {
                    return;
                }

                this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
                this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
            };

            this.Unloaded += (sender, args) => {
                if (this.scroller != null) {
                    this.scroller.ScrollChanged -= this.OnScrollerOnScrollChanged;
                    this.scroller.SizeChanged -= this.OnScrollerOnSizeChanged;
                }
            };
        }

        private void OnScrollerOnSizeChanged(object o, SizeChangedEventArgs e) {
            this.ScheduleRender();
        }

        private void OnScrollerOnScrollChanged(object o, ScrollChangedEventArgs e) {
            this.ScheduleRender();
        }

        private void ScheduleRender() {
            this.Dispatcher.Invoke(this.InvalidateVisual);
        }

        protected override void ParentLayoutInvalidated(UIElement child) {
            base.ParentLayoutInvalidated(child);
            this.Dispatcher.InvokeAsync(this.InvalidateVisual, DispatcherPriority.ContextIdle);
        }

        /// <summary>
        /// Gets the amount of drawing space this ruler should use to draw
        /// </summary>
        /// <param name="availableSize">
        /// An out parameter for the amount of actual space that was available.
        /// By default, it defaults to the width/height of the return value, however, if
        /// this ruler is placed somewhere in a scroll viewer, then it returns the total amount of space available by the scrollviewer</param>
        /// <returns></returns>
        public Rect GetDrawingBounds(out Size availableSize) {
            availableSize = this.RenderSize;
            if (this.scroller != null) {
                double offsetX = this.scroller.HorizontalOffset, offsetY = this.scroller.VerticalOffset;
                // availableSize = new Size(this.scroller.ExtentWidth, this.scroller.ExtentHeight);
                return new Rect(offsetX, offsetY, Math.Min(this.scroller.ViewportWidth, this.ActualWidth), Math.Min(this.scroller.ViewportHeight, this.ActualHeight));
            }
            else {
                return new Rect(new Point(), this.RenderSize);
            }
        }

        protected override Size MeasureOverride(Size constraint) {
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeBounds) {
            return base.ArrangeOverride(arrangeBounds);
        }

        protected override void OnRender(DrawingContext dc) {
            Rect rect = this.GetDrawingBounds(out Size size);
            Rect bgRect = rect;
            bgRect.Inflate(-2, -2);
            // dc.DrawRectangle(null, new Pen(Brushes.Black, 1), rect);
            // dc.DrawRectangle(Brushes.Green, null, bgRect);

            const int steps = 10;
            double offset = Ceil(rect.X, steps) - rect.X;

            Pen pen = new Pen(Brushes.Orange, 1d);

            double ratio = size.Width / rect.Width;
            double normalizeMinSize = 1 / this.ActualWidth;
            double minStepValue = normalizeMinSize * 10000d;
            int minStepValueMagnitude = (int) Math.Floor(Math.Log10(minStepValue));
            double normalizeMinStepValue = minStepValue / Math.Pow(10, minStepValueMagnitude);
            int normalizeRealStepValue = new List<int>() {1, 2, 5, 10, 50, 100, 500}.Union(new[] {10}).First(x => x > normalizeMinStepValue);
            double realStepValue = normalizeRealStepValue * Math.Pow(10, minStepValueMagnitude);
            double pixelStep = size.Width * realStepValue / 10000d;
            double subPixelStep = (pixelStep / 10) * ratio;
            double subOffset;
            // int i = 0;
            // do {
            //     subOffset = offset + (i * subPixelStep);
            //     subOffset += rect.X;
            //     dc.DrawLine(pen, new Point(subOffset, 0), new Point(subOffset, size.Height));
            //     i++;
            // } while (subOffset < rect.Right);

            base.OnRender(dc);
        }

        public static int Ceil(int value, int multiple) {
            int mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }

        public static double Ceil(double value, int multiple) {
            double mod = value % multiple;
            return mod == 0 ? value : value + (multiple - mod);
        }
    }
}