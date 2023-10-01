using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;

namespace FramePFX.WPF.Controls
{
    public static class TreeExtension
    {
        public static readonly DependencyProperty IsInitiallyExpandableProperty = DependencyProperty.RegisterAttached("IsInitiallyExpandable", typeof(bool), typeof(TreeExtension), new FrameworkPropertyMetadata(BoolBox.False, PropertyChangedCallback));

        public static void SetIsInitiallyExpandable(DependencyObject element, bool value) => element.SetValue(IsInitiallyExpandableProperty, value.Box());

        public static bool GetIsInitiallyExpandable(DependencyObject element) => (bool) element.GetValue(IsInitiallyExpandableProperty);

        private static readonly RoutedEventHandler ExpandedHandler = OnItemExpanded;

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if ((bool) e.NewValue && d is TreeViewItem item)
            {
                item.Expanded += ExpandedHandler;
            }
        }

        private static void OnItemExpanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem) sender;
            item.Expanded -= ExpandedHandler;
            item.ClearValue(IsInitiallyExpandableProperty);
        }
    }
}