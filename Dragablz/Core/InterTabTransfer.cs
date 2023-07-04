using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Dragablz.Dockablz;

namespace Dragablz.Core {
    internal enum InterTabTransferReason {
        Breach,
        Reentry
    }

    internal class InterTabTransfer {
        private readonly object _item;
        private readonly DragablzItem _originatorContainer;
        private readonly Orientation _breachOrientation;
        private readonly Point _dragStartWindowOffset;
        private readonly Point _dragStartItemOffset;
        private readonly Point _itemPositionWithinHeader;
        private readonly Size _itemSize;
        private readonly IList<FloatingItemSnapShot> _floatingItemSnapShots;
        private readonly bool _isTransposing;
        private readonly InterTabTransferReason _transferReason;

        public InterTabTransfer(object item, DragablzItem originatorContainer, Orientation breachOrientation, Point dragStartWindowOffset, Point dragStartItemOffset, Point itemPositionWithinHeader, Size itemSize, IList<FloatingItemSnapShot> floatingItemSnapShots, bool isTransposing) {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (originatorContainer == null)
                throw new ArgumentNullException(nameof(originatorContainer));
            if (floatingItemSnapShots == null)
                throw new ArgumentNullException(nameof(floatingItemSnapShots));

            this._transferReason = InterTabTransferReason.Breach;

            this._item = item;
            this._originatorContainer = originatorContainer;
            this._breachOrientation = breachOrientation;
            this._dragStartWindowOffset = dragStartWindowOffset;
            this._dragStartItemOffset = dragStartItemOffset;
            this._itemPositionWithinHeader = itemPositionWithinHeader;
            this._itemSize = itemSize;
            this._floatingItemSnapShots = floatingItemSnapShots;
            this._isTransposing = isTransposing;
        }

        public InterTabTransfer(
            object item,
            DragablzItem originatorContainer,
            Point dragStartItemOffset,
            IList<FloatingItemSnapShot> floatingItemSnapShots) {
            if (item == null)
                throw new ArgumentNullException(nameof(item));
            if (originatorContainer == null)
                throw new ArgumentNullException(nameof(originatorContainer));
            if (floatingItemSnapShots == null)
                throw new ArgumentNullException(nameof(floatingItemSnapShots));

            this._transferReason = InterTabTransferReason.Reentry;

            this._item = item;
            this._originatorContainer = originatorContainer;
            this._dragStartItemOffset = dragStartItemOffset;
            this._floatingItemSnapShots = floatingItemSnapShots;
        }

        public Orientation BreachOrientation {
            get { return this._breachOrientation; }
        }

        public Point DragStartWindowOffset {
            get { return this._dragStartWindowOffset; }
        }

        public object Item {
            get { return this._item; }
        }

        public DragablzItem OriginatorContainer {
            get { return this._originatorContainer; }
        }

        public InterTabTransferReason TransferReason {
            get { return this._transferReason; }
        }

        public Point DragStartItemOffset {
            get { return this._dragStartItemOffset; }
        }

        public Point ItemPositionWithinHeader {
            get { return this._itemPositionWithinHeader; }
        }

        public Size ItemSize {
            get { return this._itemSize; }
        }

        public IList<FloatingItemSnapShot> FloatingItemSnapShots {
            get { return this._floatingItemSnapShots; }
        }

        public bool IsTransposing {
            get { return this._isTransposing; }
        }
    }
}