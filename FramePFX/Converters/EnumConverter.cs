using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FramePFX.Converters {
    public class EnumConverter : IValueConverter {
        public Type EnumType { get; set; }

        public EnumDictionaryCollection Items { get; set; }

        public EnumConverter() {
            this.Items = new EnumDictionaryCollection();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (this.EnumType == null || value == null || this.Items == null) {
                return null;
            }

            try {
                string name = Enum.GetName(this.EnumType, value);
                return this.Items.FindValue(name);
            }
            catch {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class EnumDictionaryCollection : List<KeyValuePair> {
        public object FindValue(string name) {
            foreach (KeyValuePair pair in this) {
                if (pair.Key.ToString() == name) {
                    return pair.Value;
                }
            }

            return null;
        }
    }

    [ContentProperty("Value")]
    public class KeyValuePair {
        public object Key { get; set; }

        [DependsOn("Property")]
        public object Value { get; set; }
    }
}