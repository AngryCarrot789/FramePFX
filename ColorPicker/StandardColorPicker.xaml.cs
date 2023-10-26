using ColorPicker.Models;
using System.Windows;

namespace ColorPicker {
    public partial class StandardColorPicker : DualPickerControlBase {
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(StandardColorPicker),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty ShowAlphaProperty =
            DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(StandardColorPicker),
                new PropertyMetadata(true));

        public static readonly DependencyProperty PickerTypeProperty
            = DependencyProperty.Register(nameof(PickerType), typeof(PickerType), typeof(StandardColorPicker),
                new PropertyMetadata(PickerType.HSV));

        public double SmallChange {
            get => (double) this.GetValue(SmallChangeProperty);
            set => this.SetValue(SmallChangeProperty, value);
        }

        public bool ShowAlpha {
            get => (bool) this.GetValue(ShowAlphaProperty);
            set => this.SetValue(ShowAlphaProperty, value);
        }

        public PickerType PickerType {
            get => (PickerType) this.GetValue(PickerTypeProperty);
            set => this.SetValue(PickerTypeProperty, value);
        }

        public StandardColorPicker() : base() {
            this.InitializeComponent();
        }
    }
}