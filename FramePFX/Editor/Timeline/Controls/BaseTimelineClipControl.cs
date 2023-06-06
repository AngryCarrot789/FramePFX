using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using FramePFX.Core.Editor.ResourceManaging.ViewModels;
using FramePFX.Core.Editor.ViewModels;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Layer.Clips;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public abstract class BaseTimelineClipControl : ContentControl, IClipHandle {
        public static readonly DependencyProperty UnitZoomProperty =
            BaseTimelineLayerControl.UnitZoomProperty.AddOwner(
                typeof(BaseTimelineClipControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((BaseTimelineClipControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty IsSelectedProperty =
            Selector.IsSelectedProperty.AddOwner(
                typeof(BaseTimelineClipControl),
                new FrameworkPropertyMetadata(
                    false,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.Journal,
                    (d, e) => ((BaseTimelineClipControl) d).OnIsSelectedChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty IsDroppableTargetOverProperty = DependencyProperty.Register("IsDroppableTargetOver", typeof(bool), typeof(BaseTimelineClipControl), new PropertyMetadata(BoolBox.False));

        public static readonly DependencyProperty HeaderBrushProperty =
            DependencyProperty.Register(
                "HeaderBrush",
                typeof(Brush),
                typeof(BaseTimelineClipControl),
                new PropertyMetadata(null));

        public static readonly RoutedEvent SelectedEvent = Selector.SelectedEvent.AddOwner(typeof(BaseTimelineClipControl));
        public static readonly RoutedEvent UnselectedEvent = Selector.UnselectedEvent.AddOwner(typeof(BaseTimelineClipControl));

        /// <summary>
        /// The zoom level of this timeline layer
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        [Category("Appearance")]
        public bool IsSelected {
            get => (bool) this.GetValue(IsSelectedProperty);
            set => this.SetValue(IsSelectedProperty, value);
        }

        [Category("Appearance")]
        public Brush HeaderBrush {
            get => (Brush) this.GetValue(HeaderBrushProperty);
            set => this.SetValue(HeaderBrushProperty, value);
        }

        public bool IsDroppableTargetOver {
            get => (bool) this.GetValue(IsDroppableTargetOverProperty);
            set => this.SetValue(IsDroppableTargetOverProperty, value);
        }

        public event RoutedEventHandler Selected {
            add => this.AddHandler(SelectedEvent, value);
            remove => this.RemoveHandler(SelectedEvent, value);
        }

        public event RoutedEventHandler Unselected {
            add => this.AddHandler(UnselectedEvent, value);
            remove => this.RemoveHandler(UnselectedEvent, value);
        }

        public Selector ParentSelector {
            get => ItemsControl.ItemsControlFromItemContainer(this) as Selector;
        }

        public BaseTimelineLayerControl Layer {
            get => this.ParentSelector as BaseTimelineLayerControl;
        }

        public TimelineControl Timeline {
            get => this.Layer?.Timeline;
        }

        protected bool isUpdatingUnitZoom;
        protected Point lastLeftClickPoint;
        protected Thumb PART_ThumbLeft;
        protected Thumb PART_ThumbRight;
        protected bool isDraggingThumb;
        protected bool isClipDragActivated;
        protected bool isCancellingDragAction;
        protected bool isProcessingMouseAction;

        private bool isProcessingDrop;

        protected BaseTimelineClipControl() {
            this.AllowDrop = true;
            this.Drop += this.OnDrop;
        }

        protected override void OnDragEnter(DragEventArgs e) {
            base.OnDragEnter(e);

        }

        protected override void OnDragOver(DragEventArgs e) {
            base.OnDragOver(e);
            if (e.Data.GetDataPresent("ResourceItem")) {
                object obj = e.Data.GetData("ResourceItem");
                if (obj is ResourceItemViewModel resource && this.DataContext is IDropClipResource drop && drop.CanDropResource(resource)) {
                    this.IsDroppableTargetOver = true;
                    e.Effects = DragDropEffects.Move;
                    return;
                }
            }

            this.ClearValue(IsDroppableTargetOverProperty);
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        protected override void OnDragLeave(DragEventArgs e) {
            base.OnDragLeave(e);
            this.Dispatcher.Invoke(() => {
                this.ClearValue(IsDroppableTargetOverProperty);
            }, DispatcherPriority.Loaded);
        }

        private void OnDrop(object sender, DragEventArgs e) {
            e.Handled = true;
            if (this.isProcessingDrop) {
                return;
            }

            this.isProcessingDrop = true;
            this.OnItemDrop(e);
        }

        private async void OnItemDrop(DragEventArgs e) {
            if (e.Data.GetDataPresent("ResourceItem")) {
                object obj = e.Data.GetData("ResourceItem");
                if (obj is ResourceItemViewModel resource && this.DataContext is IDropClipResource drop) {
                    await drop.OnDropResource(resource);
                }
            }

            this.isProcessingDrop = false;
            this.ClearValue(IsDroppableTargetOverProperty);
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Stretch;
            this.PART_ThumbLeft = this.GetTemplateElement<Thumb>("PART_ThumbLeft");
            this.PART_ThumbRight = this.GetTemplateElement<Thumb>("PART_ThumbRight");
            this.PART_ThumbLeft.DragDelta += this.OnDragLeftThumb;
            this.PART_ThumbRight.DragDelta += this.OnDragRightThumb;
        }

        protected virtual void OnDragLeftThumb(object sender, DragDeltaEventArgs e) {

        }

        protected virtual void OnDragRightThumb(object sender, DragDeltaEventArgs e) {

        }

        protected virtual void BeginDragMovement(/* Point mousePosition */) {
            if (this.Timeline.DragData != null) {
                return; // ?????
            }

            this.Timeline.BeginDragAction();
            // this.CaptureMouse();
            // this.Focus();
        }

        private void OnIsSelectedChanged(bool oldSelected, bool newSelected) {
            if (newSelected) {
                this.OnSelected(new RoutedEventArgs(SelectedEvent, this));
            }
            else {
                this.OnUnselected(new RoutedEventArgs(SelectedEvent, this));
            }
        }

        public virtual void OnSelected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        public virtual void OnUnselected(RoutedEventArgs e) {
            this.RaiseEvent(e);
        }

        protected virtual void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom)
                return;

            TimelineUtils.ValidateNonNegative(newZoom);
            if (Math.Abs(oldZoom - newZoom) >= TimelineUtils.MinUnitZoom) {
                this.isUpdatingUnitZoom = true;
                this.UpdatePositionAndSize();
                this.isUpdatingUnitZoom = false;
            }
        }

        public void UpdatePositionAndSize() {
            this.UpdatePosition();
            this.UpdateSize();
        }

        public abstract void UpdatePosition();

        public abstract void UpdateSize();

        protected T GetTemplateElement<T>(string name) where T : DependencyObject {
            return this.GetTemplateChild(name) is T value ? value : throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
        }
    }
}