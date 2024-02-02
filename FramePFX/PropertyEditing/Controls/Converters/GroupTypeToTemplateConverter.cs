using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.PropertyEditing.Controls.Converters {
    public class GroupTypeToTemplateConverter : IValueConverter {
        public FrameworkTemplate PrimaryExpander { get; set; }
        public FrameworkTemplate SecondaryExpander { get; set; }
        public FrameworkTemplate NoExpanderStyle { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == DependencyProperty.UnsetValue || !(value is GroupType groupType)) {
                return DependencyProperty.UnsetValue;
            }

            switch (groupType) {
                case GroupType.PrimaryExpander: return this.PrimaryExpander;
                case GroupType.SecondaryExpander: return this.SecondaryExpander;
                case GroupType.NoExpander: return this.NoExpanderStyle;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}