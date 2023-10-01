using System;
using System.Globalization;
using System.Windows.Data;

namespace FramePFX.WPF.Editor.Notifications.SavingProject
{
    public class SavingProjectMessageConverter : IMultiValueConverter
    {
        public static SavingProjectMessageConverter Instance { get; } = new SavingProjectMessageConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool saving = (bool) values[0];
            bool success = (bool) values[1];
            string errMsg = (string) values[2];
            if (saving)
            {
                return "Saving project...";
            }
            else if (success)
            {
                return "Project Saved!";
            }
            else
            {
                return $"Failed to save project: {errMsg}";
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}