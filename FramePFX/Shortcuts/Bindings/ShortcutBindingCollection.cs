using System.Windows;

namespace FramePFX.Shortcuts.Bindings {
    public class ShortcutBindingCollection : FreezableCollection<ShortcutCommandBinding> {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(ShortcutBindingCollection),
                typeof(ShortcutBindingCollection),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public static void SetCollection(DependencyObject element, ShortcutBindingCollection value) => element.SetValue(CollectionProperty, value);

        public static ShortcutBindingCollection GetCollection(DependencyObject element) => (ShortcutBindingCollection) element.GetValue(CollectionProperty);
    }
}