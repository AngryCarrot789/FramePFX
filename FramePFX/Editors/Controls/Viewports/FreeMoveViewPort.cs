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

using FramePFX.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;

namespace FramePFX.Editors.Controls.Viewports
{
    public class FreeMoveViewPort : Border
    {
        private static readonly object ZeroDoubleBoxed = 0d;

        #region Dependency Properties

        public static readonly DependencyProperty MinimumZoomScaleProperty =
            DependencyProperty.Register(
                "MinimumZoomScale",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    0.05d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnMinimumZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceMinimumZoom));

        public static readonly DependencyProperty MaximumZoomScaleProperty =
            DependencyProperty.Register(
                "MaximumZoomScale",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    double.PositiveInfinity,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnMaximumZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceMaximumZoom));

        public static readonly DependencyProperty ZoomScaleProperty =
            DependencyProperty.Register(
                "ZoomScale",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceZoom));

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(
                "HorizontalOffset",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    ZeroDoubleBoxed,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnHorizontalOffsetChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceHorizontalOffset));

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(
                "VerticalOffset",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    ZeroDoubleBoxed,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnVerticalOffsetChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceVerticalOffset));

        public static readonly DependencyProperty PanToCursorOnUserZoomProperty =
            DependencyProperty.Register(
                "PanToCursorOnUserZoom",
                typeof(bool),
                typeof(FreeMoveViewPort),
                new PropertyMetadata(BoolBox.True));

        #endregion

        public double MinimumZoomScale
        {
            get => (double) this.GetValue(MinimumZoomScaleProperty);
            set => this.SetValue(MinimumZoomScaleProperty, value);
        }

        public double MaximumZoomScale
        {
            get => (double) this.GetValue(MaximumZoomScaleProperty);
            set => this.SetValue(MaximumZoomScaleProperty, value);
        }

        public double ZoomScale
        {
            get => (double) this.GetValue(ZoomScaleProperty);
            set => this.SetValue(ZoomScaleProperty, value);
        }

        public double HorizontalOffset
        {
            get => (double) this.GetValue(HorizontalOffsetProperty);
            set => this.SetValue(HorizontalOffsetProperty, value);
        }

        public double VerticalOffset
        {
            get => (double) this.GetValue(VerticalOffsetProperty);
            set => this.SetValue(VerticalOffsetProperty, value);
        }

        /// <summary>
        /// Gets or sets a value which indicates whether this control should try to pan towards the user's cursor when they zoom in or out
        /// </summary>
        public bool PanToCursorOnUserZoom
        {
            get => (bool) this.GetValue(PanToCursorOnUserZoomProperty);
            set => this.SetValue(PanToCursorOnUserZoomProperty, value.Box());
        }

        protected override int VisualChildrenCount => 1;

        protected override IEnumerator LogicalChildren => (this.InternalChild == null ? new List<object>() : new List<object>() {this.InternalChild}).GetEnumerator();

        private ContainerVisual InternalVisual
        {
            get
            {
                if (this._internalVisual == null)
                {
                    this._internalVisual = new ContainerVisual();
                    this.AddVisualChild(this._internalVisual);
                }

                return this._internalVisual;
            }
        }

        private UIElement InternalChild
        {
            get
            {
                VisualCollection children = this.InternalVisual.Children;
                return children.Count != 0 ? children[0] as UIElement : null;
            }
            set
            {
                VisualCollection children = this.InternalVisual.Children;
                if (children.Count != 0)
                    children.Clear();
                children.Add(value);
            }
        }

        private Transform InternalTransform
        {
            get => this.InternalVisual.Transform;
            set => this.InternalVisual.Transform = value;
        }

        public override UIElement Child
        {
            get => this.InternalChild;
            set
            {
                UIElement internalChild = this.InternalChild;
                if (internalChild == value)
                    return;
                this.RemoveLogicalChild(internalChild);
                if (value != null)
                    this.AddLogicalChild(value);
                this.InternalChild = value;
                this.InvalidateMeasure();
            }
        }

        private readonly ScaleTransform scaleTransform;
        private ContainerVisual _internalVisual;
        private Point lastMousePoint = new Point();

        public FreeMoveViewPort()
        {
            this.Loaded += this.OnLoaded;
            this.PreviewMouseWheel += this.OnPreviewMouseWheel;
            this.Background = Brushes.Transparent;
            this.InternalTransform = this.scaleTransform = new ScaleTransform(1d, 1d);
        }

        private void OnMinimumZoomChanged(double oldValue, double newValue)
        {
        }

        private void OnMaximumZoomChanged(double oldValue, double newValue)
        {
        }

        private void OnZoomChanged(double oldValue, double newValue)
        {
        }

        private void OnHorizontalOffsetChanged(double oldValue, double newValue)
        {
        }

        private void OnVerticalOffsetChanged(double oldValue, double newValue)
        {
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                this.FitContentToCenter();
            }, DispatcherPriority.Background);
        }

        public void FitContentToCenter()
        {
            UIElement child = this.InternalChild;
            if (child == null)
            {
                return;
            }

            this.HorizontalOffset = 0;
            this.VerticalOffset = 0;
            this.ZoomScale = 0;

            this.UpdateLayout();

            Size childSize = child.DesiredSize;
            double ratioW = this.ActualWidth / childSize.Width;
            double ratioH = this.ActualHeight / childSize.Height;
            if (ratioH < ratioW && childSize.Height > 0)
            {
                this.ZoomScale = this.ActualHeight / childSize.Height;
            }
            else if (childSize.Width > 0)
            {
                this.ZoomScale = this.ActualWidth / childSize.Width;
            }
            else
            {
                this.ZoomScale = 1;
            }
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ModifierKeys modifiers = Keyboard.Modifiers;
            if (!(e.Delta < 0d || e.Delta > 0d))
            {
                return;
            }

            if ((modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) != 0)
            {
                // zoom in or out
                // I spent so much time trying to get this working myself but could never figure it out,
                // but this post worked pretty much first try. Smh my head.
                // I changed a few things though, like flipping zoom
                // https://gamedev.stackexchange.com/a/182177/160952
                double oldzoom = this.ZoomScale;
                double newzoom = oldzoom * (e.Delta > 0 ? 1.1 : 0.9);
                this.ZoomScale = newzoom;
                if (this.PanToCursorOnUserZoom)
                {
                    newzoom = this.ZoomScale;
                    Size size = new Size(this.ActualWidth, this.ActualHeight);
                    Point pos = e.GetPosition(this);
                    double pixels_difference_w = (size.Width / oldzoom) - (size.Width / newzoom);
                    double side_ratio_x = (pos.X - (size.Width / 2)) / size.Width;
                    this.HorizontalOffset -= pixels_difference_w * side_ratio_x;
                    double pixels_difference_h = (size.Height / oldzoom) - (size.Height / newzoom);
                    double side_ratio_h = (pos.Y - (size.Height / 2)) / size.Height;
                    this.VerticalOffset -= pixels_difference_h * side_ratio_h;
                }
            }
            else if ((modifiers & ModifierKeys.Shift) != 0)
            {
                // horizontally offset
                this.HorizontalOffset += (e.Delta / 120d) * (1d / this.ZoomScale) * 20d;
            }
            else
            {
                // vertically offset
                this.VerticalOffset += (e.Delta / 120d) * (1d / this.ZoomScale) * 20d;
            }

            e.Handled = true;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.lastMousePoint = e.GetPosition(this);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            Point mousePoint = e.GetPosition(this);
            if (e.MiddleButton == MouseButtonState.Pressed || (e.LeftButton == MouseButtonState.Pressed && (Keyboard.Modifiers & ModifierKeys.Alt) != 0))
            {
                if (!this.IsMouseCaptured)
                {
                    this.CaptureMouse();
                }

                Vector change = mousePoint - this.lastMousePoint;
                this.HorizontalOffset += change.X / this.ZoomScale;
                this.VerticalOffset += change.Y / this.ZoomScale;
            }
            else
            {
                this.ReleaseMouseCapture();
            }

            this.lastMousePoint = mousePoint;
        }

        #region Measure and Arrangement

        protected override Size MeasureOverride(Size constraint)
        {
            Size size = new Size();
            if (this.InternalChild is UIElement child)
            {
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            }

            return size;
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            UIElement child = this.InternalChild;
            if (child != null)
            {
                Size desired = child.DesiredSize;
                double left = ((arrangeSize.Width - desired.Width) / 2) + this.HorizontalOffset;
                double top = ((arrangeSize.Height - desired.Height) / 2) + this.VerticalOffset;
                // this.InternalTransform = new ScaleTransform(this.ZoomScale, this.ZoomScale, arrangeSize.Width / 2d, arrangeSize.Height / 2d);

                double zoom = this.ZoomScale;
                this.scaleTransform.ScaleX = zoom;
                this.scaleTransform.ScaleY = zoom;
                this.scaleTransform.CenterX = arrangeSize.Width / 2d;
                this.scaleTransform.CenterY = arrangeSize.Height / 2d;

                // Size visualSize = new Size(desired.Width * this.ZoomScale, desired.Height * this.ZoomScale);
                // if (visualSize.Width > arrangeSize.Width) {
                //     double diff = visualSize.Width - arrangeSize.Width;
                //     left -= (diff / 4d);
                // }
                // if (visualSize.Height > arrangeSize.Height) {
                //     double diff = visualSize.Height - arrangeSize.Height;
                //     top -= (diff / 4d);
                // }

                child.Arrange(new Rect(new Point(left, top), desired));
            }

            return arrangeSize;
        }

        #endregion

        #region Visual children and stuff

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "index out of range: " + index);
            return this.InternalVisual;
        }

        #endregion

        #region Dependency property handlers

        private static object CoerceMinimumZoom(DependencyObject port, object value)
        {
            return value is double min ? (min < 0d ? ZeroDoubleBoxed : value) : ZeroDoubleBoxed;
        }

        private static object CoerceMaximumZoom(DependencyObject port, object value)
        {
            object minimum = port.GetValue(MinimumZoomScaleProperty);
            if (!(value is double max))
                return ZeroDoubleBoxed;
            if (max < 0d)
                return ZeroDoubleBoxed;
            return max < (double) minimum ? minimum : value;
        }

        private static object CoerceZoom(DependencyObject port, object value)
        {
            object min = port.GetValue(MinimumZoomScaleProperty);
            if (!(value is double scale) || scale < 0d || scale < (double) min)
                return min;
            object max = port.GetValue(MaximumZoomScaleProperty);
            return scale > (double) max ? max : value;
        }

        private static object CoerceHorizontalOffset(DependencyObject port, object value)
        {
            return value;
        }

        private static object CoerceVerticalOffset(DependencyObject port, object value)
        {
            return value;
        }

        private static void OnMinimumZoomChanged(FreeMoveViewPort port, double oldValue, double newValue)
        {
            port.CoerceValue(MaximumZoomScaleProperty);
            port.CoerceValue(ZoomScaleProperty);
            port.OnMinimumZoomChanged(oldValue, newValue);
        }

        private static void OnMaximumZoomChanged(FreeMoveViewPort port, double oldValue, double newValue)
        {
            port.CoerceValue(ZoomScaleProperty);
            port.OnMaximumZoomChanged(oldValue, newValue);
        }

        private static void OnZoomChanged(FreeMoveViewPort port, double oldValue, double newValue)
        {
            port.OnZoomChanged(oldValue, newValue);
        }

        private static void OnHorizontalOffsetChanged(FreeMoveViewPort port, double oldValue, double newValue)
        {
            port.OnHorizontalOffsetChanged(oldValue, newValue);
        }

        private static void OnVerticalOffsetChanged(FreeMoveViewPort port, double oldValue, double newValue)
        {
            port.OnVerticalOffsetChanged(oldValue, newValue);
        }

        #endregion
    }
}