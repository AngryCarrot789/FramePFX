using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Utils;

namespace FramePFX.Converters {
    public class VolumeToDbConverter : IValueConverter {
        public static VolumeToDbConverter Instance { get; } = new VolumeToDbConverter();

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

            double val = AudioUtils.VolumeToDb(input);
            if (this.RoundedPlaces is int round)
                val = Math.Round(val, round);
            return value is float ? (float) val : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is float f) {
                return AudioUtils.DbToVolume(f);
            }
            else if (value is double d) {
                return AudioUtils.DbToVolume(d);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }
    }
}