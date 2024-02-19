/*
 * The MIT License (MIT)
 *
 * Copyright (c) 2012 Yves Goergen, Goroll
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the
 * rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
 * sell copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
 * A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FramePFX.Editors.Controls.TreeViews.Controls {
    /// <summary>
    /// Implements the logic for the multiple selection strategy.
    /// </summary>
    public class SelectionMultiple : ISelectionStrategy {
        private readonly MultiSelectTreeView treeView;
        private BorderSelectionLogic borderSelectionLogic;
        private MultiSelectTreeViewItem lastShiftRoot;

        public SelectionMultiple(MultiSelectTreeView treeView) {
            this.treeView = treeView;
        }

        public event EventHandler<PreviewSelectionChangedEventArgs> PreviewSelectionChanged;

        public void ApplyTemplate() {
            this.borderSelectionLogic = new BorderSelectionLogic(
                this.treeView,
                this.treeView.Template.FindName("selectionBorder", this.treeView) as Border,
                this.treeView.Template.FindName("scrollViewer", this.treeView) as ScrollViewer,
                this.treeView.Template.FindName("content", this.treeView) as ItemsPresenter);
        }

        public bool Select(MultiSelectTreeViewItem item) {
            if (IsControlKeyDown) {
                if (this.treeView.SelectedItems.Contains(item)) {
                    return this.Deselect(item, true);
                }
                else {
                    PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, item);
                    this.OnPreviewSelectionChanged(e);
                    if (e.CancelAny) {
                        FocusHelper.Focus(item, true);
                        return false;
                    }

                    return this.SelectCore(item);
                }
            }
            else {
                if (this.treeView.SelectedItems.Count == 1 && this.treeView.SelectedItems[0] == item) {
                    // Requested to select the single already-selected item. Don't change the selection.
                    FocusHelper.Focus(item, true);
                    this.lastShiftRoot = item;
                    return true;
                }
                else {
                    return this.SelectCore(item);
                }
            }
        }

        public bool SelectCore(MultiSelectTreeViewItem item) {
            IList selection = this.treeView.SelectedItems;
            if (IsControlKeyDown) {
                if (!selection.Contains(item)) {
                    selection.Add(item);
                }

                this.lastShiftRoot = item;
            }
            else if (IsShiftKeyDown && selection.Count > 0) {
                MultiSelectTreeViewItem firstSelectedItem = (MultiSelectTreeViewItem) (this.lastShiftRoot ?? selection.First());
                List<MultiSelectTreeViewItem> newSelection = this.treeView.GetNodesToSelectBetween(firstSelectedItem, item).Select(n => n).ToList();
                // Make a copy of the list because we're modifying it while enumerating it
                object[] selectedItems = new object[selection.Count];
                selection.CopyTo(selectedItems, 0);
                // Remove all items no longer selected
                foreach (object selItem in selectedItems.Where(i => !newSelection.Contains(i))) {
                    PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(false, selItem);
                    this.OnPreviewSelectionChanged(e);
                    if (e.CancelAll) {
                        FocusHelper.Focus(item);
                        return false;
                    }

                    if (!e.CancelThis) {
                        selection.Remove(selItem);
                    }
                }

                // Add new selected items
                foreach (MultiSelectTreeViewItem newItem in newSelection.Where(i => !selectedItems.Contains(i))) {
                    PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, newItem);
                    this.OnPreviewSelectionChanged(e);
                    if (e.CancelAll) {
                        FocusHelper.Focus(item, true);
                        return false;
                    }

                    if (!e.CancelThis) {
                        selection.Add(newItem);
                    }
                }
            }
            else {
                if (selection.Count > 0) {
                    foreach (MultiSelectTreeViewItem selItem in new ArrayList(selection)) {
                        PreviewSelectionChangedEventArgs e2 = new PreviewSelectionChangedEventArgs(false, selItem);
                        this.OnPreviewSelectionChanged(e2);
                        if (e2.CancelAll) {
                            FocusHelper.Focus(item);
                            this.lastShiftRoot = item;
                            return false;
                        }

                        if (!e2.CancelThis) {
                            selection.Remove(selItem);
                        }
                    }
                }

                PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, item);
                this.OnPreviewSelectionChanged(e);
                if (e.CancelAny) {
                    FocusHelper.Focus(item, true);
                    this.lastShiftRoot = item;
                    return false;
                }

                selection.Add(item);
                this.lastShiftRoot = item;
            }

            FocusHelper.Focus(item, true);
            return true;
        }

        public bool SelectCurrentBySpace() {
            // Another item was focused by Ctrl+Arrow key
            MultiSelectTreeViewItem item = this.GetFocusedItem();
            if (this.treeView.SelectedItems.Contains(item)) {
                // With Ctrl key, toggle this item selection (deselect now).
                // Without Ctrl key, always select it (is already selected).
                if (IsControlKeyDown) {
                    if (!this.Deselect(item, true))
                        return false;
                    item.IsSelected = false;
                }
            }
            else {
                PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, item);
                this.OnPreviewSelectionChanged(e);
                if (e.CancelAny) {
                    FocusHelper.Focus(item, true);
                    return false;
                }

                item.IsSelected = true;
                if (!this.treeView.SelectedItems.Contains(item)) {
                    this.treeView.SelectedItems.Add(item);
                }
            }

            FocusHelper.Focus(item, true);
            return true;
        }

        public bool SelectNextFromKey() {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            MultiSelectTreeViewItem item = MultiSelectTreeView.GetNextItem(this.GetFocusedItem(), items);
            return this.SelectFromKey(item);
        }

        public bool SelectPreviousFromKey() {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            MultiSelectTreeViewItem item = MultiSelectTreeView.GetPreviousItem(this.GetFocusedItem(), items);
            return this.SelectFromKey(item);
        }

        public bool SelectFirstFromKey() {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            MultiSelectTreeViewItem item = MultiSelectTreeView.GetFirstItem(items);
            return this.SelectFromKey(item);
        }

        public bool SelectLastFromKey() {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            MultiSelectTreeViewItem item = MultiSelectTreeView.GetLastItem(items);
            return this.SelectFromKey(item);
        }

        public bool SelectPageUpFromKey() {
            return this.SelectPageUpDown(false);
        }

        public bool SelectPageDownFromKey() {
            return this.SelectPageUpDown(true);
        }

        public bool SelectAllFromKey() {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            // Add new selected items
            foreach (MultiSelectTreeViewItem item in items.Where(i => !this.treeView.SelectedItems.Contains(i))) {
                PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, item);
                this.OnPreviewSelectionChanged(e);
                if (e.CancelAll) {
                    return false;
                }

                if (!e.CancelThis) {
                    this.treeView.SelectedItems.Add(item);
                }
            }

            return true;
        }

        public bool SelectParentFromKey() {
            DependencyObject parent = this.GetFocusedItem();
            while (parent != null) {
                parent = VisualTreeHelper.GetParent(parent);
                if (parent is MultiSelectTreeViewItem)
                    break;
            }

            return this.SelectFromKey(parent as MultiSelectTreeViewItem);
        }

        public bool Deselect(MultiSelectTreeViewItem item, bool bringIntoView = false) {
            PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(false, item);
            this.OnPreviewSelectionChanged(e);
            if (e.CancelAny)
                return false;

            this.treeView.SelectedItems.Remove(item);
            if (item == this.lastShiftRoot) {
                this.lastShiftRoot = null;
            }

            FocusHelper.Focus(item, bringIntoView);
            return true;
        }

        public void Dispose() {
            if (this.borderSelectionLogic != null) {
                this.borderSelectionLogic.Dispose();
                this.borderSelectionLogic = null;
            }

            GC.SuppressFinalize(this);
        }

        public void InvalidateLastShiftRoot(object item) {
            if (this.lastShiftRoot == item) {
                this.lastShiftRoot = null;
            }
        }

        internal bool SelectByRectangle(MultiSelectTreeViewItem item) {
            PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(true, item);
            this.OnPreviewSelectionChanged(e);
            if (e.CancelAny) {
                this.lastShiftRoot = item;
                return false;
            }

            if (!this.treeView.SelectedItems.Contains(item)) {
                this.treeView.SelectedItems.Add(item);
            }

            this.lastShiftRoot = item;
            return true;
        }

        internal bool DeselectByRectangle(MultiSelectTreeViewItem item) {
            PreviewSelectionChangedEventArgs e = new PreviewSelectionChangedEventArgs(false, item);
            this.OnPreviewSelectionChanged(e);
            if (e.CancelAny) {
                this.lastShiftRoot = item;
                return false;
            }

            this.treeView.SelectedItems.Remove(item);
            if (item == this.lastShiftRoot) {
                this.lastShiftRoot = null;
            }

            return true;
        }

        private MultiSelectTreeViewItem GetFocusedItem() {
            return MultiSelectTreeView.EnumerableTreeRecursiveFirst(x => x.IsFocused, this.treeView, false, false);
        }

        private bool SelectFromKey(MultiSelectTreeViewItem item) {
            if (item == null) {
                return false;
            }

            // If Ctrl is pressed just focus it, so it can be selected by Space. Otherwise select it.
            if (IsControlKeyDown) {
                FocusHelper.Focus(item, true);
                return true;
            }
            else {
                return this.SelectCore(item);
            }
        }

        private bool SelectPageUpDown(bool down) {
            List<MultiSelectTreeViewItem> items = MultiSelectTreeView.GetEntireTreeRecursive(this.treeView, false, false);
            MultiSelectTreeViewItem item = this.GetFocusedItem();
            if (item == null) {
                return down ? this.SelectLastFromKey() : this.SelectFirstFromKey();
            }

            double targetY = item.TransformToAncestor(this.treeView).Transform(new Point()).Y;
            FrameworkElement itemContent = (FrameworkElement) item.Template.FindName("headerBorder", item);
            double offset = this.treeView.ActualHeight - 2 * itemContent.ActualHeight;
            if (!down)
                offset = -offset;
            targetY += offset;
            while (true) {
                MultiSelectTreeViewItem newItem = down ? MultiSelectTreeView.GetNextItem(item, items) : MultiSelectTreeView.GetPreviousItem(item, items);
                if (newItem == null)
                    break;
                item = newItem;
                double itemY = item.TransformToAncestor(this.treeView).Transform(new Point()).Y;
                if (down && itemY > targetY ||
                    !down && itemY < targetY) {
                    break;
                }
            }

            return this.SelectFromKey(item);
        }

        protected void OnPreviewSelectionChanged(PreviewSelectionChangedEventArgs e) {
            EventHandler<PreviewSelectionChangedEventArgs> handler = this.PreviewSelectionChanged;
            if (handler != null) {
                handler(this, e);
                this.LastCancelAll = e.CancelAll;
            }
        }

        #region Properties

        public bool LastCancelAll { get; private set; }

        internal static bool IsControlKeyDown {
            get {
                return (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control;
            }
        }

        private static bool IsShiftKeyDown {
            get {
                return (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift;
            }
        }

        #endregion
    }
}