using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelineControl : ItemsControl, ITimelineHandle {
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

        /// <summary>
        /// The horizontal zoom multiplier of this timeline, which affects the size of all layers
        /// and therefore clips. This is a value used for converting frames into pixels
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long MaxDuration {
            get => (long) this.GetValue(MaxDurationProperty);
            set => this.SetValue(MaxDurationProperty, value);
        }

        /// <summary>
        /// Returns the number of frames that are visible on screen (calculated by dividing the render width by the unit zoom)
        /// </summary>
        public double VisibleFrames => this.ActualWidth / this.UnitZoom;

        public TimelineClipDragData DragData { get; set; }

        private ScrollViewer PART_ScrollViewer;
        private ItemsPresenter PART_ItemsPresenter;
        private TimelinePlayHeadControl PART_PlayHead;
        private Border PART_TimestampBoard;

        public TimelineControl() {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer.SetCanContentScroll(this, false);
        }

        public IEnumerable<TimelineLayerControl> GetLayerContainers() {
            return this.GetLayerContainers<TimelineLayerControl>(this.Items);
        }

        public IEnumerable<TLayer> GetLayerContainers<TLayer>() where TLayer : TimelineLayerControl {
            return this.GetLayerContainers<TLayer>(this.Items);
        }

        public IEnumerable<TLayer> GetLayerContainers<TLayer>(IEnumerable items) where TLayer : TimelineLayerControl {
            int i = 0;
            foreach (object item in items) {
                if (item is TLayer a) {
                    yield return a;
                }
                else if (this.ItemContainerGenerator.ContainerFromIndex(i) is TLayer b) {
                    yield return b;
                }

                i++;
            }
        }

        public IEnumerable<TimelineClipControl> GetSelectedClipContainers() {
            return this.GetLayerContainers().SelectMany(x => x.GetSelectedClipContainers());
        }

        public IEnumerable<TClip> GetSelectedClipContainers<TClip>() where TClip : TimelineClipControl {
            return this.GetLayerContainers().SelectMany(x => x.GetSelectedClipContainers<TClip>());
        }

        public IEnumerable<TClip> GetClipsInSpan<TClip>(FrameSpan span) where TClip : TimelineClipControl {
            List<TClip> list = new List<TClip>();
            foreach (TimelineLayerControl layer in this.GetLayerContainers()) {
                foreach (TimelineClipControl clip in layer.GetClipsThatIntersect(span)) {
                    if (clip is TClip t) {
                        list.Add(t);
                    }
                }
            }

            return list;
        }

        public IEnumerable<TimelineClipControl> GetClipsInSpan(FrameSpan span) {
            return this.GetClipsInSpan<TimelineClipControl>(span);
        }

        private void OnMaxDurationChanged(long oldValue, long newValue) {
            if (this.PART_ItemsPresenter != null) {
                this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(newValue, this.UnitZoom);
            }
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
            this.PART_PlayHead = this.GetTemplateElement<TimelinePlayHeadControl>("PART_PlayHead");
            this.PART_TimestampBoard = this.GetTemplateElement<Border>("PART_TimestampBoard");
            if (this.PART_TimestampBoard != null) {
                this.PART_TimestampBoard.MouseLeftButtonDown += (s, e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X, e, true);
            }

            this.MouseLeftButtonDown += (s,e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X + this.PART_ScrollViewer?.HorizontalOffset ?? 0d, e, false);
        }

        private void MovePlayheadForMouseButtonEvent(double x, MouseButtonEventArgs e, bool enableThumbDragging = true) {
            if (x >= 0d) {
                long frameX = TimelineUtils.PixelToFrame(x, this.UnitZoom);
                if (frameX >= 0 && frameX < this.MaxDuration) {
                    this.PART_PlayHead.FrameIndex = frameX;
                }

                e.Handled = true;
                if (enableThumbDragging) {
                    this.PART_PlayHead.EnableDragging(new Point(x, 0));
                }
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            base.OnPreviewMouseWheel(e);
            if ((Keyboard.Modifiers & (ModifierKeys.Control | ModifierKeys.Alt)) != 0) {
                double multiplier;
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                    multiplier = e.Delta > 0 ? 1.05 : 0.95;
                }
                else {
                    multiplier = e.Delta > 0 ? 1.1 : 0.9;
                }

                double oldzoom = this.UnitZoom;
                double newzoom = TimelineUtils.ClampUnit(oldzoom * multiplier);
                this.UnitZoom = newzoom;
                if (this.PART_ScrollViewer is ScrollViewer scroller) {
                    // CBA to get this working. It works for the FreeMoveViewPortV2 but not this for some reason...
                    // even after debugging the view port code, pixels_difference_w is not the same as
                    // the difference in screen pixels but yet the math still works? wtff
                    // newzoom = this.UnitZoom;
                    // Size size = new Size(scroller.ActualWidth, scroller.ActualHeight);
                    // Point pos = e.GetPosition(scroller);
                    // double pixels_difference_w = (size.Width / oldzoom) - (size.Width / newzoom);
                    // double side_ratio_x = (pos.X - (size.Width / 2)) / size.Width;
                    // double offset = scroller.HorizontalOffset;
                    // scroller.ScrollToHorizontalOffset(offset - (pixels_difference_w * side_ratio_x));

                    // double viewport = scroller.ViewportWidth;  //  1,000
                    // double mouseX = e.GetPosition(scroller).X; //    500
                    // double offsetFrameX = mouseX / oldzoom;
                    // this.UnitZoom = newzoom;
                    // double frameWidth = viewport / newzoom;
                    // this.UpdateLayout();
                    // scroller.UpdateLayout();
                    // double oldFrameScrolled = scroller.HorizontalOffset / oldzoom;
                    // double newFrameScrolled = oldFrameScrolled * newzoom;
                    // newFrameScrolled = e.Delta < 0 ? (newFrameScrolled - (frameWidth / 2d)) : (newFrameScrolled + offsetFrameX);
                    // if (double.IsNaN(newFrameScrolled)) {
                    //     newFrameScrolled = 0d;
                    // }
                    // scroller.ScrollToHorizontalOffset(newFrameScrolled);
                }
            }
            else if (this.PART_ScrollViewer != null && (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift) {
                if (e.Delta < 0) {
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                    e.Handled = true;
                }
                else if (e.Delta > 0) {
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                    e.Handled = true;
                }
            }
        }

        private object lastItem;

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.lastItem;
            this.lastItem = null;
            switch (item) {
                case VideoLayerViewModel _: return new VideoTimelineLayerControl();
            }

            throw new Exception("Could not create layer container for item: " + item);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is TimelineLayerControl) {
                return true;
            }

            this.lastItem = item;
            return false;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
        }

        protected override void ClearContainerForItemOverride(DependencyObject element, object item) {
            base.ClearContainerForItemOverride(element, item);
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (Math.Abs(oldZoom - newZoom) <= TimelineUtils.MinUnitZoom) {
                return;
            }

            if (this.PART_ItemsPresenter != null) {
                this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(this.MaxDuration, this.UnitZoom);
            }

            foreach (TimelineLayerControl element in this.GetLayerContainers()) {
                element.OnUnitZoomChanged();
            }

            this.PART_PlayHead?.UpdatePosition();
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

        public void SetPrimarySelection(TimelineClipControl clip) {
            foreach (TimelineLayerControl layerControl in this.GetLayerContainers()) {
                layerControl.UnselectAll();
            }

            clip.Layer.MakeSingleSelection(clip);
        }
    }
}
