using System.Windows;
using System.Windows.Controls;

namespace FramePFX.WPF.AttachedProperties
{
    public static class TopmostFocus
    {
        private class ZIndexExchangeData
        {
            public int OldFocusZIndex { get; set; }
        }

        public static readonly DependencyProperty FocusedZIndexProperty =
            DependencyProperty.RegisterAttached(
                "FocusedZIndex",
                typeof(int),
                typeof(TopmostFocus),
                new PropertyMetadata(0, OnZIndexPropertyChanged));

        private static readonly DependencyPropertyKey PreviousDataPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "PreviousData",
                typeof(ZIndexExchangeData),
                typeof(TopmostFocus),
                new PropertyMetadata(null));

        public static void SetFocusedZIndex(UIElement element, int value) => element.SetValue(FocusedZIndexProperty, value);
        public static int GetFocusedZIndex(UIElement element) => (int) element.GetValue(FocusedZIndexProperty);

        private static ZIndexExchangeData GetPreviousData(UIElement element)
        {
            ZIndexExchangeData data = (ZIndexExchangeData) element.GetValue(PreviousDataPropertyKey.DependencyProperty);
            if (data == null)
                element.SetValue(PreviousDataPropertyKey, data = new ZIndexExchangeData());
            return data;
        }

        private static readonly RoutedEventHandler GotFocusHandler = ControlOnGotFocus;
        private static readonly RoutedEventHandler LostFocusHandler = ControlOnLostFocus;

        private static void OnZIndexPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement control)
            {
                control.GotFocus -= GotFocusHandler;
                control.LostFocus -= LostFocusHandler;
                if (e.NewValue is int)
                {
                    control.GotFocus += GotFocusHandler;
                    control.LostFocus += LostFocusHandler;
                }
            }
        }

        private static void ControlOnGotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                ZIndexExchangeData prevdata = GetPreviousData(element);
                prevdata.OldFocusZIndex = Panel.GetZIndex(element);
                int newIndex = GetFocusedZIndex(element);
                Panel.SetZIndex(element, newIndex);
            }
        }

        private static void ControlOnLostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                int oldIndex = GetPreviousData(element).OldFocusZIndex;
                Panel.SetZIndex(element, oldIndex);
            }
        }
    }
}