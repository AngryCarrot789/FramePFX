using System.Windows;

namespace FramePFX.Themes.Attached {
    public static class CornerHelper {
        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.RegisterAttached("CornerRadius", typeof(double), typeof(CornerHelper), new PropertyMetadata(0d));

        public static void SetCornerRadius(DependencyObject element, double value) {
            element.SetValue(CornerRadiusProperty, value);
        }

        public static double GetCornerRadius(DependencyObject element) {
            return (double) element.GetValue(CornerRadiusProperty);
        }
    }
}
