using System.Windows;
using System.Windows.Input;

namespace ColorPicker {
    public partial class ColorDisplay : DualPickerControlBase {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register(nameof(CornerRadius), typeof(double), typeof(ColorDisplay)
                , new PropertyMetadata(0d));

        public double CornerRadius {
            get { return (double) this.GetValue(CornerRadiusProperty); }
            set { this.SetValue(CornerRadiusProperty, value); }
        }


        public ColorDisplay() : base() {
            this.InitializeComponent();
        }

        private void SwapButton_Click(object sender, RoutedEventArgs e) {
            this.SwapColors();
        }

        private void HintColor_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            this.SetMainColorFromHintColor();
        }
    }
}