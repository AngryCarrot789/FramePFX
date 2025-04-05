// 
// Copyright (c) 2024-2024 REghZy
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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using PFXToolKitUI.Utils;

namespace PFXToolKitUI.Avalonia.AvControls;

public class FreeMoveViewPortV2 : Border {
    private const double SafeMinimumZoomFactor = 0.05;

    public static readonly StyledProperty<double> MinimumZoomScaleProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, double>("MinimumZoomScale", SafeMinimumZoomFactor, coerce: (o, v) => !DoubleUtils.IsValid(v) ? o.GetValue(MinimumZoomScaleProperty!) : Math.Max(v, SafeMinimumZoomFactor));
    public static readonly StyledProperty<double> MaximumZoomScaleProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, double>("MaximumZoomScale", 50, coerce: (o, v) => !DoubleUtils.IsValid(v) ? o.GetValue(MaximumZoomScaleProperty!) : Math.Max(v, o.GetValue(MinimumZoomScaleProperty)));
    public static readonly StyledProperty<double> ZoomScaleProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, double>("ZoomScale", 1.0, coerce: (o, v) => !DoubleUtils.IsValid(v) ? o.GetValue(ZoomScaleProperty!) : Maths.Clamp(v, o.GetValue(MinimumZoomScaleProperty), o.GetValue(MaximumZoomScaleProperty)));
    public static readonly StyledProperty<double> HorizontalOffsetProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, double>("HorizontalOffset");
    public static readonly StyledProperty<double> VerticalOffsetProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, double>("VerticalOffset");
    public static readonly StyledProperty<bool> PanToCursorOnUserZoomProperty = AvaloniaProperty.Register<FreeMoveViewPortV2, bool>("PanToCursorOnUserZoom");
    public static readonly DirectProperty<FreeMoveViewPortV2, double> ExtentWidthProperty = AvaloniaProperty.RegisterDirect<FreeMoveViewPortV2, double>("ExtentWidth", o => o.ExtentWidth, null);
    public static readonly DirectProperty<FreeMoveViewPortV2, double> ExtentHeightProperty = AvaloniaProperty.RegisterDirect<FreeMoveViewPortV2, double>("ExtentHeight", o => o.ExtentHeight, null);

    private double _extentWidth;
    private double _extentHeight;

    public double ExtentWidth {
        get => this._extentWidth;
        set => this.SetAndRaise(ExtentWidthProperty, ref this._extentWidth, value);
    }

    public double ExtentHeight {
        get => this._extentHeight;
        set => this.SetAndRaise(ExtentHeightProperty, ref this._extentHeight, value);
    }

    public double MinimumZoomScale {
        get => this.GetValue(MinimumZoomScaleProperty);
        set => this.SetValue(MinimumZoomScaleProperty, value);
    }

    public double MaximumZoomScale {
        get => this.GetValue(MaximumZoomScaleProperty);
        set => this.SetValue(MaximumZoomScaleProperty, value);
    }

    public double ZoomScale {
        get => this.GetValue(ZoomScaleProperty);
        set => this.SetValue(ZoomScaleProperty, value);
    }

    public double HorizontalOffset {
        get => this.GetValue(HorizontalOffsetProperty);
        set => this.SetValue(HorizontalOffsetProperty, value);
    }

    public double VerticalOffset {
        get => this.GetValue(VerticalOffsetProperty);
        set => this.SetValue(VerticalOffsetProperty, value);
    }

    public bool PanToCursorOnUserZoom {
        get => this.GetValue(PanToCursorOnUserZoomProperty);
        set => this.SetValue(PanToCursorOnUserZoomProperty, value);
    }

    private Point lastMousePointAbs; // Relative to parent container
    private Point lastMousePointRel; // Relative to us being zoomed and translated

    public TransformationContainer? CanvasTransformContainer { get; set; }
    public SKAsyncViewPort? AsyncViewPort { get; set; }

    public FreeMoveViewPortV2() {
        this.AddHandler(PointerWheelChangedEvent, this.OnPreviewMouseWheel, RoutingStrategies.Tunnel, false);
        this.Background = Brushes.Transparent;
    }

    static FreeMoveViewPortV2() {
        AffectsMeasure<FreeMoveViewPortV2>(MinimumZoomScaleProperty, MaximumZoomScaleProperty, ZoomScaleProperty, HorizontalOffsetProperty, VerticalOffsetProperty);

        MinimumZoomScaleProperty.Changed.AddClassHandler<FreeMoveViewPortV2, double>((o, e) => {
            o.CoerceValue(MaximumZoomScaleProperty);
            o.CoerceValue(ZoomScaleProperty);
        });

        MaximumZoomScaleProperty.Changed.AddClassHandler<FreeMoveViewPortV2, double>((o, e) => {
            o.CoerceValue(ZoomScaleProperty);
        });

        HorizontalOffsetProperty.Changed.AddClassHandler<FreeMoveViewPortV2, double>((o, e) => {
        });

        VerticalOffsetProperty.Changed.AddClassHandler<FreeMoveViewPortV2, double>((o, e) => {
        });
    }

    public void Setup(TransformationContainer container, SKAsyncViewPort vp) {
        this.CanvasTransformContainer = container;
        this.AsyncViewPort = vp;
    }

    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        ApplicationPFX.Instance.Dispatcher.InvokeAsync(this.FitContentToCenter, DispatchPriority.Background);
    }

    public void FitContentToCenter() {
        if (this.AsyncViewPort == null) {
            return;
        }

        this.HorizontalOffset = 0;
        this.VerticalOffset = 0;
        this.ZoomScale = 1;

        // Process new zoom after layout update which occurs just after render
        const double AddedBorder = 20; // pixels

        Size mySize = this.Bounds.Size;
        Size childSize = this.AsyncViewPort.DesiredSize.Inflate(new Thickness(AddedBorder));
        double ratioW = mySize.Width / childSize.Width;
        double ratioH = mySize.Height / childSize.Height;
        if (ratioH < ratioW && childSize.Height > 0) {
            this.ZoomScale = mySize.Height / childSize.Height;
        }
        else if (childSize.Width > 0) {
            this.ZoomScale = mySize.Width / childSize.Width;
        }

        if (this.CanvasTransformContainer?.Child is Control containerChild) {
            containerChild.InvalidateMeasure();
        }
    }

    private void OnPreviewMouseWheel(object? sender, PointerWheelEventArgs e) {
        KeyModifiers modifiers = e.KeyModifiers;

        double delta = e.Delta.Y;
        if (DoubleUtils.IsZero(delta))
            delta = e.Delta.X;
        if (DoubleUtils.IsZero(delta))
            return;

        if ((modifiers & (KeyModifiers.Alt | KeyModifiers.Control)) != 0) {
            // zoom in or out
            // I spent so much time trying to get this working myself but could never figure it out,
            // but this post worked pretty much first try. Smh my head.
            // I changed a few things though, like flipping zoom
            // https://gamedev.stackexchange.com/a/182177/160952
            double oldzoom = this.ZoomScale;
            double newzoom = oldzoom * (delta > 0 ? 1.1 : 0.9);
            this.ZoomScale = newzoom;
            if (this.PanToCursorOnUserZoom) {
                newzoom = this.ZoomScale;
                Size size = this.Bounds.Size;
                Point pos = e.GetPosition(this);
                double pixels_difference_w = (size.Width / oldzoom) - (size.Width / newzoom);
                double side_ratio_x = (pos.X - (size.Width / 2)) / size.Width;
                this.HorizontalOffset -= pixels_difference_w * side_ratio_x;
                double pixels_difference_h = (size.Height / oldzoom) - (size.Height / newzoom);
                double side_ratio_h = (pos.Y - (size.Height / 2)) / size.Height;
                this.VerticalOffset -= pixels_difference_h * side_ratio_h;
            }
            else {
                if (this.CanvasTransformContainer?.Child is Control child) {
                    child.InvalidateMeasure();
                }
            }
        }
        else if ((modifiers & KeyModifiers.Shift) != 0) {
            // horizontally offset
            this.HorizontalOffset += delta * (1d / this.ZoomScale) * 20d;
        }
        else {
            // vertically offset
            this.VerticalOffset += delta * (1d / this.ZoomScale) * 20d;
        }

        e.Handled = true;
    }

    protected override void OnPointerEntered(PointerEventArgs e) {
        base.OnPointerEntered(e);
        this.lastMousePointAbs = e.GetPosition(this);
        this.lastMousePointRel = e.GetPosition(this);
    }

    protected override void OnPointerMoved(PointerEventArgs e) {
        base.OnPointerMoved(e);
        PointerPoint info = e.GetCurrentPoint(this);
        Point mousePoint = info.Position;
        if (info.Properties.IsMiddleButtonPressed || (info.Properties.IsLeftButtonPressed && (e.KeyModifiers & KeyModifiers.Alt) != 0)) {
            if (!ReferenceEquals(info.Pointer.Captured, this)) {
                info.Pointer.Capture(this);
            }

            Vector change = (e.GetPosition(this) - this.lastMousePointRel);
            this.HorizontalOffset += change.X / this.ZoomScale;
            this.VerticalOffset += change.Y / this.ZoomScale;
        }
        else {
            info.Pointer.Capture(null);
        }

        this.lastMousePointAbs = mousePoint;
        this.lastMousePointRel = e.GetPosition(this);
    }

    #region Measure and Arrangement

    protected override Size MeasureOverride(Size constraint) {
        Size size = new Size();
        this.AsyncViewPort?.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        this.ExtentWidth = constraint.Width * this.ZoomScale;
        this.ExtentHeight = constraint.Height * this.ZoomScale;
        return size;
    }

    protected override Size ArrangeOverride(Size arrangeSize) {
        if (this.CanvasTransformContainer is TransformationContainer container) {
            container.RenderTransform = new ScaleTransform(this.ZoomScale, this.ZoomScale);
            container.RenderTransformOrigin = new RelativePoint(arrangeSize.Width / 2d, arrangeSize.Height / 2d, RelativeUnit.Absolute);

            // Size visualSize = new Size(desired.Width * this.ZoomScale, desired.Height * this.ZoomScale);
            // if (visualSize.Width > arrangeSize.Width) {
            //     double diff = visualSize.Width - arrangeSize.Width;
            //     left -= (diff / 4d);
            // }
            // if (visualSize.Height > arrangeSize.Height) {
            //     double diff = visualSize.Height - arrangeSize.Height;
            //     top -= (diff / 4d);
            // }

            if (container.Child is Control control) {
                Size desired = container.DesiredSize;
                double left = ((arrangeSize.Width - desired.Width) / 2) + this.HorizontalOffset;
                double top = ((arrangeSize.Height - desired.Height) / 2) + this.VerticalOffset;
                control.Arrange(new Rect(new Point(left, top), desired));
            }
        }

        return arrangeSize;
    }

    #endregion
}