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

namespace FramePFX.Editors.Controls.TreeViews.Controls
{
    internal interface ISelectionStrategy : IDisposable
    {
        event EventHandler<PreviewSelectionChangedEventArgs> PreviewSelectionChanged;

        void ApplyTemplate();
        bool SelectCore(MultiSelectTreeViewItem owner);
        bool Deselect(MultiSelectTreeViewItem item, bool bringIntoView = false);
        bool SelectPreviousFromKey();
        bool SelectNextFromKey();
        bool SelectFirstFromKey();
        bool SelectLastFromKey();
        bool SelectPageUpFromKey();
        bool SelectPageDownFromKey();
        bool SelectAllFromKey();
        bool SelectParentFromKey();
        bool SelectCurrentBySpace();
        bool Select(MultiSelectTreeViewItem treeViewItem);
    }

    public class PreviewSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets a value indicating whether the item was selected or deselected.
        /// </summary>
        public bool Selecting { get; private set; }

        /// <summary>
        /// Gets the item that is being selected or deselected.
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selection change of this item shall be
        /// cancelled.
        /// </summary>
        public bool CancelThis { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the selection change of this item and all other
        /// affected items shall be cancelled.
        /// </summary>
        public bool CancelAll { get; set; }

        /// <summary>
        /// Gets a value indicating whether any of the Cancel flags is set.
        /// </summary>
        public bool CancelAny { get { return this.CancelThis || this.CancelAll; } }

        public PreviewSelectionChangedEventArgs(bool selecting, object item)
        {
            this.Selecting = selecting;
            this.Item = item;
        }
    }
}