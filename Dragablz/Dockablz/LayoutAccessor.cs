using System;
using System.Collections.Generic;

namespace Dragablz.Dockablz {
    /// <summary>
    /// Provides information about the <see cref="Layout"/> instance.
    /// </summary>
    public class LayoutAccessor {
        private readonly Layout _layout;
        private readonly BranchAccessor _branchAccessor;
        private readonly TabablzControl _tabablzControl;

        public LayoutAccessor(Layout layout) {
            if (layout == null)
                throw new ArgumentNullException("layout");

            this._layout = layout;

            var branch = this.Layout.Content as Branch;
            if (branch != null)
                this._branchAccessor = new BranchAccessor(branch);
            else
                this._tabablzControl = this.Layout.Content as TabablzControl;
        }

        public Layout Layout {
            get { return this._layout; }
        }

        public IEnumerable<DragablzItem> FloatingItems {
            get { return this._layout.FloatingDragablzItems(); }
        }

        /// <summary>
        /// <see cref="BranchAccessor"/> and <see cref="TabablzControl"/> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public BranchAccessor BranchAccessor {
            get { return this._branchAccessor; }
        }

        /// <summary>
        /// <see cref="BranchAccessor"/> and <see cref="TabablzControl"/> are mutually exclusive, according to whether the layout has been split, or just contains a tab control.
        /// </summary>
        public TabablzControl TabablzControl {
            get { return this._tabablzControl; }
        }

        /// <summary>
        /// Visits the content of the layout, according to its content type.  No more than one of the provided <see cref="Action"/>
        /// callbacks will be called.  
        /// </summary>        
        public LayoutAccessor Visit(
            Action<BranchAccessor> branchVisitor = null,
            Action<TabablzControl> tabablzControlVisitor = null,
            Action<object> contentVisitor = null) {
            if (this._branchAccessor != null) {
                if (branchVisitor != null) {
                    branchVisitor(this._branchAccessor);
                }

                return this;
            }

            if (this._tabablzControl != null) {
                if (tabablzControlVisitor != null)
                    tabablzControlVisitor(this._tabablzControl);

                return this;
            }

            if (this._layout.Content != null && contentVisitor != null)
                contentVisitor(this._layout.Content);

            return this;
        }

        /// <summary>
        /// Gets all the Tabablz controls in a Layout, regardless of location.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<TabablzControl> TabablzControls() {
            var tabablzControls = new List<TabablzControl>();
            this.Visit(tabablzControls, BranchAccessorVisitor, TabablzControlVisitor);
            return tabablzControls;
        }

        private static void TabablzControlVisitor(IList<TabablzControl> resultSet, TabablzControl tabablzControl) {
            resultSet.Add(tabablzControl);
        }

        private static void BranchAccessorVisitor(IList<TabablzControl> resultSet, BranchAccessor branchAccessor) {
            branchAccessor.Visit(resultSet, BranchItem.First, BranchAccessorVisitor, TabablzControlVisitor).Visit(resultSet, BranchItem.Second, BranchAccessorVisitor, TabablzControlVisitor);
        }
    }
}