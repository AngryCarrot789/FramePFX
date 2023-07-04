using System;

namespace Dragablz.Dockablz {
    public class BranchResult {
        private readonly Branch _branch;
        private readonly TabablzControl _tabablzControl;

        public BranchResult(Branch branch, TabablzControl tabablzControl) {
            if (branch == null)
                throw new ArgumentNullException("branch");
            if (tabablzControl == null)
                throw new ArgumentNullException("tabablzControl");

            this._branch = branch;
            this._tabablzControl = tabablzControl;
        }

        /// <summary>
        /// The new branch.
        /// </summary>
        public Branch Branch {
            get { return this._branch; }
        }

        /// <summary>
        /// The new tab control.
        /// </summary>
        public TabablzControl TabablzControl {
            get { return this._tabablzControl; }
        }
    }
}