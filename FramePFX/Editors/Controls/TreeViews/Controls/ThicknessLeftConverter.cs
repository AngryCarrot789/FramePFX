using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.Editors.Controls.TreeViews.Controls {
    public class ThicknessLeftConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is int i)
                return new Thickness {Left = i};
            if (value is double d)
                return new Thickness {Left = d};
            return new Thickness();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}