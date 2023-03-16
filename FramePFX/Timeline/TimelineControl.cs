using System;
using System.Windows.Controls;
using System.Windows;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using FramePFX.Timeline.Layer;
using System.Windows.Input;

namespace FramePFX.Timeline {
    public class TimelineControl : ItemsControl {

        #region Dependency Properties

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((TimelineControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnitZoom(v)));

        public static readonly DependencyProperty MaxDurationProperty =
            DependencyProperty.Register(
                "MaxDuration",
                typeof(long),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    100L,
                    FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                    (d, e) => ((TimelineControl) d).OnMaxDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? 0 : v));

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

        private ScrollViewer PART_ScrollViewer;
        private ItemsPresenter PART_ItemsPresenter;
        private TimelinePlayheadControl PART_PlayHead;

        public TimelineClipDragData DragData;

        public TimelineControl() {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer.SetCanContentScroll(this, false);
            this.DataContextChanged += (sender, args) => {
                if (args.NewValue is TimelineViewModel vm) {
                    vm.Control = this;
                }
            };
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
            if (this.PART_PlayHead != null) {
                this.PART_PlayHead.Timeline = this;
            }

            if (this.PART_ScrollViewer != null) {
                // this.PART_ScrollViewer.ScrollChanged += this.PART_ScrollViewerOnScrollChanged;
                // this.PART_ScrollViewer.PreviewMouseWheel += this.PART_ScrollViewerOnMouseWheel;
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            base.OnPreviewMouseWheel(e);

            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control) {
                double offset = e.Delta / 120d;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                    offset /= 2d;
                }

                // offset = 1d / offset;
                this.UnitZoom = Math.Max(this.UnitZoom + offset, 1);
                ScrollViewer scroller = this.PART_ScrollViewer;
                if (scroller != null) {
                    this.Dispatcher.Invoke(() => {
                        // scrollable = 10,000
                        // viewport   =  1,000
                        // Mouse X    =    500
                        // multiplier =    0.5
                        // CurrOffset =  6,000
                        // NewOffset  =  6,250

                        double scrollable = scroller.ExtentWidth; // 10,000
                        double viewport = scroller.ViewportWidth; //  1,000
                        double currOffset = scroller.HorizontalOffset; //  6,000
                        double mouseX = e.GetPosition(scroller).X; //    500
                        double scrollMultiplier = (mouseX / viewport); //    0.5
                        scrollMultiplier *= offset;
                        double targetZoomX = scrollMultiplier * viewport; //    250
                        double offsetH = currOffset + targetZoomX;


                        // double percent = mouseX / scroller.ViewportWidth;
                        // percent = Maths.Map(percent, 0d, 1d, -1d, 1d);
                        // double offsetH = scroller.HorizontalOffset + (scroller.ViewportWidth * percent);
                        if (double.IsNaN(offsetH)) {
                            offsetH = 0d;
                        }

                        scroller.ScrollToHorizontalOffset(offsetH);
                    });
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

                layer.Timeline = this;
            }
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (TimelineUtils.IsZoomEqual(oldZoom, newZoom)) {
                return;
            }

            if (Math.Abs(oldZoom - newZoom) > TimelineUtils.MinUnitZoom) {
                foreach (TimelineLayerControl element in this.GetLayers()) {
                    element.UnitZoom = newZoom;
                }

                if (this.PART_ItemsPresenter != null) {
                    this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(this.MaxDuration, this.UnitZoom);
                }
            }
        }

        public IEnumerable<TimelineLayerControl> GetLayers() {
            foreach (object item in this.Items) {
                if (item is TimelineLayerControl layer1) {
                    yield return layer1;
                }
                else if (this.ItemContainerGenerator.ContainerFromItem(item) is TimelineLayerControl layer2) {
                    yield return layer2;
                }
            }
        }

        public IEnumerable<TimelineClipControl> GetSelectedClips() {
            foreach (TimelineLayerControl layer in this.GetLayers()) {
                foreach (object item in layer.SelectedItems) {
                    if (item is TimelineClipControl c1) {
                        yield return c1;
                    }
                    else if (layer.ItemContainerGenerator.ContainerFromItem(item) is TimelineClipControl c2) {
                        yield return c2;
                    }
                }
            }
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
            foreach (TimelineLayerControl layerControl in this.GetLayers()) {
                layerControl.SelectedItems.Clear();
            }

            clip.IsSelected = true;
        }

        public void BeginDragAction() {
            if (this.DragData != null) {
                return;
            }

            List<TimelineClipControl> list = this.GetSelectedClips().ToList();
            if (list.Count < 1) {
                return;
            }

            this.DragData = new TimelineClipDragData(this);
            this.DragData.OnBegin(list);
        }
    }
}
