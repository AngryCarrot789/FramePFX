using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.ViewModels.Timeline;
using FramePFX.Core.Editor.ViewModels.Timeline.Layers;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Utils;

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
                    10000L,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    (d, e) => ((TimelineControl) d).OnMaxDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? TimelineUtils.ZeroLongBox : v));

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

        public Point ClipMousePosForLayerTransition { get; set; }

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

        public void SetZoomAndZoomToCenter(double zoom) {
            double oldzoom = this.UnitZoom;
            this.UnitZoom = zoom;
            zoom = this.UnitZoom;
            if (this.PART_ScrollViewer is ScrollViewer scroller) {
                double center_x = scroller.ViewportWidth / 2d;
                double target_offset = (scroller.HorizontalOffset + center_x) / oldzoom;
                double scaled_target_offset = target_offset * zoom;
                double new_offset = scaled_target_offset - center_x;
                scroller.ScrollToHorizontalOffset(new_offset);
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e) {
            base.OnPreviewMouseWheel(e);
            ModifierKeys mods = Keyboard.Modifiers;

            if ((mods & ModifierKeys.Alt) != 0) {
                if (e.OriginalSource is DependencyObject dependencyObject) {
                    while (dependencyObject != null && !ReferenceEquals(dependencyObject, this) && !(dependencyObject is TimelineLayerControl)) {
                        dependencyObject = VisualTreeUtils.GetParent(dependencyObject);
                    }

                    if (dependencyObject is TimelineLayerControl layer && layer.ViewModel is LayerViewModel vm) {
                        vm.Height = Maths.Clamp(vm.Height + (e.Delta / 120d) * 20d, vm.MinHeight, vm.MaxHeight);
                    }
                }
            }
            else if ((mods & ModifierKeys.Control) != 0) {
                if (this.PART_ScrollViewer is ScrollViewer scroller) {
                    double multiplier;
                    if ((mods & ModifierKeys.Shift) != 0) {
                        multiplier = e.Delta > 0 ? 1.05 : 0.95;
                    }
                    else {
                        multiplier = e.Delta > 0 ? 1.2 : 0.8;
                    }

                    double oldzoom = this.UnitZoom;
                    double newzoom = Math.Max(oldzoom * multiplier, 0.01d);
                    this.UnitZoom = newzoom; // let the coerce function clamp the zoom value
                    newzoom = this.UnitZoom;

                    if (Maths.Equals(oldzoom, newzoom, TimelineUtils.MinUnitZoom)) {
                        return;
                    }

                    // comments assume this.ActualWidth == scroller.ViewportWidth == 1000
                    // and scroller.ExtendWidth at UnitZoom 1.0 = 10000, meaning width ratio = 10

                    { // managed to get zooming towards the cursor working
                        double mouse_x = e.GetPosition(scroller).X;
                        double target_offset = (scroller.HorizontalOffset + mouse_x) / oldzoom;
                        double scaled_target_offset = target_offset * newzoom;
                        double new_offset = scaled_target_offset - mouse_x;
                        scroller.ScrollToHorizontalOffset(new_offset);
                    }

                    e.Handled = true;

                    // { // this code zooms towards frame 1000 if viewport_width is 1000
                    //     // double viewport_width = this.ActualWidth;
                    //     // double pixels_difference = (viewport_width / oldzoom) - (viewport_width / newzoom); // 90.909090
                    //     // actual_pixel_change is the number of pixels that have been removed from the end of the viewport
                    //     // scroller.ScrollToHorizontalOffset(((scroller.HorizontalOffset / oldzoom) + (pixels_difference * newzoom)) * oldzoom);
                    // }

                    // double extent_width = scroller.ExtentWidth;
                    // double viewport_width = this.ActualWidth;
                    // double width_ratio = extent_width / viewport_width;
                    // double pixels_difference = (viewport_width / oldzoom) - (viewport_width / newzoom); // 90.909090
                    // double actual_pixel_change = pixels_difference * newzoom; // 100
                    // // actual_pixel_change is the number of pixels that have been removed from the end of the viewport
                    //
                    // double actual_offset = scroller.HorizontalOffset;
                    // double full_horizontal_offset = actual_offset / newzoom;
                    // double visible_pixels = newzoom * viewport_width;
                    // double width_ratio_1 = (extent_width / newzoom) / viewport_width;
                    // double side_ratio_x = (actual_offset / 2) / full_horizontal_offset;
                    // double new_offset = full_horizontal_offset - actual_pixel_change;
                    //
                    // {
                    //     scroller.ScrollToHorizontalOffset(((actual_offset / oldzoom) + (pixels_difference * newzoom)) * oldzoom);
                    //     double center_position = viewport_width / 2;
                    //     double center_change = center_position * (oldzoom / newzoom);
                    //     // scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + center_change);
                    //     // scroller.ScrollToHorizontalOffset((scroller.HorizontalOffset / (oldzoom / newzoom)) + (pixels_difference * newzoom));
                    // }

                    // Debug.WriteLine("---------------------------------------------------------");
                    // Debug.WriteLine("extent_width:".FitLength(25) + extent_width);
                    // Debug.WriteLine("viewport_width:".FitLength(25) + viewport_width);
                    // Debug.WriteLine("actual_offset:".FitLength(25) + actual_offset);
                    // Debug.WriteLine("full_horizontal_offset:".FitLength(25) + full_horizontal_offset);
                    // Debug.WriteLine("width_ratio:".FitLength(25) + width_ratio);
                    // Debug.WriteLine("pixels_difference:".FitLength(25) + pixels_difference);
                    // Debug.WriteLine("actual_pixel_change:".FitLength(25) + actual_pixel_change);
                    // Debug.WriteLine("visible_pixels:".FitLength(25) + visible_pixels);
                    // Debug.WriteLine("width_ratio_1:".FitLength(25) + width_ratio_1);
                    // Debug.WriteLine("side_ratio_x:".FitLength(25) + side_ratio_x);
                    // Debug.WriteLine("new_offset:".FitLength(25) + new_offset);
                    // Debug.WriteLine("old_zoom:".FitLength(25) + oldzoom);
                    // Debug.WriteLine("new_zoom:".FitLength(25) + newzoom);
                    // Debug.WriteLine("---------------------------------------------------------");

                    // clip width = 31 px
                    // clip begin; distance from old to new = 50 px
                    // expected horizontal offset = 50/2 = 25

                    // then when zoomed in 10x:
                    // clip width = 80px
                    // expected horizontal offset = 797.37059

                    /*
                        extent_width:            23579.47691
                        viewport_width:          1000
                        actual_offset:           0
                        full_horizontal_offset:  0
                        width_ratio:             23.57947691
                        pixels_difference:       38.5543289429532
                        actual_pixel_change:     100
                        visible_pixels:          2593.7424601
                        width_ratio_1:           9.09090909090909
                        side_ratio_x:            NaN
                        new_offset:              -100
                        old_zoom:                2.357947691
                        new_zoom:                2.5937424601
                        zoom_dif =               0.2357947691

                        extent_width / pixels_difference = 611.5910463
                     */



                    /*
                        Zooming in from 9 mouse wheels to 10
                        extent_width:            23579.47691
                        viewport_width:          1000
                        actual_offset:           679.959802543391
                        full_horizontal_offset:  262.153938952434
                        width_ratio:             23.57947691
                        pixels_difference:       38.5543289429532
                        actual_pixel_change:     100
                        visible_pixels:          2593.7424601
                        width_ratio_1:           9.09090909090909
                        side_ratio_x:            1.29687123005
                        new_offset:              162.153938952434 (full_horizontal_offset - actual_pixel_change)
                        old_zoom:                2.357947691
                        new_zoom:                2.5937424601
                        zoom_dif =               0.2357947691


                        Zooming in from 10 mouse wheels to 11
                        PlayHead is at pixel 501, not 500 like what the above `expected horizontal offset`! Take that into account
                        However, the actual_offset of 798.2 should be proof enough that the data mostly lines up
                        extent_width:            25937.424601
                        viewport_width:          1000
                        actual_offset:           798.238161430575
                        full_horizontal_offset:  279.777605914659
                        width_ratio:             25.937424601
                        pixels_difference:       35.0493899481393
                        actual_pixel_change:     100
                        visible_pixels:          2853.11670611
                        width_ratio_1:           9.09090909090909
                        side_ratio_x:            1.426558353055
                        new_offset:              179.777605914659
                        old_zoom:                2.5937424601
                        new_zoom:                2.85311670611
                        zoom_dif =               0.25937424601

                        actual_offset_diff                = 117.410787456609
                        actual_offset_diff / side_ratio_x = 90.5388552
                        visible_pixels_diff               = 259.37424601
                     */

                    // scroller.ScrollToHorizontalOffset(actual_offset + new_offset);


                    // double thisOldWidth = this.ActualWidth;
                    // double scrollOldWidth = scroller.ExtentWidth;
                    //
                    // this.UnitZoom = newzoom;
                    //
                    // scroller.InvalidateArrange();
                    // scroller.UpdateLayout();
                    //
                    // double thisNewWidth = this.ActualWidth;
                    // double scrollNewWidth = scroller.ExtentWidth;
                    //
                    // // width = 1000
                    // double width = scrollNewWidth;
                    // double pixels_difference_w222 = (width / oldzoom) - (width / newzoom);
                    // double pixels_difference_w = scrollNewWidth - scrollOldWidth;
                    // //scroller.ScrollToHorizontalOffset(scroller.HorizontalOffset + (pixels_difference_w / 2d));
                    //
                    // double ratio = (scroller.ExtentWidth / newzoom) / this.ActualWidth;
                    // double zmdif = newzoom - oldzoom;
                    // double incre = zmdif * ratio;
                    // double aaa = ((scroller.ExtentWidth / newzoom) - this.ActualWidth / newzoom); // 9964
                    // double bbb = (aaa / incre) / ratio; // 38.1

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
            else if (this.PART_ScrollViewer != null && (mods & ModifierKeys.Shift) == ModifierKeys.Shift) {
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
                case VideoLayerViewModel _: return new VideoLayerControl();
                case AudioLayerViewModel _: return new AudioLayerControl();
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
