using ColorPicker.Models;
using System;
using System.Windows;
using System.Windows.Media;

namespace ColorPicker {
    public class DualPickerControlBase : PickerControlBase, ISecondColorStorage, IHintColorStateStorage {
        public static readonly DependencyProperty SecondColorStateProperty =
            DependencyProperty.Register(nameof(SecondColorState), typeof(ColorState), typeof(DualPickerControlBase),
                new PropertyMetadata(new ColorState(1, 1, 1, 1, 0, 0, 1, 0, 0, 1), OnSecondColorStatePropertyChange));

        public static readonly DependencyProperty SecondaryColorProperty =
            DependencyProperty.Register(nameof(SecondaryColor), typeof(Color), typeof(DualPickerControlBase),
                new PropertyMetadata(Colors.White, OnSecondaryColorPropertyChange));

        private readonly SecondColorDecorator secondColorDecorator;
        private readonly HintColorDecorator hintColorDecorator;

        public ColorState SecondColorState {
            get => (ColorState) this.GetValue(SecondColorStateProperty);
            set => this.SetValue(SecondColorStateProperty, value);
        }

        public NotifyableColor SecondColor {
            get;
            set;
        }

        public Color SecondaryColor {
            get => (Color) this.GetValue(SecondaryColorProperty);
            set => this.SetValue(SecondaryColorProperty, value);
        }


        public static readonly DependencyProperty HintColorStateProperty =
            DependencyProperty.Register(nameof(HintColorState), typeof(ColorState), typeof(DualPickerControlBase),
                new PropertyMetadata(new ColorState(0, 0, 0, 0, 0, 0, 0, 0, 0, 0), OnHintColorStatePropertyChange));

        public static readonly DependencyProperty HintColorProperty =
            DependencyProperty.Register(nameof(HintColor), typeof(Color), typeof(DualPickerControlBase),
                new PropertyMetadata(Colors.Transparent, OnHintColorPropertyChanged));

        public ColorState HintColorState {
            get => (ColorState) this.GetValue(HintColorStateProperty);
            set => this.SetValue(HintColorStateProperty, value);
        }

        public NotifyableColor HintNotifyableColor {
            get;
            set;
        }

        public Color HintColor {
            get { return (Color) this.GetValue(HintColorProperty); }
            set { this.SetValue(HintColorProperty, value); }
        }

        public static readonly DependencyProperty UseHintColorProperty =
            DependencyProperty.Register(nameof(UseHintColor), typeof(bool), typeof(DualPickerControlBase), new PropertyMetadata(false));

        public bool UseHintColor {
            get { return (bool) this.GetValue(UseHintColorProperty); }
            set { this.SetValue(UseHintColorProperty, value); }
        }


        private bool ignoreSecondaryColorChange = false;
        private bool ignoreSecondaryColorPropertyChange = false;

        private bool ignoreHintNotifyableColorChange = false;
        private bool ignoreHintColorPropertyChange = false;

        public DualPickerControlBase() : base() {
            this.secondColorDecorator = new SecondColorDecorator(this);
            this.hintColorDecorator = new HintColorDecorator(this);

            this.SecondColor = new NotifyableColor(this.secondColorDecorator);
            this.SecondColor.PropertyChanged += (sender, args) => {
                if (!this.ignoreSecondaryColorChange) {
                    this.ignoreSecondaryColorPropertyChange = true;
                    this.SecondaryColor = System.Windows.Media.Color.FromArgb(
                        (byte) Math.Round(this.SecondColor.A),
                        (byte) Math.Round(this.SecondColor.RGB_R),
                        (byte) Math.Round(this.SecondColor.RGB_G),
                        (byte) Math.Round(this.SecondColor.RGB_B));
                    this.ignoreSecondaryColorPropertyChange = false;
                }
            };

            this.HintNotifyableColor = new NotifyableColor(this.hintColorDecorator);
            this.HintNotifyableColor.PropertyChanged += (sender, args) => {
                if (!this.ignoreHintNotifyableColorChange) {
                    this.ignoreHintColorPropertyChange = true;
                    this.HintColor = System.Windows.Media.Color.FromArgb(
                        (byte) Math.Round(this.HintNotifyableColor.A),
                        (byte) Math.Round(this.HintNotifyableColor.RGB_R),
                        (byte) Math.Round(this.HintNotifyableColor.RGB_G),
                        (byte) Math.Round(this.HintNotifyableColor.RGB_B));
                    this.ignoreHintColorPropertyChange = false;
                }
            };
        }

        public void SwapColors() {
            ColorState temp = this.ColorState;
            this.ColorState = this.SecondColorState;
            this.SecondColorState = temp;
        }

        public void SetMainColorFromHintColor() {
            this.ColorState = this.HintColorState;
        }

        private static void OnSecondColorStatePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((DualPickerControlBase) d).SecondColor.UpdateEverything((ColorState) args.OldValue);
        }

        private static void OnHintColorStatePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            ((DualPickerControlBase) d).HintNotifyableColor.UpdateEverything((ColorState) args.OldValue);
        }

        private static void OnHintColorPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            DualPickerControlBase sender = (DualPickerControlBase) d;
            Color newValue = (Color) args.NewValue;
            if (sender.ignoreHintColorPropertyChange)
                return;
            sender.ignoreHintNotifyableColorChange = true;
            sender.HintNotifyableColor.A = newValue.A;
            sender.HintNotifyableColor.RGB_R = newValue.R;
            sender.HintNotifyableColor.RGB_G = newValue.G;
            sender.HintNotifyableColor.RGB_B = newValue.B;
            sender.ignoreHintNotifyableColorChange = false;
        }

        private static void OnSecondaryColorPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs args) {
            DualPickerControlBase sender = (DualPickerControlBase) d;
            if (sender.ignoreSecondaryColorPropertyChange)
                return;
            Color newValue = (Color) args.NewValue;
            sender.ignoreSecondaryColorChange = true;
            sender.SecondColor.A = newValue.A;
            sender.SecondColor.RGB_R = newValue.R;
            sender.SecondColor.RGB_G = newValue.G;
            sender.SecondColor.RGB_B = newValue.B;
            sender.ignoreSecondaryColorChange = false;
        }
    }
}