using System;

namespace Dragablz.Dockablz {
    /// <summary>
    /// Provides information about where a tab control is withing a layout structure.
    /// </summary>
    public class LocationReport {
        private readonly TabablzControl _tabablzControl;
        private readonly Layout _rootLayout;
        private readonly Branch _parentBranch;
        private readonly bool _isLeaf;
        private readonly bool _isSecondLeaf;

        //TODO I've internalised constructor for now, so I can come back and add Window without breaking.

        internal LocationReport(TabablzControl tabablzControl, Layout rootLayout)
            : this(tabablzControl, rootLayout, null, false) {
        }

        internal LocationReport(TabablzControl tabablzControl, Layout rootLayout, Branch parentBranch, bool isSecondLeaf) {
            if (tabablzControl == null)
                throw new ArgumentNullException("tabablzControl");
            if (rootLayout == null)
                throw new ArgumentNullException("rootLayout");

            this._tabablzControl = tabablzControl;
            this._rootLayout = rootLayout;
            this._parentBranch = parentBranch;
            this._isLeaf = this._parentBranch != null;
            this._isSecondLeaf = isSecondLeaf;
        }

        public TabablzControl TabablzControl {
            get { return this._tabablzControl; }
        }

        public Layout RootLayout {
            get { return this._rootLayout; }
        }

        /// <summary>
        /// Gets the parent branch if this is a leaf. If the <see cref="TabablzControl"/> is directly under the <see cref="RootLayout"/> will be <c>null</c>.
        /// </summary>
        public Branch ParentBranch {
            get { return this._parentBranch; }
        }

        /// <summary>
        /// Idicates if this is a leaf in a branch. <c>True</c> if <see cref="ParentBranch"/> is not null.
        /// </summary>
        public bool IsLeaf {
            get { return this._isLeaf; }
        }

        public bool IsSecondLeaf {
            get { return this._isSecondLeaf; }
        }
    }
}