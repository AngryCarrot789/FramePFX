using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Shortcuts.Bindings
{
    /// <summary>
    /// A collection of <see cref="ShortcutCommandBinding"/>
    /// </summary>
    public class ShortcutCommandCollection : FreezableCollection<ShortcutCommandBinding>
    {
        public static readonly DependencyProperty CollectionProperty =
            DependencyProperty.RegisterAttached(
                "Collection",
                typeof(ShortcutCommandCollection),
                typeof(ShortcutCommandCollection),
                new FrameworkPropertyMetadata(null, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (ReferenceEquals(e.OldValue, e.NewValue))
            {
                return;
            }

            if (e.NewValue == null)
            {
                if (e.OldValue == null)
                    throw new Exception("Impossible condition");
                OnCollectionRemoved(d, (ShortcutCommandCollection) e.OldValue);
            }
        }

        public ShortcutCommandCollection()
        {
            ((INotifyCollectionChanged) this).CollectionChanged += this.OnCollectionChanged;
        }

        private static void OnCollectionRemoved(DependencyObject obj, ShortcutCommandCollection collection)
        {
            if (obj is FrameworkElement element)
            {
                element.Loaded -= OnElementLoaded;
                element.Unloaded -= OnElementUnloaded;
            }
        }

        private static void OnCollectionSet(DependencyObject obj, ShortcutCommandCollection collection)
        {
            if (obj is FrameworkElement element)
            {
                element.Loaded += OnElementLoaded;
                element.Unloaded += OnElementUnloaded;
            }
        }

        private static void OnElementLoaded(object sender, RoutedEventArgs e)
        {
            DependencyObject element = (DependencyObject) sender;
            ShortcutCommandCollection collection = GetCollection(element);
            if (collection == null || collection.Count < 1)
            {
                return;
            }

            foreach (ShortcutCommandBinding binding in collection)
            {
                binding.OnElementLoaded(element);
            }
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            DependencyObject element = (DependencyObject) sender;
            ShortcutCommandCollection collection = GetCollection(element);
            if (collection == null || collection.Count < 1)
            {
                return;
            }

            foreach (ShortcutCommandBinding binding in collection)
            {
                binding.OnElementUnloaded(element);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
        }

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

        public static List<ShortcutCommandBinding> GetCommandBindingHierarchy(DependencyObject source)
        {
            List<ShortcutCommandBinding> list = new List<ShortcutCommandBinding>();
            do
            {
                object localValue = source.ReadLocalValue(CollectionProperty);
                if (localValue != DependencyProperty.UnsetValue && localValue is ShortcutCommandCollection collection && collection.Count > 0)
                {
                    list.AddRange(collection);
                }
            } while ((source = VisualTreeUtils.GetParent(source)) != null);

            return list;
        }
    }
}