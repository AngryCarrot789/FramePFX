using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Core.Utils;

namespace FramePFX.Controls.Helpers {
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
                new PropertyMetadata(BoolBox.False, OnUpdateSelectedItemsOnChangeChanged));

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
            obj.SetValue(UpdateSelectedItemsOnChangeProperty, value);
        }

        private static bool IS_UPDATING_SELECTION;

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (e.OldValue == e.NewValue || IS_UPDATING_SELECTION) {
                return;
            }

            if (d is ListBox box) {
                IS_UPDATING_SELECTION = true;
                box.SelectedItems.Clear();
                if (e.NewValue is IList selectedItems) {
                    foreach (object item in selectedItems) {
                        box.SelectedItems.Add(item);
                    }
                }

                IS_UPDATING_SELECTION = false;
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
            if (IS_UPDATING_SELECTION) {
                return;
            }

            if (sender is Selector selector) {
                IList selectedItems = GetSelectedItems(selector);
                if (selectedItems == null) {
                    return;
                }

                IS_UPDATING_SELECTION = true;

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

                IS_UPDATING_SELECTION = false;
            }
        }
    }
}