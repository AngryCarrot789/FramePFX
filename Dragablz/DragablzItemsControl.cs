using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using Dragablz.Core;

namespace Dragablz {
    /// <summary>
    /// Items control which typically uses a canvas and 
    /// </summary>
    public class DragablzItemsControl : ItemsControl {
        private object[] _previousSortQueryResult;

        static DragablzItemsControl() {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DragablzItemsControl), new FrameworkPropertyMetadata(typeof(DragablzItemsControl)));
        }

        public DragablzItemsControl() {
            this.ItemContainerGenerator.StatusChanged += this.ItemContainerGeneratorOnStatusChanged;
            this.ItemContainerGenerator.ItemsChanged += this.ItemContainerGeneratorOnItemsChanged;
            this.AddHandler(DragablzItem.XChangedEvent, new RoutedPropertyChangedEventHandler<double>(this.ItemXChanged));
            this.AddHandler(DragablzItem.YChangedEvent, new RoutedPropertyChangedEventHandler<double>(this.ItemYChanged));
            this.AddHandler(DragablzItem.DragDelta, new DragablzDragDeltaEventHandler(this.ItemDragDelta));
            this.AddHandler(DragablzItem.DragCompleted, new DragablzDragCompletedEventHandler(this.ItemDragCompleted));
            this.AddHandler(DragablzItem.DragStarted, new DragablzDragStartedEventHandler(this.ItemDragStarted));
            this.AddHandler(DragablzItem.MouseDownWithinEvent, new DragablzItemEventHandler(this.ItemMouseDownWithinHandlerTarget));
        }

        public static readonly DependencyProperty FixedItemCountProperty = DependencyProperty.Register(
            "FixedItemCount", typeof(int), typeof(DragablzItemsControl), new PropertyMetadata(default(int)));

        public int FixedItemCount {
            get { return (int) this.GetValue(FixedItemCountProperty); }
            set { this.SetValue(FixedItemCountProperty, value); }
        }

        private void ItemContainerGeneratorOnItemsChanged(object sender, ItemsChangedEventArgs itemsChangedEventArgs) {
            //throw new NotImplementedException();
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            if (this.ContainerCustomisations != null && this.ContainerCustomisations.ClearingContainerForItemOverride != null)
                this.ContainerCustomisations.ClearingContainerForItemOverride(element, item);

            base.ClearContainerForItemOverride(element, item);

            ((DragablzItem) element).SizeChanged -= this.ItemSizeChangedEventHandler;

            this.Dispatcher.BeginInvoke(new Action(() => {
                var dragablzItems = this.DragablzItems().ToList();
                if (this.ItemsOrganiser == null)
                    return;
                this.ItemsOrganiser.Organise(this, new Size(this.ItemsPresenterWidth, this.ItemsPresenterHeight), dragablzItems);
                var measure = this.ItemsOrganiser.Measure(this, new Size(this.ActualWidth, this.ActualHeight), dragablzItems);
                this.ItemsPresenterWidth = measure.Width;
                this.ItemsPresenterHeight = measure.Height;
            }), DispatcherPriority.Input);
        }

        public static readonly DependencyProperty ItemsOrganiserProperty = DependencyProperty.Register(
            "ItemsOrganiser", typeof(IItemsOrganiser), typeof(DragablzItemsControl), new PropertyMetadata(default(IItemsOrganiser)));

        public IItemsOrganiser ItemsOrganiser {
            get { return (IItemsOrganiser) this.GetValue(ItemsOrganiserProperty); }
            set { this.SetValue(ItemsOrganiserProperty, value); }
        }

        public static readonly DependencyProperty PositionMonitorProperty = DependencyProperty.Register(
            "PositionMonitor", typeof(PositionMonitor), typeof(DragablzItemsControl), new PropertyMetadata(default(PositionMonitor)));

        public PositionMonitor PositionMonitor {
            get { return (PositionMonitor) this.GetValue(PositionMonitorProperty); }
            set { this.SetValue(PositionMonitorProperty, value); }
        }

        private static readonly DependencyPropertyKey ItemsPresenterWidthPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ItemsPresenterWidth", typeof(double), typeof(DragablzItemsControl),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty ItemsPresenterWidthProperty =
            ItemsPresenterWidthPropertyKey.DependencyProperty;

        public double ItemsPresenterWidth {
            get { return (double) this.GetValue(ItemsPresenterWidthProperty); }
            private set { this.SetValue(ItemsPresenterWidthPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey ItemsPresenterHeightPropertyKey =
            DependencyProperty.RegisterReadOnly(
                "ItemsPresenterHeight", typeof(double), typeof(DragablzItemsControl),
                new PropertyMetadata(default(double)));

        public static readonly DependencyProperty ItemsPresenterHeightProperty =
            ItemsPresenterHeightPropertyKey.DependencyProperty;

        public double ItemsPresenterHeight {
            get { return (double) this.GetValue(ItemsPresenterHeightProperty); }
            private set { this.SetValue(ItemsPresenterHeightPropertyKey, value); }
        }

        /// <summary>
        /// Adds an item to the underlying source, displaying in a specific position in rendered control.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="addLocationHint"></param>
        public void AddToSource(object item, AddLocationHint addLocationHint) {
            this.AddToSource(item, null, addLocationHint);
        }

        /// <summary>
        /// Adds an item to the underlying source, displaying in a specific position in rendered control.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="nearItem"></param>
        /// <param name="addLocationHint"></param>
        public void AddToSource(object item, object nearItem, AddLocationHint addLocationHint) {
            CollectionTeaser collectionTeaser;
            if (CollectionTeaser.TryCreate(this.ItemsSource, out collectionTeaser))
                collectionTeaser.Add(item);
            else
                this.Items.Add(item);
            this.MoveItem(new MoveItemRequest(item, nearItem, addLocationHint));
        }

        internal ContainerCustomisations ContainerCustomisations { get; set; }

        private void ItemContainerGeneratorOnStatusChanged(object sender, EventArgs eventArgs) {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return;

            this.InvalidateMeasure();
            //extra kick
            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure), DispatcherPriority.Loaded);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            var dragablzItem = item as DragablzItem;
            if (dragablzItem == null)
                return false;

            return true;
        }

        protected override DependencyObject GetContainerForItemOverride() {
            var result = this.ContainerCustomisations != null && this.ContainerCustomisations.GetContainerForItemOverride != null
                ? this.ContainerCustomisations.GetContainerForItemOverride()
                : new DragablzItem();

            result.SizeChanged += this.ItemSizeChangedEventHandler;

            return result;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            if (this.ContainerCustomisations != null && this.ContainerCustomisations.PrepareContainerForItemOverride != null)
                this.ContainerCustomisations.PrepareContainerForItemOverride(element, item);

            base.PrepareContainerForItemOverride(element, item);
        }

        protected override Size MeasureOverride(Size constraint) {
            if (this.ItemsOrganiser == null)
                return base.MeasureOverride(constraint);

            if (this.LockedMeasure.HasValue) {
                this.ItemsPresenterWidth = this.LockedMeasure.Value.Width;
                this.ItemsPresenterHeight = this.LockedMeasure.Value.Height;
                return this.LockedMeasure.Value;
            }

            var dragablzItems = this.DragablzItems().ToList();
            var maxConstraint = new Size(double.PositiveInfinity, double.PositiveInfinity);

            this.ItemsOrganiser.Organise(this, maxConstraint, dragablzItems);
            var measure = this.ItemsOrganiser.Measure(this, new Size(this.ActualWidth, this.ActualHeight), dragablzItems);

            this.ItemsPresenterWidth = measure.Width;
            this.ItemsPresenterHeight = measure.Height;

            var width = double.IsInfinity(constraint.Width) ? measure.Width : constraint.Width;
            var height = double.IsInfinity(constraint.Height) ? measure.Height : constraint.Height;

            return new Size(width, height);
        }

        internal void InstigateDrag(object item, Action<DragablzItem> continuation) {
            var dragablzItem = (DragablzItem) this.ItemContainerGenerator.ContainerFromItem(item);
            dragablzItem.InstigateDrag(continuation);
        }

        /// <summary>
        /// Move an item in the rendered layout.
        /// </summary>
        /// <param name="moveItemRequest"></param>
        public void MoveItem(MoveItemRequest moveItemRequest) {
            if (moveItemRequest == null)
                throw new ArgumentNullException("moveItemRequest");

            if (this.ItemsOrganiser == null)
                return;

            var dragablzItem = moveItemRequest.Item as DragablzItem ?? this.ItemContainerGenerator.ContainerFromItem(
                                   moveItemRequest.Item) as DragablzItem;
            var contextDragablzItem = moveItemRequest.Context as DragablzItem ?? this.ItemContainerGenerator.ContainerFromItem(
                                          moveItemRequest.Context) as DragablzItem;

            if (dragablzItem == null)
                return;

            var sortedItems = this.DragablzItems().OrderBy(di => di.LogicalIndex).ToList();
            sortedItems.Remove(dragablzItem);

            switch (moveItemRequest.AddLocationHint) {
                case AddLocationHint.First:
                    sortedItems.Insert(0, dragablzItem);
                    break;
                case AddLocationHint.Last:
                    sortedItems.Add(dragablzItem);
                    break;
                case AddLocationHint.Prior:
                case AddLocationHint.After:
                    if (contextDragablzItem == null)
                        return;

                    var contextIndex = sortedItems.IndexOf(contextDragablzItem);
                    sortedItems.Insert(moveItemRequest.AddLocationHint == AddLocationHint.Prior ? contextIndex : contextIndex + 1, dragablzItem);

                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            //TODO might not be too great for perf on larger lists
            var orderedEnumerable = sortedItems.OrderBy(di => sortedItems.IndexOf(di));

            this.ItemsOrganiser.Organise(this, new Size(this.ItemsPresenterWidth, this.ItemsPresenterHeight), orderedEnumerable);
        }

        internal IEnumerable<DragablzItem> DragablzItems() {
            return this.Containers<DragablzItem>().ToList();
        }

        internal Size? LockedMeasure { get; set; }

        private void ItemDragStarted(object sender, DragablzDragStartedEventArgs eventArgs) {
            if (this.ItemsOrganiser != null) {
                var bounds = new Size(this.ActualWidth, this.ActualHeight);
                this.ItemsOrganiser.OrganiseOnDragStarted(this, bounds, this.DragablzItems().Except(new[] {eventArgs.DragablzItem}).ToList(),
                    eventArgs.DragablzItem);
            }

            eventArgs.Handled = true;

            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure), DispatcherPriority.Loaded);
        }

        private void ItemDragCompleted(object sender, DragablzDragCompletedEventArgs eventArgs) {
            var dragablzItems = this.DragablzItems().Select(i => {
                i.IsDragging = false;
                i.IsSiblingDragging = false;
                return i;
            }).ToList();

            if (this.ItemsOrganiser != null) {
                var bounds = new Size(this.ActualWidth, this.ActualHeight);
                this.ItemsOrganiser.OrganiseOnDragCompleted(this, bounds,
                    dragablzItems.Except(eventArgs.DragablzItem),
                    eventArgs.DragablzItem);
            }

            eventArgs.Handled = true;

            //wowsers
            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure));
            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure), DispatcherPriority.Loaded);
        }

        private void ItemDragDelta(object sender, DragablzDragDeltaEventArgs eventArgs) {
            var bounds = new Size(this.ItemsPresenterWidth, this.ItemsPresenterHeight);
            var desiredLocation = new Point(
                eventArgs.DragablzItem.X + eventArgs.DragDeltaEventArgs.HorizontalChange,
                eventArgs.DragablzItem.Y + eventArgs.DragDeltaEventArgs.VerticalChange
            );
            if (this.ItemsOrganiser != null) {
                if (this.FixedItemCount > 0 && this.ItemsOrganiser.Sort(this.DragablzItems()).Take(this.FixedItemCount).Contains(eventArgs.DragablzItem)) {
                    eventArgs.Handled = true;
                    return;
                }

                desiredLocation = this.ItemsOrganiser.ConstrainLocation(this, bounds,
                    new Point(eventArgs.DragablzItem.X, eventArgs.DragablzItem.Y),
                    new Size(eventArgs.DragablzItem.ActualWidth, eventArgs.DragablzItem.ActualHeight),
                    desiredLocation, eventArgs.DragablzItem.DesiredSize);
            }

            foreach (var dragableItem in this.DragablzItems().Except(new[] {eventArgs.DragablzItem})) // how about Linq.Where() ?
            {
                dragableItem.IsSiblingDragging = true;
            }

            eventArgs.DragablzItem.IsDragging = true;

            eventArgs.DragablzItem.X = desiredLocation.X;
            eventArgs.DragablzItem.Y = desiredLocation.Y;

            if (this.ItemsOrganiser != null)
                this.ItemsOrganiser.OrganiseOnDrag(
                    this,
                    bounds, this.DragablzItems().Except(new[] {eventArgs.DragablzItem}), eventArgs.DragablzItem);

            eventArgs.DragablzItem.BringIntoView();

            eventArgs.Handled = true;
        }

        private void ItemXChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs) {
            this.UpdateMonitor(routedPropertyChangedEventArgs);
        }

        private void ItemYChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs) {
            this.UpdateMonitor(routedPropertyChangedEventArgs);
        }

        private void UpdateMonitor(RoutedEventArgs routedPropertyChangedEventArgs) {
            if (this.PositionMonitor == null)
                return;

            var dragablzItem = (DragablzItem) routedPropertyChangedEventArgs.OriginalSource;

            if (!Equals(ItemsControlFromItemContainer(dragablzItem), this))
                return;

            this.PositionMonitor.OnLocationChanged(new LocationChangedEventArgs(dragablzItem.Content, new Point(dragablzItem.X, dragablzItem.Y)));

            var linearPositionMonitor = this.PositionMonitor as StackPositionMonitor;
            if (linearPositionMonitor == null)
                return;

            var sortedItems = linearPositionMonitor.Sort(this.Containers<DragablzItem>()).Select(di => di.Content).ToArray();
            if (this._previousSortQueryResult == null || !this._previousSortQueryResult.SequenceEqual(sortedItems))
                linearPositionMonitor.OnOrderChanged(new OrderChangedEventArgs(this._previousSortQueryResult, sortedItems));

            this._previousSortQueryResult = sortedItems;
        }

        private void ItemMouseDownWithinHandlerTarget(object sender, DragablzItemEventArgs e) {
            if (this.ItemsOrganiser == null)
                return;

            var bounds = new Size(this.ActualWidth, this.ActualHeight);
            this.ItemsOrganiser.OrganiseOnMouseDownWithin(this, bounds, this.DragablzItems().Except(e.DragablzItem).ToList(),
                e.DragablzItem);
        }

        private void ItemSizeChangedEventHandler(object sender, SizeChangedEventArgs e) {
            this.InvalidateMeasure();
            //extra kick
            this.Dispatcher.BeginInvoke(new Action(this.InvalidateMeasure), DispatcherPriority.Loaded);
        }
    }
}