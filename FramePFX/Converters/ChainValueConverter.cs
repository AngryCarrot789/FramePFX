using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace FramePFX.Converters {
    public class ChainValueConverter : IValueConverter {
        private List<IValueConverter> converters;

        public List<IValueConverter> Converters {
            get => this.converters ?? (this.converters = new List<IValueConverter>());
            set => this.converters = value;
        }

        public ChainValueConverter() {
        }

        public ChainValueConverter(IEnumerable<IValueConverter> converters) {
            this.converters = converters != null ? new List<IValueConverter>(converters) : new List<IValueConverter>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (this.converters == null || this.converters.Count < 1)
                return value;
            foreach (IValueConverter converter in this.converters)
                value = converter.Convert(value, targetType, parameter, culture);
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (this.converters == null || this.converters.Count < 1)
                return value;
            foreach (IValueConverter converter in this.converters)
                value = converter.ConvertBack(value, targetType, parameter, culture);
            return value;
        }
    }
}