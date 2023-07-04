using System;

namespace Dragablz.Dockablz {
    internal class LocationReportBuilder {
        private readonly TabablzControl _targetTabablzControl;
        private Branch _branch;
        private bool _isSecondLeaf;
        private Layout _layout;

        public LocationReportBuilder(TabablzControl targetTabablzControl) {
            this._targetTabablzControl = targetTabablzControl;
        }

        public TabablzControl TargetTabablzControl {
            get { return this._targetTabablzControl; }
        }

        public bool IsFound { get; private set; }

        public void MarkFound() {
            if (this.IsFound)
                throw new InvalidOperationException("Already found.");

            this.IsFound = true;

            this._layout = this.CurrentLayout;
        }

        public void MarkFound(Branch branch, bool isSecondLeaf) {
            if (branch == null)
                throw new ArgumentNullException("branch");
            if (this.IsFound)
                throw new InvalidOperationException("Already found.");

            this.IsFound = true;

            this._layout = this.CurrentLayout;
            this._branch = branch;
            this._isSecondLeaf = isSecondLeaf;
        }

        public Layout CurrentLayout { get; set; }

        public LocationReport ToLocationReport() {
            return new LocationReport(this._targetTabablzControl, this._layout, this._branch, this._isSecondLeaf);
        }
    }
}