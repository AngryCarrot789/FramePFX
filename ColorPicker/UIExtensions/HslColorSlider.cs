using System;
using ColorPicker.Models;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker.UIExtensions {
    internal class HslColorSlider : PreviewColorSlider {
        public static readonly DependencyProperty SliderHslTypeProperty =
            DependencyProperty.Register(nameof(SliderHslType), typeof(string), typeof(HslColorSlider),
                new PropertyMetadata(""));

        protected override bool RefreshGradient => this.SliderHslType != "H";

        public HslColorSlider() : base() { }

        public string SliderHslType {
            get => (string) this.GetValue(SliderHslTypeProperty);
            set => this.SetValue(SliderHslTypeProperty, value);
        }

        protected override void GenerateBackground() {
            if (this.SliderHslType == "H") {
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

            if (this.SliderHslType == "L") {
                Color colorStart = this.GetColorForSelectedArgb(0);
                Color colorEnd = this.GetColorForSelectedArgb(255);
                this.LeftCapColor.Color = colorStart;
                this.RightCapColor.Color = colorEnd;
                this.BackgroundGradient = new GradientStopCollection() {
                    new GradientStop(colorStart, 0),
                    new GradientStop(this.GetColorForSelectedArgb(128), 0.5),
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
            switch (this.SliderHslType) {
                case "H": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HslToRgb(value, 1.0, 0.5);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                case "S": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HslToRgb(this.CurrentColorState.HSL_H, value / 255.0, this.CurrentColorState.HSL_L);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                case "L": {
                    Tuple<double, double, double> rgbtuple = ColorSpaceHelper.HslToRgb(this.CurrentColorState.HSL_H, this.CurrentColorState.HSL_S, value / 255.0);
                    double r = rgbtuple.Item1, g = rgbtuple.Item2, b = rgbtuple.Item3;
                    return Color.FromArgb(255, (byte) (r * 255), (byte) (g * 255), (byte) (b * 255));
                }
                default: return Color.FromArgb(255, (byte) (this.CurrentColorState.RGB_R * 255), (byte) (this.CurrentColorState.RGB_G * 255), (byte) (this.CurrentColorState.RGB_B * 255));
            }
        }
    }
}