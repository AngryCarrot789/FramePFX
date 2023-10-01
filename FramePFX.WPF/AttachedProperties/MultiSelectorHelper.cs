using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Utils;

namespace FramePFX.WPF.AttachedProperties
{
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
    public static class MultiSelectorHelper
    {
        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(MultiSelectorHelper),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        public static readonly DependencyProperty UpdatePropertyOnSelectionChangedProperty =
            DependencyProperty.RegisterAttached(
                "UpdatePropertyOnSelectionChanged",
                typeof(bool),
                typeof(MultiSelectorHelper),
                new PropertyMetadata(BoolBox.True, (d, e) => AutoRegisterSelectionChangedHandler(d)));

        private static readonly DependencyPropertyKey UpdatingSelectionProperty =
            DependencyProperty.RegisterAttachedReadOnly(
                "UpdatingSelection",
                typeof(bool),
                typeof(MultiSelectorHelper),
                new PropertyMetadata(BoolBox.False));

        private static readonly DependencyProperty IsSelectionChangedRegisteredProperty =
            DependencyProperty.RegisterAttached(
                "IsSelectionChangedRegistered",
                typeof(bool),
                typeof(MultiSelectorHelper),
                new PropertyMetadata(BoolBox.False));

        public static IList GetSelectedItems(DependencyObject obj) => (IList) obj.GetValue(SelectedItemsProperty);

        public static void SetSelectedItems(DependencyObject obj, IList value) => obj.SetValue(SelectedItemsProperty, value);

        public static bool GetUpdatePropertyOnSelectionChanged(DependencyObject obj) => (bool) obj.GetValue(UpdatePropertyOnSelectionChangedProperty);

        public static void SetUpdatePropertyOnSelectionChanged(DependencyObject obj, bool value) => obj.SetValue(UpdatePropertyOnSelectionChangedProperty, value.Box());

        private static void SetUpdatingSelection(DependencyObject element, bool value) => element.SetValue(UpdatingSelectionProperty, value.Box());
        private static bool IsUpdatingSelection(DependencyObject element) => (bool) element.GetValue(UpdatingSelectionProperty.DependencyProperty);

        private static void SetIsSelectionChangedRegistered(DependencyObject element, bool value) => element.SetValue(IsSelectionChangedRegisteredProperty, value);

        private static bool GetIsSelectionChangedRegistered(DependencyObject element) => (bool) element.GetValue(IsSelectionChangedRegisteredProperty);

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (IsUpdatingSelection(d))
            {
                return;
            }

            if (d is Selector)
            {
                IList newList = (IList) e.NewValue;
                SetUpdatingSelection(d, true);
                try
                {
                    IList list = null;
                    if (d is ListBox box)
                    {
                        if (box.SelectionMode != SelectionMode.Single)
                            list = box.SelectedItems;
                    }
                    else if (d is MultiSelector ms)
                    {
                        list = ms.SelectedItems;
                    }

                    if (list != null)
                    {
                        list.Clear();
                        if (newList != null && newList.Count > 0)
                        {
                            foreach (object item in newList)
                            {
                                list.Add(item);
                            }
                        }
                    }
                }
                finally
                {
                    SetUpdatingSelection(d, false);
                }
            }

            AutoRegisterSelectionChangedHandler(d);
        }

        private static void AutoRegisterSelectionChangedHandler(DependencyObject d)
        {
            if (d is Selector s)
            {
                if (GetIsSelectionChangedRegistered(d))
                {
                    if (!GetUpdatePropertyOnSelectionChanged(d))
                    {
                        SetIsSelectionChangedRegistered(d, false);
                        s.SelectionChanged -= OnUISelectionChanged;
                    }
                }
                else if (GetUpdatePropertyOnSelectionChanged(d))
                {
                    SetIsSelectionChangedRegistered(d, true);
                    s.SelectionChanged += OnUISelectionChanged;
                }
            }
        }

        private static bool ListEquals(IList a, IList b)
        {
            int cA = a.Count, cB = b.Count;
            if (cA != cB)
            {
                return false;
            }

            for (int i = 0; i < cA; i++)
            {
                if (!ReferenceEquals(a[i], b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static void OnUISelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Selector selector)
            {
                if (IsUpdatingSelection(selector))
                    return;

                IList dstList = GetSelectedItems(selector);
                if (dstList == null)
                    return;

                bool update = false;
                try
                {
                    IList srcList;
                    switch (selector)
                    {
                        case ListBox lb when lb.SelectionMode != SelectionMode.Single:
                            srcList = lb.SelectedItems;
                            break;
                        case MultiSelector ms:
                            srcList = ms.SelectedItems;
                            break;
                        default:
                            srcList = null;
                            break;
                    }

                    if (srcList != null)
                    {
                        // Can massively improve performance for property pages
                        if (ListEquals(srcList, dstList))
                        {
                            return;
                        }

                        SetUpdatingSelection(selector, update = true);
                        if (srcList.Count < 2)
                        {
                            // most likely more efficient to clear and add a possible single selection
                            dstList.Clear();
                            foreach (object item in srcList)
                                dstList.Add(item);
                        }
                        else
                        {
                            int expected = dstList.Count - e.RemovedItems.Count + e.AddedItems.Count;
                            if (expected == srcList.Count)
                            {
                                foreach (object o in e.RemovedItems)
                                    dstList.Remove(o);
                                foreach (object o in e.AddedItems)
                                    dstList.Add(o);
                            }
                            else
                            {
                                Debug.WriteLine($"Selection discrepancy: Expected {expected} selected items in the source list, but got {srcList.Count}");
                                dstList.Clear();
                                foreach (object item in srcList)
                                    dstList.Add(item);
                            }
                        }
                    }
                    else
                    {
                        SetUpdatingSelection(selector, update = true);
                        foreach (object o in e.RemovedItems)
                            dstList.Remove(o);
                        foreach (object o in e.AddedItems)
                            dstList.Add(o);
                    }

                    if (!(dstList is INotifyCollectionChanged))
                        SetSelectedItems(selector, dstList);
                }
                finally
                {
                    if (update)
                        SetUpdatingSelection(selector, false);
                }
            }
        }
    }
}