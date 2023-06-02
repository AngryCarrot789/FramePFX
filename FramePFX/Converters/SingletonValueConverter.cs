using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace FramePFX.Converters {
    /// <summary>
    /// AngelSix way of doing singleton converters
    /// </summary>
    /// <typeparam name="T">The type of converter</typeparam>
    public abstract class SingletonValueConverter<T> : MarkupExtension, IValueConverter where T : class, new() {
        private static T instance;

        public static T Instance { get => instance ?? (instance = new T()); }

        public override object ProvideValue(IServiceProvider serviceProvider) {
            return Instance;
        }

        public abstract object Convert(object value, Type targetType, object parameter, CultureInfo culture);

        public abstract object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }
}