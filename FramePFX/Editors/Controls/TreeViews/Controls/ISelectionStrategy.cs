using System;

namespace FramePFX.Editors.Controls.TreeViews.Controls {
    internal interface ISelectionStrategy : IDisposable {
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

    public class PreviewSelectionChangedEventArgs : EventArgs {
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

        public PreviewSelectionChangedEventArgs(bool selecting, object item) {
            this.Selecting = selecting;
            this.Item = item;
        }
    }
}