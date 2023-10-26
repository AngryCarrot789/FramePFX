using System.Windows;
using System.Windows.Media;

namespace ColorPicker.UIExtensions {
    internal class RgbColorSlider : PreviewColorSlider {
        public static readonly DependencyProperty SliderArgbTypeProperty =
            DependencyProperty.Register(nameof(SliderArgbType), typeof(string), typeof(RgbColorSlider),
                new PropertyMetadata(""));

        public RgbColorSlider() : base() { }

        public string SliderArgbType {
            get => (string) this.GetValue(SliderArgbTypeProperty);
            set => this.SetValue(SliderArgbTypeProperty, value);
        }

        protected override void GenerateBackground() {
            Color colorStart = this.GetColorForSelectedArgb(0);
            Color colorEnd = this.GetColorForSelectedArgb(255);
            this.LeftCapColor.Color = colorStart;
            this.RightCapColor.Color = colorEnd;
            this.BackgroundGradient = new GradientStopCollection {
                new GradientStop(colorStart, 0.0),
                new GradientStop(colorEnd, 1)
            };
        }

        private Color GetColorForSelectedArgb(int value) {
            byte a = (byte) (this.CurrentColorState.A * 255);
            byte r = (byte) (this.CurrentColorState.RGB_R * 255);
            byte g = (byte) (this.CurrentColorState.RGB_G * 255);
            byte b = (byte) (this.CurrentColorState.RGB_B * 255);
            switch (this.SliderArgbType) {
                case "A": return Color.FromArgb((byte) value, r, g, b);
                case "R": return Color.FromArgb(255, (byte) value, g, b);
                case "G": return Color.FromArgb(255, r, (byte) value, b);
                case "B": return Color.FromArgb(255, r, g, (byte) value);
                default: return Color.FromArgb(a, r, g, b);
            }

            ;
        }
    }
}