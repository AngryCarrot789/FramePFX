// 
// Copyright (c) 2023-2024 REghZy
// 
// This file is part of FramePFX.
// 
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using FramePFX.Utils;

namespace FramePFX.AttachedProperties {
    /// <summary>
    /// A helper class for binding a source collection (typically an observable collection) to a target's selected items, 
    /// with support for two-way selection change. The source list must implement <see cref="INotifyCollectionChanged"/>
    /// </summary>
    public static class ObservableSelectionHelper {
        private static readonly Dictionary<object, List<WeakReference>> SourceToTargetMap;
        private static readonly object SourceToTargetLock;
        private static readonly List<IList> ActiveUpdateList;
        private static readonly NotifyCollectionChangedEventHandler CachedSourceCollectionChanged = OnSourceCollectionChanged;
        private static readonly SelectionChangedEventHandler CachedTargetCollectionChanged = OnTargetCollectionChanged;

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "SelectedItems",
                typeof(IList),
                typeof(ObservableSelectionHelper),
                new FrameworkPropertyMetadata(null, OnSelectedItemsChanged));

        // I'm using this instead of removing the CollectionChanged handler of the source list,
        // because that operations is more intensive as it has to scan the multimap of weak references

        private static readonly DependencyPropertyKey IsProcessingSourceSelectionChangedPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly(
                "IsProcessingSourceSelectionChanged",
                typeof(bool),
                typeof(ObservableSelectionHelper),
                new FrameworkPropertyMetadata(BoolBox.False, FrameworkPropertyMetadataOptions.NotDataBindable));

        private static readonly DependencyProperty IsProcessingSourceSelectionChangedProperty;

        static ObservableSelectionHelper() {
            SourceToTargetMap = new Dictionary<object, List<WeakReference>>();
            SourceToTargetLock = new object();
            IsProcessingSourceSelectionChangedProperty = IsProcessingSourceSelectionChangedPropertyKey.DependencyProperty;
            ActiveUpdateList = new List<IList>();
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            if (ReferenceEquals(e.OldValue, e.NewValue)) {
                return;
            }

            Selector target;
            IList targetList;
            if (d is ListBox lb) {
                if (lb.SelectionMode == SelectionMode.Single)
                    throw new Exception("List box's SelectionMode is not set to multiple or extended");
                targetList = lb.SelectedItems;
                target = lb;
            }
            else if (d is MultiSelector ms) {
                targetList = ms.SelectedItems;
                target = ms;
            }
            else {
                throw new Exception("Unsupported target type: " + d.GetType());
            }

            IList sourceList = (IList) e.NewValue;
            if (sourceList != null && !(sourceList is INotifyCollectionChanged)) {
                throw new Exception($"Source collection must implement " + nameof(INotifyCollectionChanged));
            }

            // Fun fact: Selector uses an observable collection for the internal SelectedItems property

            if (e.OldValue != null) {
                UpdateSourceEventHandler(target, e.OldValue, false);
                UpdateTargetEventHandler(target, false);
            }

            if (sourceList != null) {
                if (!ListEquals(targetList, sourceList)) {
                    targetList.Clear();
                    foreach (object item in sourceList) {
                        targetList.Add(item);
                    }
                }

                UpdateSourceEventHandler(target, sourceList, true);
                UpdateTargetEventHandler(target, true);
            }
        }

        private static void UpdateTargetEventHandler(DependencyObject target, bool connect) {
            if (connect) {
                ((Selector) target).SelectionChanged += CachedTargetCollectionChanged;
            }
            else {
                ((Selector) target).SelectionChanged -= CachedTargetCollectionChanged;
            }
        }

        private static void UpdateSourceEventHandler(DependencyObject target, object sourceList, bool connect) {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            lock (SourceToTargetLock) {
                if (connect) {
                    ((INotifyCollectionChanged) sourceList).CollectionChanged += CachedSourceCollectionChanged;
                    AddTargetHandler(target, sourceList);
                }
                else {
                    ((INotifyCollectionChanged) sourceList).CollectionChanged -= CachedSourceCollectionChanged;
                    RemoveTargetHandler(target, sourceList);
                }
            }
        }

        private static void RemoveTargetHandler(DependencyObject target, object sourceList) {
            if (SourceToTargetMap.TryGetValue(sourceList, out List<WeakReference> references)) {
                for (int i = references.Count - 1; i >= 0; i--) {
                    object referenceTarget = references[i].Target;
                    if (ReferenceEquals(referenceTarget, target)) {
                        references.RemoveAt(i);
                    }
                }
            }
        }

        private static void AddTargetHandler(DependencyObject target, object sourceList) {
            if (!SourceToTargetMap.TryGetValue(sourceList, out List<WeakReference> references)) {
                SourceToTargetMap[sourceList] = references = new List<WeakReference>();
            }
            else if (references.Count > 0) {
                for (int i = references.Count - 1; i >= 0; i--) {
                    object referenceTarget = references[i].Target;
                    if (ReferenceEquals(referenceTarget, target)) {
                        return;
                    }
                }
            }

            references.Add(new WeakReference(target));
        }

        // SOURCE: A data object that typically raises property and data changed notifications
        // TARGET: A dependency object (typically), e.g. ListBox

        // A view model's observable collection changed
        private static void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            // Here we need to get the binding target(s) from the source
            if (!DispatcherUtils.IsOnMainThread()) {
                // log just in case the exception gets eaten up by a black hole
                throw new Exception("Cannot process observable collection changes while off the main thread");
            }

            IList sourceList = (IList) sender;
            for (int i = ActiveUpdateList.Count - 1; i >= 0; i--) {
                if (ReferenceEquals(ActiveUpdateList[i], sourceList)) {
                    return;
                }
            }

            List<DependencyObject> targets;
            lock (SourceToTargetLock) {
                if (!SourceToTargetMap.TryGetValue(sender, out List<WeakReference> references) || references.Count < 1) {
                    return;
                }

                targets = null;
                for (int i = 0; i < references.Count; i++) {
                    DependencyObject target = (DependencyObject) references[i].Target;
                    if (target != null) {
                        if (!GetIsProcessingSourceSelectionChanged(target)) {
                            (targets ?? (targets = new List<DependencyObject>(1))).Add(target);
                        }
                        else {
                            // ooops
                        }
                    }
                    else {
                        references.RemoveAt(i--);
                    }
                }
            }

            if (targets == null) {
                return;
            }

            // Here, we update the target's selection from the source list
            // This would require disconnecting selection changed events
            using (ErrorList list = new ErrorList("Failed to handle observable collection changed event", false)) {
                foreach (DependencyObject target in targets) {
                    UpdateTargetEventHandler(target, false);
                    try {
                        IList targetList;
                        if (target is ListBox listBox) {
                            if (listBox.SelectionMode == SelectionMode.Single) {
                                if (sourceList.Count < 1) {
                                    listBox.ClearValue(Selector.SelectedItemProperty);
                                }
                                else {
                                    object firstNonNull = null;
                                    for (int i = 0; i < sourceList.Count; i++) {
                                        if ((firstNonNull = sourceList[i]) != null)
                                            break;
                                    }

                                    listBox.SelectedItem = firstNonNull;
                                }

                                continue;
                            }
                            else {
                                targetList = listBox.SelectedItems;
                            }
                        }
                        else if (target is MultiSelector ms) {
                            targetList = ms.SelectedItems;
                        }
                        else {
                            continue;
                        }

                        // The code below has to handle the cases when the source selected items are not actually
                        // contained in the target object's items collection, and therefore, adding them to the
                        // target's selection list wouldn't really do anything anyway.

                        // There isn't really an efficient way to do this. WPF internally has helper 'assumption'
                        // flags which can, but apart from that, you have to lookup each item new selected item
                        // in the target's items collection to see if they can actually be selected

                        // So this code will just do some possible preconditional checks, and if they succeed,
                        // it lets WPF handle those cases where the items cannot be selected
                        int targetListCount = targetList.Count;
                        switch (e.Action) {
                            case NotifyCollectionChangedAction.Add: {
                                int index = e.NewStartingIndex;
                                if (index == -1) {
                                    index = targetListCount;
                                }

                                if (index > targetListCount) {
                                    // weird selection setup; possibly bound a MultiSelectTreeView to a ListBox?
                                    foreach (object item in e.NewItems) {
                                        targetList.Add(item);
                                    }
                                }
                                else {
                                    foreach (object item in e.NewItems) {
                                        targetList.Insert(index++, item);
                                    }
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Remove: {
                                if (targetListCount < 1) {
                                    break;
                                }

                                int index = e.OldStartingIndex;
                                foreach (object item in e.OldItems) {
                                    if (index != -1 && index < targetList.Count && ReferenceEquals(targetList[index], item)) {
                                        targetList.RemoveAt(index);
                                    }
                                    else {
                                        targetList.Remove(item);
                                    }
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Replace: {
                                if (targetListCount < 1) {
                                    break;
                                }

                                int rmvidx = e.OldStartingIndex;
                                foreach (object item in e.OldItems) {
                                    if (rmvidx != -1 && rmvidx < targetList.Count && ReferenceEquals(targetList[rmvidx], item)) {
                                        targetList.RemoveAt(rmvidx);
                                    }
                                    else {
                                        targetList.Remove(item);
                                    }
                                }

                                int addidx = e.NewStartingIndex;
                                foreach (object item in e.NewItems) {
                                    if (addidx != -1 && addidx < targetList.Count) {
                                        targetList.Insert(addidx++, item);
                                    }
                                    else {
                                        targetList.Add(item);
                                    }
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Move: {
                                int i = e.OldStartingIndex, j = e.NewStartingIndex;
                                if (i == -1 || j == -1 || i > targetListCount || j > targetListCount) {
                                    break;
                                }

                                foreach (object item in e.NewItems) {
                                    if (!ReferenceEquals(targetList[i], item))
                                        Debug.WriteLine("Possibly corrupt selected items for move operation: indices do not match");
                                    CollectionUtils.MoveItem(targetList, i++, j++);
                                }

                                break;
                            }
                            case NotifyCollectionChangedAction.Reset:
                                if (targetListCount > 0)
                                    targetList.Clear();
                                break;
                            default: throw new ArgumentOutOfRangeException();
                        }
                    }
                    catch (Exception exception) {
                        list.Add(new Exception($"Failed to update target selection for '{target}' ({target.GetType()})", exception));
                    }
                    finally {
                        UpdateTargetEventHandler(target, true);
                    }
                }

                if (list.TryGetException(out Exception error)) {
                    IoC.MessageService.ShowMessage("Error", "An exception occurred while processing selection change. " +
                                                            "This may have corrupted the application in some way, so please restart.\n\n" +
                                                            "See the app logs for more info");
                }
            }
        }

        // A list box's selection changed
        private static void OnTargetCollectionChanged(object sender, SelectionChangedEventArgs e) {
            if (!(sender is Selector selector)) {
                throw new Exception("Unsupported sender object for SelectionChanged event");
            }

            IList dataSourceList = GetSelectedItems(selector);
            if (dataSourceList == null) {
                return;
            }

            IList selectorList;
            if (selector is ListBox list) {
                if (list.SelectionMode == SelectionMode.Single) {
                    object selection = list.SelectedItem;
                    if (selection != null) {
                        if (dataSourceList.Count == 1 && Equals(dataSourceList[0], selection)) {
                            return;
                        }
                    }
                    else if (dataSourceList.Count < 1) {
                        return;
                    }

                    SetIsProcessingSourceSelectionChanged(selector, dataSourceList, true);
                    // UpdateSourceEventHandler(selector, dataSourceList, false);
                    if (dataSourceList.Count > 0)
                        dataSourceList.Clear();
                    if (selection != null)
                        dataSourceList.Add(selection);
                    // UpdateSourceEventHandler(selector, dataSourceList, true);
                    SetIsProcessingSourceSelectionChanged(selector, dataSourceList, false);
                    return;
                }
                else {
                    selectorList = list.SelectedItems;
                }
            }
            else if (selector is MultiSelector ms) {
                selectorList = ms.SelectedItems;
            }
            else {
                selectorList = null;
            }

            if (selectorList == null) {
                SetIsProcessingSourceSelectionChanged(selector, dataSourceList, true);
                // UpdateSourceEventHandler(selector, dataSourceList, false);
                try {
                    foreach (object o in e.RemovedItems)
                        dataSourceList.Remove(o);
                    foreach (object o in e.AddedItems)
                        dataSourceList.Add(o);
                }
                finally {
                    SetIsProcessingSourceSelectionChanged(selector, dataSourceList, false);
                    // UpdateSourceEventHandler(selector, dataSourceList, true);
                }

                return;
            }

            // Can massively improve performance for property pages
            if (ListEquals(dataSourceList, selectorList)) {
                return;
            }

            SetIsProcessingSourceSelectionChanged(selector, dataSourceList, true);
            // UpdateSourceEventHandler(selector, dataSourceList, false);
            try {
                if (selectorList.Count < 2) {
                    // most likely more efficient to clear and add a possible single selection
                    if (dataSourceList.Count > 0)
                        dataSourceList.Clear();
                    foreach (object item in selectorList)
                        dataSourceList.Add(item);
                }
                else {
                    int expected = dataSourceList.Count - e.RemovedItems.Count + e.AddedItems.Count;
                    if (expected == selectorList.Count) {
                        foreach (object o in e.RemovedItems)
                            dataSourceList.Remove(o);
                        foreach (object o in e.AddedItems)
                            dataSourceList.Add(o);
                    }
                    else {
                        Debug.WriteLine($"Selection discrepancy: Expected {expected} selected items in the source list, but got {selectorList.Count}");
                        dataSourceList.Clear();
                        foreach (object item in selectorList)
                            dataSourceList.Add(item);
                    }
                }
            }
            finally {
                SetIsProcessingSourceSelectionChanged(selector, dataSourceList, false);
                // UpdateSourceEventHandler(selector, dataSourceList, true);
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

        public static IList GetSelectedItems(DependencyObject obj) {
            return (IList) obj.GetValue(SelectedItemsProperty);
        }

        public static void SetSelectedItems(DependencyObject obj, IList value) {
            obj.SetValue(SelectedItemsProperty, value);
        }

        private static void SetIsProcessingSourceSelectionChanged(DependencyObject element, IList sourceList, bool value) {
            if (value) {
                element.SetValue(IsProcessingSourceSelectionChangedPropertyKey, BoolBox.True);
                foreach (IList list in ActiveUpdateList)
                    if (ReferenceEquals(sourceList, list))
                        return;
                ActiveUpdateList.Add(sourceList);
            }
            else {
                element.SetValue(IsProcessingSourceSelectionChangedPropertyKey, BoolBox.False);
                for (int i = ActiveUpdateList.Count - 1; i >= 0; i--) {
                    if (ReferenceEquals(sourceList, ActiveUpdateList[i])) {
                        ActiveUpdateList.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        private static bool GetIsProcessingSourceSelectionChanged(DependencyObject element) {
            return (bool) element.GetValue(IsProcessingSourceSelectionChangedProperty);
        }
    }
}