using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xaml;
using FramePFX.Core.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Controls {
    public class FreeMoveViewPort : Decorator {
        private static readonly object ZeroDoubleObject = 0d;
        private static readonly object DefaultMinZoomObject = 0.1d;
        private static readonly object DefaultZoomObject = 1d;
        private static readonly object DefaultMaxZoomObject = double.PositiveInfinity;

        #region Dependency Properties

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(
                "Background",
                typeof(Brush),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty MinimumZoomScaleProperty =
            DependencyProperty.Register(
                "MinimumZoomScale",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    DefaultMinZoomObject,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnMinimumZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceMinimumZoom));

        public static readonly DependencyProperty MaximumZoomScaleProperty =
            DependencyProperty.Register(
                "MaximumZoomScale",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(
                    DefaultMaxZoomObject,
                    FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => OnMaximumZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                    CoerceMaximumZoom));

        public static readonly DependencyProperty ZoomScaleProperty = DependencyProperty.Register(
            "ZoomScale",
            typeof(double),
            typeof(FreeMoveViewPort),
            new FrameworkPropertyMetadata(
                DefaultZoomObject,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                (d, e) => OnZoomChanged((FreeMoveViewPort) d, (double) e.OldValue, (double) e.NewValue),
                CoerceZoom));

        public static readonly DependencyProperty HorizontalOffsetProperty =
            DependencyProperty.Register(
                "HorizontalOffset",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(ZeroDoubleObject, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public static readonly DependencyProperty VerticalOffsetProperty =
            DependencyProperty.Register(
                "VerticalOffset",
                typeof(double),
                typeof(FreeMoveViewPort),
                new FrameworkPropertyMetadata(ZeroDoubleObject, FrameworkPropertyMetadataOptions.AffectsMeasure));

        #endregion

        [Category("Appearance")]
        public Brush Background {
            get => (Brush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        public double MinimumZoomScale {
            get => (double) this.GetValue(MinimumZoomScaleProperty);
            set => this.SetValue(MinimumZoomScaleProperty, value);
        }

        public double MaximumZoomScale {
            get => (double) this.GetValue(MaximumZoomScaleProperty);
            set => this.SetValue(MaximumZoomScaleProperty, value);
        }

        public double ZoomScale {
            get => (double) this.GetValue(ZoomScaleProperty);
            set => this.SetValue(ZoomScaleProperty, value);
        }

        public double HorizontalOffset {
            get => (double) this.GetValue(HorizontalOffsetProperty);
            set => this.SetValue(HorizontalOffsetProperty, value);
        }

        public double VerticalOffset {
            get => (double) this.GetValue(VerticalOffsetProperty);
            set => this.SetValue(VerticalOffsetProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override IEnumerator LogicalChildren => (this.InternalChild == null ? new List<object>() : new List<object>() {this.InternalChild}).GetEnumerator();

        private ContainerVisual _internalVisual;
        private ContainerVisual InternalVisual {
            get {
                if (this._internalVisual == null) {
                    this._internalVisual = new ContainerVisual();
                    this.AddVisualChild(this._internalVisual);
                }

                return this._internalVisual;
            }
        }

        private UIElement InternalChild {
            get {
                VisualCollection children = this.InternalVisual.Children;
                return children.Count != 0 ? children[0] as UIElement : null;
            }
            set {
                VisualCollection children = this.InternalVisual.Children;
                if (children.Count != 0)
                    children.Clear();
                children.Add(value);
            }
        }

        private Transform InternalTransform {
            get => this.InternalVisual.Transform;
            set => this.InternalVisual.Transform = value;
        }

        public override UIElement Child {
            get => this.InternalChild;
            set {
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

        public FreeMoveViewPort() {
            this.Loaded += this.OnLoaded;
            this.PreviewMouseWheel += this.OnPreviewMouseWheel;
        }

        private void OnMinimumZoomChanged(double oldValue, double newValue) {

        }

        private void OnMaximumZoomChanged(double oldValue, double newValue) {

        }

        private void OnZoomChanged(double oldValue, double newValue) {

        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            UIElement child = this.InternalChild;
            if (child == null) {
                return;
            }

            Size size = child.DesiredSize;
            double scale = this.ZoomScale;

            Size mySize = this.DesiredSize;

            double cX = (mySize.Width / 2d) - (size.Width / 2d);
            double cY = (mySize.Height / 2d) - (size.Height / 2d);

            this.HorizontalOffset = cX / scale;
            this.VerticalOffset = cY / scale;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e) {
            ModifierKeys modifiers = Keyboard.Modifiers;
            if (!(e.Delta < 0d || e.Delta > 0d)) {
                return;
            }

            if ((modifiers & (ModifierKeys.Alt | ModifierKeys.Control)) != 0) { // zoom in or out
                double change = e.Delta / 120d;
                if ((modifiers & ModifierKeys.Alt) != 0) {
                    change *= 0.5d;
                }

                double oldZoom = this.ZoomScale;
                this.ZoomScale = oldZoom + change;
                double newZoom = this.ZoomScale;
                if (Maths.Equals(oldZoom, newZoom)) {
                    return; // hit the minimum or maximum zoom
                }

                Point mouse = e.GetPosition(this);

                double offsetX = this.HorizontalOffset;
                double offsetY = this.VerticalOffset;

                Size size = this.DesiredSize;
                offsetX += ((size.Width / 2d) - mouse.X);
                offsetY += ((size.Height / 2d) - mouse.Y);

                this.Dispatcher.Invoke(() => {
                    this.HorizontalOffset = offsetX * this.ZoomScale;
                    this.VerticalOffset = offsetY * this.ZoomScale;
                }, DispatcherPriority.Background);
            }
            else if ((modifiers & ModifierKeys.Shift) != 0) { // horizontally offset
                this.HorizontalOffset += e.Delta / 12d;
            }
            else { // vertically offset
                this.VerticalOffset += e.Delta / 12d;
            }

            e.Handled = true;
        }

        #region Measure and Arrangement

        protected override Size MeasureOverride(Size constraint) {
            UIElement child = this.InternalChild;
            Size availableSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
            if (child != null) {
                child.Measure(availableSize);
            }
            return availableSize;
        }

        protected override Size ArrangeOverride(Size arrangeSize) {
            UIElement child = this.InternalChild;
            if (child != null) {
                double scale = this.ZoomScale;
                double x = this.HorizontalOffset;
                double y = this.VerticalOffset;
                this.InternalTransform = new ScaleTransform(scale, scale);
                child.Arrange(new Rect(new Point(x, y), child.DesiredSize));
            }

            return arrangeSize;
        }

        public static Size ComputeScaleFactor(Size availableSize, Size contentSize) {
            bool isWidthPositiveInfinite = double.IsPositiveInfinity(availableSize.Width);
            bool isHeightPositiveInfinite = double.IsPositiveInfinity(availableSize.Height);
            if (isWidthPositiveInfinite && isHeightPositiveInfinite) {
                return new Size(1, 1);
            }

            double width = Maths.IsZero(contentSize.Width) ? 0.0 : availableSize.Width / contentSize.Width;
            double height = Maths.IsZero(contentSize.Height) ? 0.0 : availableSize.Height / contentSize.Height;
            if (isWidthPositiveInfinite) {
                width = height;
            }
            else if (isHeightPositiveInfinite) {
                height = width;
            }
            else {
                width = height = (width < height ? width : height);
            }

            return new Size(width, height);
        }

        #endregion

        #region Visual children and stuff

        protected override Visual GetVisualChild(int index) {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index), index, "index out of range: " + index);
            return this.InternalVisual;
        }

        #endregion

        #region Dependency property handlers

        private static object CoerceMinimumZoom(DependencyObject port, object value) {
            return value is double min ? (min < 0d ? ZeroDoubleObject : value) : ZeroDoubleObject;
        }

        private static object CoerceMaximumZoom(DependencyObject port, object value) {
            object minimum = port.GetValue(MinimumZoomScaleProperty);
            if (!(value is double max))
                return ZeroDoubleObject;
            if (max < 0d)
                return ZeroDoubleObject;
            return max < (double) minimum ? minimum : value;
        }

        private static object CoerceZoom(DependencyObject port, object value) {
            object min = port.GetValue(MinimumZoomScaleProperty);
            if (!(value is double scale) || scale < 0d || scale < (double) min)
                return min;
            object max = port.GetValue(MaximumZoomScaleProperty);
            return scale > (double) max ? max : value;
        }

        private static void OnMinimumZoomChanged(FreeMoveViewPort port, double oldValue, double newValue) {
            port.CoerceValue(MaximumZoomScaleProperty);
            port.CoerceValue(ZoomScaleProperty);
            port.OnMinimumZoomChanged(oldValue, newValue);
        }

        private static void OnMaximumZoomChanged(FreeMoveViewPort port, double oldValue, double newValue) {
            port.CoerceValue(ZoomScaleProperty);
            port.OnMaximumZoomChanged(oldValue, newValue);
        }

        private static void OnZoomChanged(FreeMoveViewPort port, double oldValue, double newValue) {
            port.OnZoomChanged(oldValue, newValue);
        }

        #endregion

        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            dc.DrawRectangle(this.Background, null, new Rect(this.DesiredSize));
        }

            /*
            UIElement child = this.InternalChild;
            if (child != null) {
                double scale = this.ZoomScale;
                Size childSize = child.DesiredSize;
                Size selfSize = this.DesiredSize;
                TransformGroup group = new TransformGroup();

                double x = this.HorizontalOffset;
                double y = this.VerticalOffset;

                arrangeSize.Width = scale * childSize.Width;
                arrangeSize.Height = scale * childSize.Height;

                if (selfSize.Width < arrangeSize.Width) {
                    double diff = (arrangeSize.Width - selfSize.Width) / 2d;
                    x -= diff / scale;
                }

                if (selfSize.Height < arrangeSize.Height) {
                    double diff = (arrangeSize.Height - selfSize.Height) / 2d;
                    y -= diff / scale;
                }

                group.Children.Add(new TranslateTransform(x, y));
                group.Children.Add(new ScaleTransform(scale, scale));
                this.InternalTransform = group;
                child.Arrange(new Rect(new Point(), child.DesiredSize));
            }
            return arrangeSize;
            */

            /*
            UIElement child = this.InternalChild;
            if (child != null) {
                double scale = this.ZoomScale;
                Size childSize = child.DesiredSize;
                TransformGroup group = new TransformGroup();
                double x = this.HorizontalOffset;
                double y = this.VerticalOffset;
                arrangeSize.Width = scale * childSize.Width;
                arrangeSize.Height = scale * childSize.Height;
                group.Children.Add(new TranslateTransform(x, y));
                group.Children.Add(new ScaleTransform(scale, scale));
                this.InternalTransform = group;
                child.Arrange(new Rect(new Point(), child.DesiredSize));
            }
            */
    }
}