using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Core.Utils;

namespace FramePFX.Converters {
    public class DbToVolumeConverter : IValueConverter {
        public static DbToVolumeConverter Instance { get; } = new DbToVolumeConverter();

        public int? RoundedPlaces { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            double input;
            switch (value) {
                case float f:
                    input = f;
                    break;
                case double d:
                    input = d;
                    break;
                default: return DependencyProperty.UnsetValue;
            }

            double val = AudioUtils.DbToVolume(input);
            if (this.RoundedPlaces is int round)
                val = Math.Round(val, round);
            return value is float ? (float) val : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is float f) {
                return AudioUtils.VolumeToDb(f);
            }
            else if (value is double d) {
                return AudioUtils.VolumeToDb(d);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}