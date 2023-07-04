namespace Dragablz {
    public class MoveItemRequest {
        private readonly object _item;
        private readonly object _context;
        private readonly AddLocationHint _addLocationHint;

        public MoveItemRequest(object item, object context, AddLocationHint addLocationHint) {
            this._item = item;
            this._context = context;
            this._addLocationHint = addLocationHint;
        }

        public object Item {
            get { return this._item; }
        }

        public object Context {
            get { return this._context; }
        }

        public AddLocationHint AddLocationHint {
            get { return this._addLocationHint; }
        }
    }
}