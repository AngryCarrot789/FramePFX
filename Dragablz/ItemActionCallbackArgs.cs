using System;
using System.Windows;

namespace Dragablz {
    public delegate void ItemActionCallback(ItemActionCallbackArgs<TabablzControl> args);

    public class ItemActionCallbackArgs<TOwner> where TOwner : FrameworkElement {
        private readonly Window _window;
        private readonly TOwner _owner;
        private readonly DragablzItem _dragablzItem;

        public ItemActionCallbackArgs(Window window, TOwner owner, DragablzItem dragablzItem) {
            if (window == null)
                throw new ArgumentNullException("window");
            if (owner == null)
                throw new ArgumentNullException("owner");
            if (dragablzItem == null)
                throw new ArgumentNullException("dragablzItem");

            this._window = window;
            this._owner = owner;
            this._dragablzItem = dragablzItem;
        }

        public Window Window {
            get { return this._window; }
        }

        public TOwner Owner {
            get { return this._owner; }
        }

        public DragablzItem DragablzItem {
            get { return this._dragablzItem; }
        }

        public bool IsCancelled { get; private set; }

        public void Cancel() {
            this.IsCancelled = true;
        }
    }
}