using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Dragablz {
    public delegate void DragablzItemEventHandler(object sender, DragablzItemEventArgs e);

    public class DragablzItemEventArgs : RoutedEventArgs {
        private readonly DragablzItem _dragablzItem;

        public DragablzItemEventArgs(DragablzItem dragablzItem) {
            if (dragablzItem == null)
                throw new ArgumentNullException("dragablzItem");

            this._dragablzItem = dragablzItem;
        }

        public DragablzItemEventArgs(RoutedEvent routedEvent, DragablzItem dragablzItem)
            : base(routedEvent) {
            this._dragablzItem = dragablzItem;
        }

        public DragablzItemEventArgs(RoutedEvent routedEvent, object source, DragablzItem dragablzItem)
            : base(routedEvent, source) {
            this._dragablzItem = dragablzItem;
        }

        public DragablzItem DragablzItem {
            get { return this._dragablzItem; }
        }
    }
}