using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using FramePFX.Controls.Dragger;

namespace FramePFX.Editor.Exporting.Converters {
    public class BitRateToAltSizeConverter : IValueConverter {
        public static BitRateToAltSizeConverter Instance { get; } = new BitRateToAltSizeConverter();

        public int? RoundedPlaces { get; set; } = 2;

        public string GbFormat { get; set; } = "{0} GBit";
        public string MbFormat { get; set; } = "{0} MBit";
        public string KbFormat { get; set; } = "{0} KBit";
        public string BFormat { get; set; } = "{0} Bits";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is long bits) {
                string format;
                double dval;
                if (bits > 1000000000) {
                    format = this.GbFormat;
                    dval = bits / 1000000000d;
                }
                else if (bits > 1000000) {
                    format = this.MbFormat;
                    dval = bits / 1000000d;
                }
                else if (bits > 1000) {
                    format = this.KbFormat;
                    dval = bits / 1000d;
                }
                else {
                    format = this.BFormat;
                    dval = bits;
                    goto ret;
                }

                if (this.RoundedPlaces is int round) {
                    dval = Math.Round(dval, round);
                }

                ret:
                return string.Format(format, dval);
            }
            else {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class BitRateChangeMapper : IChangeMapper {
        public static BitRateChangeMapper Instance { get; } = new BitRateChangeMapper();

        public void OnValueChanged(double value, out double tiny, out double small, out double normal, out double large) {
            if (value > 1000000000) {
                tiny = 100d;
                small = 10000d;
                normal = 1000000d;
                large = 10000000d;
            }
            else if (value > 1000000) {
                tiny = 500d;
                small = 10000d;
                normal = 100000d;
                large = 1000000d;
            }
            else if (value > 1000) {
                tiny = 10d;
                small = 100d;
                normal = 1000d;
                large = 10000d;
            }
            else {
                tiny = 1d;
                small = 1d;
                normal = 1d;
                large = 5d;
            }
        }
    }
}