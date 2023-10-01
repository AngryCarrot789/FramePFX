using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using FramePFX.Utils;

namespace FramePFX.WPF.Converters
{
    public class BoolConverter : IValueConverter
    {
        public object TrueValue { get; set; }

        public object FalseValue { get; set; }

        public object UnsetValue { get; set; }

        public object NonBoolValue { get; set; }

        public bool ThrowForUnset { get; set; }

        public bool ThrowForNonBool { get; set; }

        public BoolConverter()
        {
            this.UnsetValue = DependencyProperty.UnsetValue;
            this.NonBoolValue = DependencyProperty.UnsetValue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return boolean ? this.TrueValue : this.FalseValue;
            }
            else if (value == DependencyProperty.UnsetValue)
            {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : this.UnsetValue;
            }
            else if (this.ThrowForNonBool)
            {
                throw new Exception("Expected boolean, got " + value);
            }
            else
            {
                return this.NonBoolValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Equals(value, this.TrueValue))
            {
                return true;
            }
            else if (Equals(value, this.FalseValue))
            {
                return false;
            }
            else if (Equals(value, this.UnsetValue))
            {
                return this.ThrowForUnset ? throw new Exception("Unset value not allowed") : DependencyProperty.UnsetValue;
            }
            else if (this.ThrowForNonBool)
            {
                throw new Exception("Expected boolean, got " + value);
            }
            else
            {
                throw new Exception("Cannot convert back from " + value);
            }
        }
    }

    public class InvertBoolConverter : BoolConverter
    {
        public static InvertBoolConverter Instance { get; } = new InvertBoolConverter();

        public InvertBoolConverter()
        {
            this.TrueValue = BoolBox.False;
            this.FalseValue = BoolBox.True;
        }
    }

    public class BoolToVisibilityConverter : BoolConverter
    {
        public static BoolToVisibilityConverter BoolToVisibleOrCollapsed { get; } = new BoolToVisibilityConverter();
        public static BoolToVisibilityConverter BoolToVisibleOrHidden { get; } = new BoolToVisibilityConverter() {FalseValue = Visibility.Hidden};

        public new Visibility TrueValue
        {
            get => (Visibility) base.TrueValue;
            set => base.TrueValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility FalseValue
        {
            get => (Visibility) base.FalseValue;
            set => base.FalseValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility UnsetValue
        {
            // get will throw by default... cast to base type to set
            get => base.UnsetValue is Visibility v ? v : Visibility.Collapsed;
            set => base.UnsetValue = NullToVisibilityConverter.Box(value);
        }

        public new Visibility NonBoolValue
        {
            // get will throw by default... cast to base type to set
            get => base.NonBoolValue is Visibility v ? v : Visibility.Collapsed;
            set => base.NonBoolValue = NullToVisibilityConverter.Box(value);
        }

        public BoolToVisibilityConverter()
        {
            this.TrueValue = Visibility.Visible;
            this.FalseValue = Visibility.Collapsed;
        }
    }

    public class BoolToBrushConverter : BoolConverter
    {
        public new Brush TrueValue
        {
            get => (Brush) base.TrueValue;
            set => base.TrueValue = value;
        }

        public new Brush FalseValue
        {
            get => (Brush) base.FalseValue;
            set => base.FalseValue = value;
        }

        public BoolToBrushConverter()
        {
            this.TrueValue = null;
            this.FalseValue = null;
        }
    }

    public class BoolToColourConverter : BoolConverter
    {
        public new Color TrueValue
        {
            get => (Color) base.TrueValue;
            set => base.TrueValue = value;
        }

        public new Color FalseValue
        {
            get => (Color) base.FalseValue;
            set => base.FalseValue = value;
        }

        public BoolToColourConverter()
        {
            this.TrueValue = Colors.Black;
            this.FalseValue = Colors.Black;
        }
    }

    public class BoolToDoubleConverter : BoolConverter
    {
        public new double TrueValue
        {
            get => (double) base.TrueValue;
            set => base.TrueValue = value;
        }

        public new double FalseValue
        {
            get => (double) base.FalseValue;
            set => base.FalseValue = value;
        }

        public BoolToDoubleConverter()
        {
            this.TrueValue = 1.0d;
            this.FalseValue = 0.0d;
        }
    }
}