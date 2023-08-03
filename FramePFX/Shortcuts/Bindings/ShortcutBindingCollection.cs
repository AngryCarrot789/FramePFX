using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;

namespace FramePFX.Shortcuts.Bindings {
    /// <summary>
    /// A collection of <see cref="ShortcutCommandBinding"/>
    /// </summary>
    public class ShortcutBindingCollection : FreezableCollection<ShortcutCommandBinding> {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(ShortcutBindingCollection),
                typeof(ShortcutBindingCollection),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public ShortcutBindingCollection() {
            // ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        // private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        // 
        // }

        /// <summary>
        /// Sets the attached <see cref="ShortcutBindingCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetCollection(DependencyObject element, ShortcutBindingCollection value) => element.SetValue(CollectionProperty, value);

        /// <summary>
        /// Gets the attached <see cref="ShortcutBindingCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static ShortcutBindingCollection GetCollection(DependencyObject element) => (ShortcutBindingCollection) element.GetValue(CollectionProperty);
    }
}