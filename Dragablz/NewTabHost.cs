using System;
using System.Windows;

namespace Dragablz {
    public class NewTabHost<TElement> : INewTabHost<TElement> where TElement : UIElement {
        private readonly TElement _container;
        private readonly TabablzControl _tabablzControl;

        public NewTabHost(TElement container, TabablzControl tabablzControl) {
            if (container == null)
                throw new ArgumentNullException("container");
            if (tabablzControl == null)
                throw new ArgumentNullException("tabablzControl");

            this._container = container;
            this._tabablzControl = tabablzControl;
        }

        public TElement Container {
            get { return this._container; }
        }

        public TabablzControl TabablzControl {
            get { return this._tabablzControl; }
        }
    }
}