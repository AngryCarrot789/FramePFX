using ColorPicker.Models;
using System.Windows;

namespace ColorPicker {
    public partial class PortableColorPicker : DualPickerControlBase {
        public static readonly DependencyProperty SmallChangeProperty =
            DependencyProperty.Register(nameof(SmallChange), typeof(double), typeof(PortableColorPicker),
                new PropertyMetadata(1.0));

        public static readonly DependencyProperty ShowAlphaProperty =
            DependencyProperty.Register(nameof(ShowAlpha), typeof(bool), typeof(PortableColorPicker),
                new PropertyMetadata(true));

        public static readonly DependencyProperty PickerTypeProperty
            = DependencyProperty.Register(nameof(PickerType), typeof(PickerType), typeof(PortableColorPicker),
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

        public PortableColorPicker() {
            this.InitializeComponent();
        }
    }
}