using System;
using ColorPicker.Models;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker.UIExtensions {
    internal class HsvColorSlider : PreviewColorSlider {
        public static readonly DependencyProperty SliderHsvTypeProperty =
            DependencyProperty.Register(nameof(SliderHsvType), typeof(string), typeof(HsvColorSlider),
                new PropertyMetadata(""));

        protected override bool RefreshGradient => this.SliderHsvType != "H";

        public HsvColorSlider() : base() { }

        public string SliderHsvType {
            get => (string) this.GetValue(SliderHsvTypeProperty);
            set => this.SetValue(SliderHsvTypeProperty, value);
        }

        protected override void GenerateBackground() {
            if (this.SliderHsvType == "H") {
                Color colorStart = this.GetColorForSelectedArgb(0);
                Color colorEnd = this.GetColorForSelectedArgb(360);
                this.LeftCapColor.Color = colorStart;
                this.RightCapColor.Color = colorEnd;
                this.BackgroundGradient = new GradientStopCollection() {
                    new GradientStop(colorStart, 0),
                    new GradientStop(this.GetColorForSelectedArgb(60), 1 / 6.0),
                    new GradientStop(this.GetColorForSelectedArgb(120), 2 / 6.0),
                    new GradientStop(this.GetColorForSelectedArgb(180), 0.5),
                    new GradientStop(this.GetColorForSelectedArgb(240), 4 / 6.0),
                    new GradientStop(this.GetColorForSelectedArgb(300), 5 / 6.0),
                    new GradientStop(colorEnd, 1)
                };
                return;
            }

            {
                Color colorStart = this.GetColorForSelectedArgb(0);
                Color colorEnd = this.GetColorForSelectedArgb(255);
                this.LeftCapColor.Color = colorStart;
                this.RightCapColor.Color = colorEnd;
                this.BackgroundGradient = new GradientStopCollection {
                    new GradientStop(colorStart, 0.0),
                    new GradientStop(colorEnd, 1)
                };
            }
        }

        private Color GetColorForSelectedArgb(int value) {
            switch (this.SliderHsvType) {
                case "H": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HsvToRgb(value, 1.0, 1.0);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                case "S": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HsvToRgb(this.CurrentColorState.HSV_H, value / 255.0, this.CurrentColorState.HSV_V);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                case "V": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HsvToRgb(this.CurrentColorState.HSV_H, this.CurrentColorState.HSV_S, value / 255.0);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                default: return Color.FromArgb((byte) (this.CurrentColorState.A * 255), (byte) (this.CurrentColorState.RGB_R * 255), (byte) (this.CurrentColorState.RGB_G * 255), (byte) (this.CurrentColorState.RGB_B * 255));
            }
        }
    }
}