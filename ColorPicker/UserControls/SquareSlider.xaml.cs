using ColorPicker.Models;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ColorPicker.UserControls {
    internal partial class SquareSlider : UserControl, INotifyPropertyChanged {
        public static readonly DependencyProperty HueProperty
            = DependencyProperty.Register(nameof(Hue), typeof(double), typeof(SquareSlider),
                new PropertyMetadata(0.0, OnHueChanged));

        public static readonly DependencyProperty HeadXProperty
            = DependencyProperty.Register(nameof(HeadX), typeof(double), typeof(SquareSlider),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty HeadYProperty
            = DependencyProperty.Register(nameof(HeadY), typeof(double), typeof(SquareSlider),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty PickerTypeProperty
            = DependencyProperty.Register(nameof(PickerType), typeof(PickerType), typeof(SquareSlider),
                new PropertyMetadata(PickerType.HSV, OnColorSpaceChanged));

        public double Hue {
            get => (double) this.GetValue(HueProperty);
            set => this.SetValue(HueProperty, value);
        }

        public double HeadX {
            get => (double) this.GetValue(HeadXProperty);
            set => this.SetValue(HeadXProperty, value);
        }

        public double HeadY {
            get => (double) this.GetValue(HeadYProperty);
            set => this.SetValue(HeadYProperty, value);
        }

        public PickerType PickerType {
            get => (PickerType) this.GetValue(PickerTypeProperty);
            set => this.SetValue(PickerTypeProperty, value);
        }

        private double _rangeX;

        public double RangeX {
            get => this._rangeX;
            set {
                this._rangeX = value;
                this.RaisePropertyChanged(nameof(this.RangeX));
            }
        }

        private double _rangeY;

        public double RangeY {
            get => this._rangeY;
            set {
                this._rangeY = value;
                this.RaisePropertyChanged(nameof(this.RangeY));
            }
        }

        public SquareSlider() {
            this.GradientBitmap = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Rgb24, null);
            this.InitializeComponent();
            this.RecalculateGradient();
        }

        private WriteableBitmap _gradientBitmap;

        public WriteableBitmap GradientBitmap {
            get => this._gradientBitmap;
            set {
                this._gradientBitmap = value;
                this.RaisePropertyChanged(nameof(this.GradientBitmap));
            }
        }

        private Func<double, double, double, Tuple<double, double, double>> colorSpaceConversionMethod = ColorSpaceHelper.HsvToRgb;

        private void RecalculateGradient() {
            int w = this.GradientBitmap.PixelWidth;
            int h = this.GradientBitmap.PixelHeight;
            double hue = this.Hue;
            byte[] pixels = new byte[w * h * 3];
            for (int j = 0; j < h; j++) {
                for (int i = 0; i < w; i++) {
                    Tuple<double, double, double> rgbtuple = this.colorSpaceConversionMethod(hue, i / (double) (w - 1), ((h - 1) - j) / (double) (h - 1));
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    int pos = (j * h + i) * 3;
                    pixels[pos] = (byte) (r * 255);
                    pixels[pos + 1] = (byte) (g * 255);
                    pixels[pos + 2] = (byte) (b * 255);
                }
            }

            this.GradientBitmap.WritePixels(new Int32Rect(0, 0, w, h), pixels, w * 3, 0);
        }

        private static void OnColorSpaceChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            SquareSlider sender = (SquareSlider) d;
            if ((PickerType) args.NewValue == PickerType.HSV)
                sender.colorSpaceConversionMethod = ColorSpaceHelper.HsvToRgb;
            else
                sender.colorSpaceConversionMethod = ColorSpaceHelper.HslToRgb;

            sender.RecalculateGradient();
        }

        private static void OnHueChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((SquareSlider) d).RecalculateGradient();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            ((UIElement) sender).CaptureMouse();
            this.UpdatePos(e.GetPosition(this));
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            Grid grid = (Grid) sender;
            if (grid.IsMouseCaptured)
                this.UpdatePos(e.GetPosition(this));
        }

        private void UpdatePos(Point pos) {
            this.HeadX = MathHelper.Clamp(pos.X / this.ActualWidth, 0, 1) * this.RangeX;
            this.HeadY = (1 - MathHelper.Clamp(pos.Y / this.ActualHeight, 0, 1)) * this.RangeY;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e) {
            ((UIElement) sender).ReleaseMouseCapture();
        }

        private void RaisePropertyChanged(string property) {
            if (property != null)
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}