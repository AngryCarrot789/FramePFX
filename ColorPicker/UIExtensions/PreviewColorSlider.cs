using ColorPicker.Models;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ColorPicker.UIExtensions {
    internal abstract class PreviewColorSlider : Slider, INotifyPropertyChanged {
        public static readonly DependencyProperty CurrentColorStateProperty =
            DependencyProperty.Register(nameof(CurrentColorState), typeof(ColorState), typeof(PreviewColorSlider),
                new PropertyMetadata(ColorStateChangedCallback));

        public static readonly DependencyProperty SmallChangeBindableProperty =
            DependencyProperty.Register(nameof(SmallChangeBindable), typeof(double), typeof(PreviewColorSlider),
                new PropertyMetadata(1.0, SmallChangeBindableChangedCallback));

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual bool RefreshGradient => true;

        public PreviewColorSlider() {
            this.Minimum = 0;
            this.Maximum = 255;
            this.SmallChange = 1;
            this.LargeChange = 10;
            this.MinHeight = 12;
            this.PreviewMouseWheel += this.OnPreviewMouseWheel;
        }

        public double SmallChangeBindable {
            get => (double) this.GetValue(SmallChangeBindableProperty);
            set => this.SetValue(SmallChangeBindableProperty, value);
        }

        public ColorState CurrentColorState {
            get => (ColorState) this.GetValue(CurrentColorStateProperty);
            set => this.SetValue(CurrentColorStateProperty, value);
        }

        private readonly LinearGradientBrush backgroundBrush = new LinearGradientBrush();

        public GradientStopCollection BackgroundGradient {
            get => this.backgroundBrush.GradientStops;
            set => this.backgroundBrush.GradientStops = value;
        }

        private SolidColorBrush _leftCapColor = new SolidColorBrush();

        public SolidColorBrush LeftCapColor {
            get => this._leftCapColor;
            set {
                this._leftCapColor = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LeftCapColor)));
            }
        }

        private SolidColorBrush _rightCapColor = new SolidColorBrush();

        public SolidColorBrush RightCapColor {
            get => this._rightCapColor;
            set {
                this._rightCapColor = value;
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RightCapColor)));
            }
        }

        public override void EndInit() {
            base.EndInit();
            this.Background = this.backgroundBrush;
            this.GenerateBackground();
        }

        protected abstract void GenerateBackground();

        protected static void ColorStateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            PreviewColorSlider slider = (PreviewColorSlider) d;
            if (slider.RefreshGradient)
                slider.GenerateBackground();
        }

        private static void SmallChangeBindableChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((PreviewColorSlider) d).SmallChange = (double) e.NewValue;
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs args) {
            this.Value = MathHelper.Clamp(this.Value + this.SmallChange * args.Delta / 120, this.Minimum, this.Maximum);
            args.Handled = true;
        }
    }
}