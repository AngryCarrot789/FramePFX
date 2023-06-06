using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core.Utils;

namespace FramePFX.Controls.Helpers {
    /// <summary>
    /// A helper class for binding selected item collections
    /// <para>
    /// When using observable collections, you just bind it directly and then handle the collection changed events in your code
    /// </para>
    /// <para>
    /// When using a normal list, ensure the list is not null at all times. This class will set the list property when the selected items are changed
    /// </para>
    /// </summary>
    public static class SelectorHelper {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(SelectorHelper),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        public static readonly DependencyProperty UpdateSelectedItemsOnChangeProperty =
            DependencyProperty.RegisterAttached(
                "UpdateSelectedItemsOnChange",
                typeof(bool),
                typeof(SelectorHelper),
                new PropertyMetadata(BoolBox.True, OnUpdateSelectedItemsOnChangeChanged));

        private static readonly DependencyPropertyKey IsUpdatingSelectionProperty =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsUpdatingSelection",
                typeof(bool),
                typeof(SelectorHelper),
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

        private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (sender is Selector selector) {
                if (GetIsUpdatingSelection(selector)) {
                    return;
                }

                IList selectedItems = GetSelectedItems(selector);
                if (selectedItems == null) {
                    return;
                }

                SetIsUpdatingSelection(selector, true);
                try {
                    IList enumerable;
                    switch (selector) {
                        case ListBox lb: enumerable = lb.SelectedItems; break;
                        case MultiSelector ms: enumerable = ms.SelectedItems; break;
                        default: enumerable = null; break;
                    }

                    if (enumerable != null) {
                        selectedItems.Clear();
                        foreach (object item in enumerable)
                            selectedItems.Add(item);
                    }
                    else {
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