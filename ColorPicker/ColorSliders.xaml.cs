using System.Windows;

namespace ColorPicker {
    public partial class ColorSliders : PickerControlBase {
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(ColorSliders),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty ShowAlphaProperty =
            DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(ColorSliders),
                new PropertyMetadata(true));

        public double SmallChange {
            get => (double) this.GetValue(SmallChangeProperty);
            set => this.SetValue(SmallChangeProperty, value);
        }

        public bool ShowAlpha {
            get => (bool) this.GetValue(ShowAlphaProperty);
            set => this.SetValue(ShowAlphaProperty, value);
        }

        public ColorSliders() : base() {
            this.InitializeComponent();
        }
    }
}