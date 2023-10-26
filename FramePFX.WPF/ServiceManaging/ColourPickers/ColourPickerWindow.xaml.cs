using System.Windows;
using System.Windows.Input;
using ColorPicker.Models;
using FramePFX.Utils;
using FramePFX.Views.Dialogs;
using FramePFX.WPF.Views;
using SkiaSharp;

namespace FramePFX.WPF.ServiceManaging.ColourPickers {
    /// <summary>
    /// Interaction logic for ColourPickerWindow.xaml
    /// </summary>
    public partial class ColourPickerWindow : BaseDialog {
        public static readonly DependencyProperty ColourProperty = DependencyProperty.Register("Colour", typeof(SKColor), typeof(ColourPickerWindow), new PropertyMetadata(SKColors.White, (d, e) => ((ColourPickerWindow) d).OnColourChanged((SKColor) e.NewValue)));

        public SKColor Colour {
            get => (SKColor) this.GetValue(ColourProperty);
            set => this.SetValue(ColourProperty, value);
        }

        public SKColor ActiveColour {
            get {
                NotifyableColor colour = this.ColourPicker.Color;
                return new SKColor(
                    (byte) Maths.Clamp((int) colour.RGB_R, 0d, 255),
                    (byte) Maths.Clamp((int) colour.RGB_G, 0d, 255),
                    (byte) Maths.Clamp((int) colour.RGB_B, 0d, 255),
                    (byte) Maths.Clamp((int) colour.A, 0d, 255));
            }
        }

        public ColourPickerWindow() {
            this.InitializeComponent();
            this.DataContext = new BaseConfirmableDialogViewModel() {
                Dialog = this
            };
        }

        protected override void OnKeyDown(KeyEventArgs e) {
            base.OnKeyDown(e);
            if (e.Key == Key.Enter) {
                e.Handled = true;
                this.DialogResult = true;
                this.Close();
            }
        }

        private void OnColourChanged(SKColor color) {
            this.ColourPicker.Color.RGB_R = color.Red;
            this.ColourPicker.Color.RGB_G = color.Green;
            this.ColourPicker.Color.RGB_B = color.Blue;
            this.ColourPicker.Color.A = color.Alpha;
        }
    }
}