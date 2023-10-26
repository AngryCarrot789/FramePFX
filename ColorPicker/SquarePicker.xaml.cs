using ColorPicker.Models;
using System.Windows;

namespace ColorPicker {
    public partial class SquarePicker : PickerControlBase {
        public static readonly DependencyProperty PickerTypeProperty
            = DependencyProperty.Register(nameof(PickerType), typeof(PickerType), typeof(SquarePicker),
                new PropertyMetadata(PickerType.HSV));

        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(SquarePicker),
                new PropertyMetadata(1.0));

        public PickerType PickerType {
            get => (PickerType) this.GetValue(PickerTypeProperty);
            set => this.SetValue(PickerTypeProperty, value);
        }

        public double SmallChange {
            get => (double) this.GetValue(SmallChangeProperty);
            set => this.SetValue(SmallChangeProperty, value);
        }

        public SquarePicker() : base() {
            this.InitializeComponent();
        }
    }
}