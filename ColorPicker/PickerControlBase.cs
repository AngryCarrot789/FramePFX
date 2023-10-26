using ColorPicker.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ColorPicker {
    public class PickerControlBase : UserControl, IColorStateStorage {
        public static readonly DependencyProperty ColorStateProperty =
            DependencyProperty.Register(nameof(ColorState), typeof(ColorState), typeof(PickerControlBase),
                new PropertyMetadata(new ColorState(0, 0, 0, 1, 0, 0, 0, 0, 0, 0), OnColorStatePropertyChange));

        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register(nameof(SelectedColor), typeof(Color), typeof(PickerControlBase),
                new PropertyMetadata(Colors.Black, OnSelectedColorPropertyChange));

        public static readonly RoutedEvent ColorChangedEvent =
            EventManager.RegisterRoutedEvent(nameof(ColorChanged),
                RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PickerControlBase));

        public ColorState ColorState {
            get => (ColorState) this.GetValue(ColorStateProperty);
            set => this.SetValue(ColorStateProperty, value);
        }

        public Color SelectedColor {
            get => (Color) this.GetValue(SelectedColorProperty);
            set => this.SetValue(SelectedColorProperty, value);
        }

        public NotifyableColor Color { get; set; }

        private bool ignoreColorPropertyChange = false;
        private bool ignoreColorChange = false;
        private Color previousColor = System.Windows.Media.Color.FromArgb(5, 5, 5, 5);

        public event RoutedEventHandler ColorChanged {
            add => this.AddHandler(ColorChangedEvent, value);
            remove => this.RemoveHandler(ColorChangedEvent, value);
        }

        public PickerControlBase() {
            this.Color = new NotifyableColor(this);
            this.Color.PropertyChanged += (sender, e) => {
                Color newColor = System.Windows.Media.Color.FromArgb(
                    (byte) Math.Round(this.Color.A),
                    (byte) Math.Round(this.Color.RGB_R),
                    (byte) Math.Round(this.Color.RGB_G),
                    (byte) Math.Round(this.Color.RGB_B));
                if (newColor != this.previousColor) {
                    this.RaiseEvent(new ColorRoutedEventArgs(ColorChangedEvent, newColor));
                    this.previousColor = newColor;
                }
            };
            this.ColorChanged += (sender, e) => {
                if (!this.ignoreColorChange) {
                    this.ignoreColorPropertyChange = true;
                    this.SelectedColor = ((ColorRoutedEventArgs) e).Color;
                    this.ignoreColorPropertyChange = false;
                }
            };
        }

        private static void OnColorStatePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((PickerControlBase) d).Color.UpdateEverything((ColorState) args.OldValue);
        }

        private static void OnSelectedColorPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            PickerControlBase sender = (PickerControlBase) d;
            if (sender.ignoreColorPropertyChange)
                return;
            Color newValue = (Color) args.NewValue;
            sender.ignoreColorChange = true;
            sender.Color.A = newValue.A;
            sender.Color.RGB_R = newValue.R;
            sender.Color.RGB_G = newValue.G;
            sender.Color.RGB_B = newValue.B;
            sender.ignoreColorChange = false;
        }
    }
}