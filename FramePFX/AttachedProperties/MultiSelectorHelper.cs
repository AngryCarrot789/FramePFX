using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core.Utils;

namespace FramePFX.AttachedProperties {
    /// <summary>
    /// A helper class for binding selected item collections.
    /// <para>
    /// When using observable collections, you just bind it directly and then handle the collection changed events in your code
    /// </para>
    /// <para>
    /// When using a normal list (that does not implement <see cref="INotifyCollectionChanged"/>), ensure the list is non-null at
    /// all times. This class will set the list property (when the selected items change) to the same instance,
    /// allowing you to handle the change in your property's setter
    /// </para>
    /// </summary>
    public static class MultiSelectorHelper {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(MultiSelectorHelper),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemsChanged));

        public static readonly DependencyProperty UpdateSelectedItemsOnChangeProperty =
            DependencyProperty.RegisterAttached(
                "UpdateSelectedItemsOnChange",
                typeof(bool),
                typeof(MultiSelectorHelper),
                new PropertyMetadata(BoolBox.True, OnUpdateSelectedItemsOnChangeChanged));

        private static readonly DependencyPropertyKey IsUpdatingSelectionProperty =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsUpdatingSelection",
                typeof(bool),
                typeof(MultiSelectorHelper),
                new PropertyMetadata(BoolBox.False));

        public static IList GetSelectedItems(DependencyObject obj) {
            return (IList) obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value) {
            obj.SetValue(SelectedItemsProperty, value);
        }

        public static bool GetUpdateSelectedItemsOnChange(DependencyObject obj) {
            return (bool) obj.GetValue(UpdateSelectedItemsOnChangeProperty);
        }

        public static void SetUpdateSelectedItemsOnChange(DependencyObject obj, bool value) {
            obj.SetValue(UpdateSelectedItemsOnChangeProperty, value.Box());
        }

        private static void SetIsUpdatingSelection(DependencyObject element, bool value) {
            element.SetValue(IsUpdatingSelectionProperty, value.Box());
        }

        private static bool GetIsUpdatingSelection(DependencyObject element) {
            return (bool) element.GetValue(IsUpdatingSelectionProperty.DependencyProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (GetIsUpdatingSelection(d)) {
                return;
            }

            if (d is Selector) {
                SetIsUpdatingSelection(d, true);
                try {
                    IList list;
                    if (d is ListBox box) {
                        list = box.SelectedItems;
                    }
                    else if (d is MultiSelector ms) {
                        list = ms.SelectedItems;
                    }
                    else {
                        list = null;
                    }

                    if (list != null) {
                        list.Clear();
                        if (e.NewValue is IList selectedItems) {
                            foreach (object item in selectedItems) {
                                list.Add(item);
                            }
                        }
                    }
                }
                finally {
                    SetIsUpdatingSelection(d, false);
                }
            }

            TryRegisterEvents(d);
        }

        private static void TryRegisterEvents(DependencyObject obj) {
            if (GetUpdateSelectedItemsOnChange(obj)) {
                if (obj is Selector selector) {
                    selector.SelectionChanged -= OnSelectionChanged;
                    selector.SelectionChanged += OnSelectionChanged;
                }
            }
        }

        private static void OnUpdateSelectedItemsOnChangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (d is Selector selector) {
                selector.SelectionChanged -= OnSelectionChanged;
                if ((bool) e.NewValue) {
                    selector.SelectionChanged += OnSelectionChanged;
                }
            }
        }

        private static bool ListEquals(IList a, IList b) {
            int cA = a.Count, cB = b.Count;
            if (cA != cB) {
                return false;
            }

            for (int i = 0; i < cA; i++) {
                if (!ReferenceEquals(a[i], b[i])) {
                    return false;
                }
            }

            return true;
        }

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is Selector selector) {
                if (GetIsUpdatingSelection(selector)) {
                    return;
                }

                IList selectedItems = GetSelectedItems(selector);
                if (selectedItems == null) {
                    return;
                }

                try {
                    IList src;
                    switch (selector) {
                        case ListBox lb:
                            src = lb.SelectedItems;
                            break;
                        case MultiSelector ms:
                            src = ms.SelectedItems;
                            break;
                        default:
                            src = null;
                            break;
                    }

                    if (src != null) {
                        // Can massively improve performance for property pages
                        if (ListEquals(src, selectedItems)) {
                            return;
                        }

                        SetIsUpdatingSelection(selector, true);

                        selectedItems.Clear();
                        foreach (object item in src)
                            selectedItems.Add(item);
                    }
                    else {
                        SetIsUpdatingSelection(selector, true);
                        if (e.RemovedItems != null) {
                            foreach (object value in e.RemovedItems) {
                                selectedItems.Remove(value);
                            }
                        }

                        if (e.AddedItems != null) {
                            foreach (object value in e.AddedItems) {
                                selectedItems.Add(value);
                            }
                        }
                    }

                    if (!(selectedItems is INotifyCollectionChanged))
                        SetSelectedItems(selector, selectedItems);
                }
                finally {
                    SetIsUpdatingSelection(selector, false);
                }
            }
        }
    }
}