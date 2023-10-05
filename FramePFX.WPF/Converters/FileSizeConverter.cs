using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace FramePFX.WPF.Converters {
    public class FileSizeConverter : IValueConverter {
        public static readonly string[] SUFFIXES = {" B ", " KB", " MB", " GB", " TB", " PB", " EB"};

        [DllImport("Shlwapi.dll", CharSet = CharSet.Auto)]
        public static extern long StrFormatByteSize(long fileSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, int bufferSize);

        public int RoundedPlaces { get; set; } = 2;

        public static string BytesToString(long bytes, int roundedPlaces = 2) {
            if (bytes <= 0) {
                return "0" + SUFFIXES[0];
            }

            int place = System.Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), roundedPlaces);
            return num + SUFFIXES[place];
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null || value == DependencyProperty.UnsetValue) {
                return value;
            }

            if (value is long bytes) {
                // StringBuilder sb = new StringBuilder(20);
                // StrFormatByteSize(bytes, sb, sb.Capacity);
                // return sb.ToString();
                return BytesToString(bytes, this.RoundedPlaces);
            }
            else {
                return $"[DEBUG_ERR_NOT_LONG: {value.GetType()} -> {value}]";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}