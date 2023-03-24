using System;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FramePFX.Timeline.Layer;
using System.Windows.Input;
using FramePFX.Timeline.Layer.Clips;

namespace FramePFX.Timeline {
    public class TimelineControl : ItemsControl, ITimelineHandle {

        #region Dependency Properties

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty MaxDurationProperty =
            DependencyProperty.Register(
                "MaxDuration",
                typeof(long),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    100L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineControl) d).OnMaxDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

        public static readonly DependencyProperty PlayHeadAreaHeightProperty =
            DependencyProperty.Register(
                "PlayHeadAreaHeight",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    40d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        #endregion

        /// <summary>
        /// The zoom level of all timeline layers
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long MaxDuration {
            get => (long) this.GetValue(MaxDurationProperty);
            set => this.SetValue(MaxDurationProperty, value);
        }

        public double PlayHeadAreaHeight {
            get => (double) this.GetValue(PlayHeadAreaHeightProperty);
            set => this.SetValue(PlayHeadAreaHeightProperty, value);
        }

        public TimelineViewModel ViewModel => this.DataContext as TimelineViewModel;

        private ScrollViewer PART_ScrollViewer;
        private ItemsPresenter PART_ItemsPresenter;
        private TimelinePlayheadControl PART_PlayHead;
        private Border PART_TimestampBoard;

        private TimelineClipDragData dragData;

        public TimelineClipDragData DragData { 
            get => this.dragData; 
            set => this.dragData = value; 
        }

        public TimelineControl() {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer.SetCanContentScroll(this, false);
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is TimelineViewModel vm) {
                    vm.Handle = this;
                }
            };

            this.Loaded += this.OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            if (this.DataContext is TimelineViewModel vm) {
                vm.PlayHeadHandle = this.PART_PlayHead;
            }
        }

        private void OnMaxDurationChanged(long oldValue, long newValue) {
            if (this.PART_ItemsPresenter != null) {
                this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(newValue, this.UnitZoom);
            }
        }

        public bool GetViewModel(out TimelineViewModel timeline) {
            return (timeline = this.DataContext as TimelineViewModel) != null;
        }

        public bool HasActiveDrag() {
            if (this.DragData == null) {
                return false;
            }
            else if (this.DragData.IsCompleted) {
                this.DragData = null; // just in case
                return false;
            }
            else {
                return true;
            }
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ScrollViewer = this.GetTemplateElement<ScrollViewer>("PART_ScrollViewer");
            this.PART_ItemsPresenter = this.GetTemplateElement<ItemsPresenter>("PART_ItemsPresenter");
            this.PART_PlayHead = this.GetTemplateElement<TimelinePlayheadControl>("PART_PlayHead");
            this.PART_TimestampBoard = this.GetTemplateElement<Border>("PART_TimestampBoard");
            // if (this.PART_PlayHead != null) {
            //     this.PART_PlayHead.Timeline = this;
            // }

            if (this.PART_ScrollViewer != null) {
                // this.PART_ScrollViewer.ScrollChanged += this.PART_ScrollViewerOnScrollChanged;
                // this.PART_ScrollViewer.PreviewMouseWheel += this.PART_ScrollViewerOnMouseWheel;
            }

            if (this.PART_TimestampBoard != null) {
                this.PART_TimestampBoard.MouseLeftButtonDown += (s, e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X, e, true);
            }

            if (this.DataContext is TimelineViewModel timeline) {
                timeline.PlayHeadHandle = this.PART_PlayHead;
            }

            this.MouseLeftButtonDown += (s,e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X + this.PART_ScrollViewer?.HorizontalOffset ?? 0d, e, false);
        }

        private void MovePlayheadForMouseButtonEvent(double x, MouseButtonEventArgs e, bool enableThumbDragging = true) {
            if (x >= 0d) {
                long frameX = TimelineUtils.PixelToFrame(x, this.UnitZoom);
                if (frameX >= 0 && this.GetViewModel(out TimelineViewModel vm) && frameX < vm.MaxDuration) {
                    vm.PlayHeadFrame = frameX;
                }

                e.Handled = true;
                if (enableThumbDragging) {
                    this.PART_PlayHead.EnableDragging(new Point(x, 0));
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            base.OnPreviewMouseWheel(e);
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control || (Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt) {
                double offset = e.Delta / 120d;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                    offset /= 2d;
                }

                double oldZoom = this.UnitZoom;
                double zoom = Math.Max(oldZoom + offset, 1);
                if (TimelineUtils.IsUnitEqual(zoom, oldZoom)) {
                    return;
                }

                // offset = 1d / offset;
                ScrollViewer scroller = this.PART_ScrollViewer;
                if (scroller != null) {
                    double viewport = scroller.ViewportWidth;  //  1,000
                    double mouseX = e.GetPosition(scroller).X; //    500
                    double offsetFrameX = mouseX / oldZoom;
                    this.UnitZoom = zoom;
                    double frameWidth = viewport / zoom;
                    this.UpdateLayout();
                    scroller.UpdateLayout();

                    // scrollable = 10,000
                    // viewport   =  1,000
                    // Mouse X    =    500
                    // multiplier =    0.5
                    // CurrOffset =  6,000
                    // NewOffset  =  6,250

                    // double scrollable = scroller.ExtentWidth;      // 10,000
                    // double currOffset = scroller.HorizontalOffset; //  6,000
                    double oldFrameScrolled = scroller.HorizontalOffset / oldZoom;
                    double newFrameScrolled = oldFrameScrolled * zoom;
                    newFrameScrolled = offset < 0 ? (newFrameScrolled - (frameWidth / 2d)) : (newFrameScrolled + offsetFrameX);

                    if (double.IsNaN(newFrameScrolled)) {
                        newFrameScrolled = 0d;
                    }

                    scroller.ScrollToHorizontalOffset(newFrameScrolled);
                }
                else {
                    this.UnitZoom = zoom;
                }
            }
            else if (this.PART_ScrollViewer != null && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                if (e.Delta < 0) {
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                }
                else if (e.Delta > 0) {
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                }
            }
            // else {
            //     int value = e.Delta / 120;
            //     if (value != 0) {
            //         this.PART_PlayHead.PlayHeadFrame = Maths.Clamp(this.PART_PlayHead.PlayHeadFrame + value, 0L, this.MaxDuration - 1);
            //     }
            // }
        }

        // private void PART_ScrollViewerOnMouseWheel(object sender, MouseWheelEventArgs e) {
        //     if (Keyboard.Modifiers == ModifierKeys.Shift) {
        //         if (e.Delta < 0) {
        //         }
        //         // if (e.Delta > 0 && this.PART_ScrollViewer.HorizontalOffset >= this.PART_ScrollViewer.ViewportWidth) {
        //         //     // We are scrolling to the right, add more space to the right of the ScrollViewer
        //         //     this.PART_ScrollViewer.Width += 50;
        //         // }
        //         // else if (e.Delta < 0 && this.PART_ScrollViewer.Width > this.PART_ScrollViewer.ActualWidth) {
        //         //     // We are scrolling back to the left, remove space from the right of the ScrollViewer
        //         //     this.PART_ScrollViewer.Width -= 50;
        //         // }
        //     }
        // }

        // private bool isScrollViewerWidthUpdated = false;

        // private void PART_ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
        //     if (this.isScrollViewerWidthUpdated || this.PART_ScrollViewer.HorizontalOffset < this.PART_ScrollViewer.ScrollableWidth) {
        //         if (this.isScrollViewerWidthUpdated && this.PART_ScrollViewer.HorizontalOffset < this.PART_ScrollViewer.ScrollableWidth) {
        //             // We have scrolled back to the left, reset the ScrollViewer width flag
        //             this.isScrollViewerWidthUpdated = false;
        //         }
        //     }
        //     else {
        //         // We have scrolled to the end, add more space to the right of the ScrollViewer
        //         this.PART_ScrollViewer.Width += 50;
        //         this.isScrollViewerWidthUpdated = true;
        //     }
        // }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineLayerControl() { Timeline = this };
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineLayerControl;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            if (element is TimelineLayerControl layer) {
                if (item is LayerViewModel viewModel) {
                    viewModel.Control = layer;
                }
                // else {
                //     throw new Exception($"Expected item of type {nameof(LayerViewModel)}, got {item?.GetType()}");
                // }

                layer.Timeline = this;
            }
            // else {
            //     throw new Exception($"Expected element of type {nameof(TimelineLayerControl)}, got {element?.GetType()}");
            // }
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
            if (element is TimelineLayerControl layer && ReferenceEquals(layer.Timeline, this)) {
                if (item is LayerViewModel viewModel && ReferenceEquals(viewModel.Control, element)) {
                    viewModel.Control = null;
                }

                layer.Timeline = null;
            }
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (TimelineUtils.IsUnitEqual(oldZoom, newZoom)) {
                return;
            }

            if (Math.Abs(oldZoom - newZoom) > TimelineUtils.MinUnitZoom) {
                foreach (TimelineLayerControl element in this.GetLayerControls()) {
                    element.UnitZoom = newZoom;
                }

                if (this.PART_ItemsPresenter != null) {
                    this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(this.MaxDuration, this.UnitZoom);
                }

                if (this.PART_PlayHead != null) {
                    this.PART_PlayHead.UpdatePosition();
                }
            }
        }

        public bool GetLayerControl(object item, out TimelineLayerControl layer) {
            return (layer = ICGenUtils.GetContainerForItem<LayerViewModel, TimelineLayerControl>(item, this.ItemContainerGenerator, x => x.Control as TimelineLayerControl)) != null;
        }

        public bool GetLayerViewModel(object item, out LayerViewModel layer) {
            return ICGenUtils.GetItemForContainer<TimelineLayerControl, LayerViewModel>(item, this.ItemContainerGenerator, x => x.ViewModel, out layer);
        }

        public IEnumerable<TimelineLayerControl> GetLayerControls() {
            foreach (object item in this.Items) {
                if (this.GetLayerControl(item, out var layer)) {
                    yield return layer;
                }
            }
        }

        public IEnumerable<LayerViewModel> GetLayerViewModels() {
            foreach (object item in this.Items) {
                if (this.GetLayerViewModel(item, out var layer)) {
                    yield return layer;
                }
            }
        }

        public IEnumerable<TimelineClipControl> GetAllSelectedClipControls() {
            foreach (TimelineLayerControl layer in this.GetLayerControls()) {
                foreach (TimelineClipControl clip in layer.GetSelectedClipControls()) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<ClipContainerViewModel> GetAllSelectedClipModels() {
            foreach (TimelineLayerControl layer in this.GetLayerControls()) {
                foreach (ClipContainerViewModel clip in layer.GetSelectedClipViewModels()) {
                    yield return clip;
                }
            }
        }

        public IEnumerable<TimelineClipControl> GetClipsInArea(FrameSpan span) {
            List<TimelineClipControl> list = new List<TimelineClipControl>();
            foreach (TimelineLayerControl layer in this.GetLayerControls()) {
                foreach (TimelineClipControl clip in layer.GetClipsInArea(span)) {
                    list.Add(clip);
                }
            }

            return list;
        }

        private T GetTemplateElement<T>(string name) where T : DependencyObject {
            if (this.GetTemplateChild(name) is T value) {
                return value;
            }
            else if (!DesignerProperties.GetIsInDesignMode(this)) {
                throw new Exception($"Missing templated child '{name}' of type {typeof(T).Name} in control '{this.GetType().Name}'");
            }
            else {
                return null;
            }
        }

        public void SetPrimarySelection(TimelineLayerControl layer, TimelineClipControl clip) {
            foreach (TimelineLayerControl layerControl in this.GetLayerControls()) {
                layerControl.UnselectAll();
            }

            clip.TimelineLayer.MakeSingleSelection(clip);
        }

        public void BeginDragAction() {
            if (this.DragData != null) {
                return;
            }

            List<TimelineClipControl> list = this.GetAllSelectedClipControls().ToList();
            if (list.Count < 1) {
                return;
            }

            this.DragData = new TimelineClipDragData(this);
            this.DragData.OnBegin(list);
        }
    }
}
