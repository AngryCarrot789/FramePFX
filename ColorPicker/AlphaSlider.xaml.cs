using System.Windows;

namespace ColorPicker {
    public partial class AlphaSlider : PickerControlBase {
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(AlphaSlider),
                new PropertyMetadata(1.0));

        public double SmallChange {
            get => (double) this.GetValue(SmallChangeProperty);
            set => this.SetValue(SmallChangeProperty, value);
        }

        public AlphaSlider() {
            this.InitializeComponent();
        }
    }
}