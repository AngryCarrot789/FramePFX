using System.Windows;

namespace FramePFX.WPF.Shortcuts.Bindings {
    /// <summary>
    /// A collection of <see cref="ShortcutCommandBinding"/>
    /// </summary>
    public class ShortcutCommandCollection : FreezableCollection<ShortcutCommandBinding> {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(ShortcutCommandCollection),
                typeof(ShortcutCommandCollection),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public ShortcutCommandCollection() {
            // ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        // private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        // 
        // }

        /// <summary>
        /// Sets the attached <see cref="ShortcutCommandCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetCollection(DependencyObject element, ShortcutCommandCollection value) => element.SetValue(CollectionProperty, value);

        /// <summary>
        /// Gets the attached <see cref="ShortcutCommandCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ShortcutCommandCollection GetCollection(DependencyObject element) => (ShortcutCommandCollection) element.GetValue(CollectionProperty);
    }
}