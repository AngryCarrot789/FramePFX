﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace ColorPicker.Converters {
    [ValueConversion(typeof(bool), typeof(bool))]
    internal class BoolToInvertedBoolConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is bool boolValue)
                return !boolValue;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException("ConvertBack() of BoolToInvertedBoolConverter is not implemented");
        }
    }
}