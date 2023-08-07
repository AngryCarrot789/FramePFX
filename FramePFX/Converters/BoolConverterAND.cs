using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Core.Utils;

namespace FramePFX.Converters
{
    public class BoolConverterAND : IMultiValueConverter
    {
        public bool EmptyArrayBool { get; set; } = false;

        public bool NonBoolBool
        {
            get => this.NonBoolValue is bool b && b;
            set => this.NonBoolValue = value.Box();
        }

        public object NonBoolValue { get; set; } = DependencyProperty.UnsetValue;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // if (values == null || values.Length != 3) {
            //     throw new Exception("Expected 3 elements, not " + (values != null ? values.Length.ToString() : "null"));
            // }
            // bool a = (bool) values[0]; // showOptionAlwaysUseResult
            // bool b = (bool) values[1]; // isAlwaysUseNextResult
            // bool c = (bool) values[2]; // showOptionAlwaysUseResultForCurrent
            // return (a && b && c).Box(); // box utils as optimisation
            if (values == null)
            {
                return this.EmptyArrayBool.Box();
            }

            foreach (object value in values)
            {
                if (value is bool boolean)
                {
                    if (!boolean)
                        return BoolBox.False;
                }
                else
                {
                    return this.NonBoolValue;
                }
            }

            return BoolBox.True;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}