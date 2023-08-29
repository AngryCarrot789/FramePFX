using System;
using System.Globalization;
using System.Windows.Data;
using FramePFX.WPF.Converters;

namespace FramePFX.WPF.Views.Message {
    public class CurrentQueueCheckBoxConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values == null || values.Length != 3) {
                throw new Exception("Expected 3 elements, not " + (values != null ? values.Length.ToString() : "null"));
            }

            bool a = (bool) values[0]; // showOptionAlwaysUseResult
            bool b = (bool) values[1]; // isAlwaysUseNextResult
            bool c = (bool) values[2]; // showOptionAlwaysUseResultForCurrent
            if (a && b && c) {
                return NullToVisibilityConverter.VisibleBox;
            }

            return NullToVisibilityConverter.CollapsedBox;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}