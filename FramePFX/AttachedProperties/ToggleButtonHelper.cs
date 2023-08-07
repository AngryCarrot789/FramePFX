using System.Windows;
using System.Windows.Controls;
using FramePFX.Core.Utils;

namespace FramePFX.AttachedProperties
{
    public static class ToggleButtonHelper
    {
        public static readonly DependencyProperty IsDisabledWhenIsCheckedIsNullProperty = DependencyProperty.RegisterAttached(
            "IsDisabledWhenIsCheckedIsNull", typeof(bool), typeof(ToggleButtonHelper), new PropertyMetadata(BoolBox.False, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CheckBox cb)
            {
                cb.Checked -= OnCheckChanged;
                cb.Unchecked -= OnCheckChanged;
                if (e.NewValue is bool b && b)
                {
                    cb.Checked += OnCheckChanged;
                    cb.Unchecked += OnCheckChanged;
                }
            }
        }

        public static void SetIsDisabledWhenIsCheckedIsNull(DependencyObject element, bool value)
        {
            element.SetValue(IsDisabledWhenIsCheckedIsNullProperty, value);
        }

        public static bool GetIsDisabledWhenIsCheckedIsNull(DependencyObject element)
        {
            return (bool) element.GetValue(IsDisabledWhenIsCheckedIsNullProperty);
        }

        private static void OnCheckChanged(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && GetIsDisabledWhenIsCheckedIsNull(cb))
            {
                cb.IsEnabled = cb.IsChecked != null;
            }
        }
    }
}