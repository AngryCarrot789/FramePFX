using System;

namespace Dragablz.Core {
    internal class TabHeaderDragStartInformation {
        private readonly DragablzItem _dragItem;
        private readonly double _dragablzItemsControlHorizontalOffset;
        private readonly double _dragablzItemControlVerticalOffset;
        private readonly double _dragablzItemHorizontalOffset;
        private readonly double _dragablzItemVerticalOffset;

        public TabHeaderDragStartInformation(
            DragablzItem dragItem,
            double dragablzItemsControlHorizontalOffset,
            double dragablzItemControlVerticalOffset,
            double dragablzItemHorizontalOffset,
            double dragablzItemVerticalOffset) {
            if (dragItem == null)
                throw new ArgumentNullException(nameof(dragItem));

            this._dragItem = dragItem;
            this._dragablzItemsControlHorizontalOffset = dragablzItemsControlHorizontalOffset;
            this._dragablzItemControlVerticalOffset = dragablzItemControlVerticalOffset;
            this._dragablzItemHorizontalOffset = dragablzItemHorizontalOffset;
            this._dragablzItemVerticalOffset = dragablzItemVerticalOffset;
        }

        public double DragablzItemsControlHorizontalOffset {
            get { return this._dragablzItemsControlHorizontalOffset; }
        }

        public double DragablzItemControlVerticalOffset {
            get { return this._dragablzItemControlVerticalOffset; }
        }

        public double DragablzItemHorizontalOffset {
            get { return this._dragablzItemHorizontalOffset; }
        }

        public double DragablzItemVerticalOffset {
            get { return this._dragablzItemVerticalOffset; }
        }

        public DragablzItem DragItem {
            get { return this._dragItem; }
        }
    }
}