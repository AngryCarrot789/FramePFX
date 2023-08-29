using System.Windows;

namespace FramePFX.WPF.Shortcuts.Bindings {
    public class InputStateCollection : FreezableCollection<InputStateBinding> {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(InputStateCollection),
                typeof(InputStateCollection),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        public InputStateCollection() {
            // ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        // private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        //
        // }

        /// <summary>
        /// Sets the attached <see cref="InputStateCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <param name="value"></param>
        public static void SetCollection(DependencyObject element, InputStateCollection value) => element.SetValue(CollectionProperty, value);

        /// <summary>
        /// Gets the attached <see cref="InputStateCollection"/> for an element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static InputStateCollection GetCollection(DependencyObject element) => (InputStateCollection) element.GetValue(CollectionProperty);
    }
}