using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.WPF.Views.Exceptions
{
    public class CollectionToVisibilityConverter : IValueConverter
    {
        public Visibility EmptyVisibility { get; set; } = Visibility.Collapsed;

        public Visibility NotEmptyVisibility { get; set; } = Visibility.Visible;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int count;
            if (value is ICollection collection)
            {
                count = collection.Count;
            }
            else if (value is int i)
            {
                count = i;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }

            switch (count)
            {
                case 0: return this.EmptyVisibility;
                default: return this.NotEmptyVisibility;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}