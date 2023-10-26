using ColorPicker.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace ColorPicker.UserControls {
    internal partial class HueSlider : UserControl {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(HueSlider),
                new PropertyMetadata(0.0));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(HueSlider),
                new PropertyMetadata(1.0));

        public double Value {
            get => (double) this.GetValue(ValueProperty);
            set => this.SetValue(ValueProperty, value);
        }

        public double SmallChange {
            get => (double) this.GetValue(SmallChangeProperty);
            set => this.SetValue(SmallChangeProperty, value);
        }

        public HueSlider() {
            this.InitializeComponent();
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e) {
            ((UIElement) sender).CaptureMouse();
            Path circle = (Path) sender;
            Point mousePos = e.GetPosition(circle);
            this.UpdateValue(mousePos, circle.ActualWidth, circle.ActualHeight);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e) {
            ((UIElement) sender).ReleaseMouseCapture();
        }

        private void OnMouseMove(object sender, MouseEventArgs e) {
            if (!((UIElement) sender).IsMouseCaptured)
                return;
            Path circle = (Path) sender;
            Point mousePos = e.GetPosition(circle);
            this.UpdateValue(mousePos, circle.ActualWidth, circle.ActualHeight);
        }

        private void UpdateValue(Point mousePos, double width, double height) {
            double x = mousePos.X / (width * 2);
            double y = mousePos.Y / (height * 2);

            double length = Math.Sqrt(x * x + y * y);
            if (length == 0)
                return;
            double angle = Math.Acos(x / length);
            if (y < 0)
                angle = -angle;
            angle = angle * 360 / (Math.PI * 2) + 180;
            this.Value = MathHelper.Clamp(angle, 0, 360);
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args) {
            this.Value = MathHelper.Mod(this.Value + this.SmallChange * args.Delta / 120, 360);
            args.Handled = true;
        }
    }
}