﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FramePFX.Core.Editor;
using FramePFX.Core.Editor.Timelines;
using FramePFX.Core.Editor.ViewModels.Timelines.Tracks;
using FramePFX.Core.Utils;
using FramePFX.Editor.Timeline.Utils;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Editor.Timeline.Controls {
    public class TimelineControl : ItemsControl, IHasZoom {
        //       Width
        // --------------------
        //  Zoom   x   Duration

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => ((TimelineControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnit(v)));

        public static readonly DependencyProperty MaxDurationProperty =
            DependencyProperty.Register(
                "MaxDuration",
                typeof(long),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    long.MaxValue,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsMeasure,
                    (d, e) => ((TimelineControl) d).OnMaxDurationChanged((long) e.OldValue, (long) e.NewValue),
                    (d, v) => (long) v < 0 ? TimelineUtils.ZeroLongBox : v));

        public static readonly DependencyProperty SelectionRectangleProperty = DependencyProperty.Register("SelectionRectangle", typeof(SelectionRect?), typeof(TimelineControl), new PropertyMetadata((SelectionRect?) null));

        public SelectionRect? SelectionRectangle {
            get => (SelectionRect?) this.GetValue(SelectionRectangleProperty);
            set => this.SetValue(SelectionRectangleProperty, value);
        }

        /// <summary>
        /// The horizontal zoom multiplier of this timeline, which affects the size of all tracks
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

        public Point ClipMousePosForTrackTransition { get; set; }

        private ScrollViewer PART_ScrollViewer;
        private ItemsPresenter PART_ItemsPresenter;
        private TimelinePlayHeadControl PART_PlayHead;
        private Border PART_TimestampBoard;
        private Border PART_SelectionRange;

        public Point selectionBeginPoint;
        public bool isLMBDownForSelection;
        public bool isSelectionActive;

        public TimelineControl() {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer.SetCanContentScroll(this, false);
            this.Loaded += (sender, args) => {

            };
        }

        public IEnumerable<TimelineTrackControl> GetTrackContainers() {
            return this.GetTrackContainers<TimelineTrackControl>(this.Items);
        }

        public IEnumerable<T> GetTrackContainers<T>() where T : TimelineTrackControl {
            return this.GetTrackContainers<T>(this.Items);
        }

        public IEnumerable<TTRack> GetTrackContainers<TTRack>(IEnumerable items) where TTRack : TimelineTrackControl {
            int i = 0;
            foreach (object item in items) {
                if (item is TTRack a) {
                    yield return a;
                }
                else if (this.ItemContainerGenerator.ContainerFromIndex(i) is TTRack b) {
                    yield return b;
                }

                i++;
            }
        }

        public IEnumerable<TimelineClipControl> GetSelectedClipContainers() {
            return this.GetTrackContainers().SelectMany(x => x.GetSelectedClipContainers());
        }

        public IEnumerable<TClip> GetSelectedClipContainers<TClip>() where TClip : TimelineClipControl {
            return this.GetTrackContainers().SelectMany(x => x.GetSelectedClipContainers<TClip>());
        }

        public IEnumerable<TClip> GetClipsInSpan<TClip>(FrameSpan span) where TClip : TimelineClipControl {
            List<TClip> list = new List<TClip>();
            foreach (TimelineTrackControl track in this.GetTrackContainers()) {
                foreach (TimelineClipControl clip in track.GetClipsThatIntersect(span)) {
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

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_ScrollViewer = this.GetTemplateElement<ScrollViewer>("PART_ScrollViewer");
            this.PART_ItemsPresenter = this.GetTemplateElement<ItemsPresenter>("PART_ItemsPresenter");
            this.PART_PlayHead = this.GetTemplateElement<TimelinePlayHeadControl>("PART_PlayHead");
            this.PART_TimestampBoard = this.GetTemplateElement<Border>("PART_TimestampBoard");
            if (this.PART_TimestampBoard != null) {
                this.PART_TimestampBoard.MouseLeftButtonDown += (s, e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X, e, true);
            }

            this.MouseLeftButtonDown += (s, e) => this.MovePlayheadForMouseButtonEvent(e.GetPosition((IInputElement) s).X + this.PART_ScrollViewer?.HorizontalOffset ?? 0d, e, false);
            this.PART_SelectionRange = this.GetTemplateElement<Border>("PART_SelectionRange");
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            this.OnLeftButtonDown(e.GetPosition(this));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            this.OnLeftButtonUp(e.GetPosition(this));
        }

        private void OnLeftButtonDown(Point point) {
            this.selectionBeginPoint = point;
            this.isLMBDownForSelection = true;
            this.isSelectionActive = false;
            this.ClearSelection();
        }

        private void OnLeftButtonUp(Point point) {
            if (!this.isLMBDownForSelection) {
                return;
            }

            Vector diff = (this.selectionBeginPoint - point);
            if (Math.Abs(diff.X) < 4d || Math.Abs(diff.Y) < 4d) {
                this.ClearSelection();
            }

            this.isLMBDownForSelection = false;
            this.isSelectionActive = false;
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);

            if (!this.isLMBDownForSelection)
                return;
            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.isLMBDownForSelection)
                    this.OnLeftButtonUp(e.GetPosition(this));
                return;
            }

            Point origin = this.selectionBeginPoint;
            Point point = e.GetPosition(this);
            point.X = Maths.Clamp(point.X, 0, this.ActualWidth);
            point.Y = Maths.Clamp(point.Y, 0, this.ActualHeight);

            Vector diff = (this.selectionBeginPoint - point);
            if (!this.isSelectionActive) {
                if (Math.Abs(diff.X) < 4d || Math.Abs(diff.Y) < 4d) {
                    return;
                }

                this.isSelectionActive = true;
            }

            double zoom = this.UnitZoom;
            long a = TimelineUtils.PixelToFrame(origin.X, zoom);
            long b = TimelineUtils.PixelToFrame(point.X, zoom);
            long begin = Math.Min(a, b);
            long end = Math.Max(a, b);
            long duration = end - begin;
            if (duration < 1) {
                return;
            }

            double accumulatedHeight = 0d;
            int originIndex = -1;
            int pointIndex = -1;
            List<TimelineTrackControl> list = this.GetTrackContainers().ToList();
            Rect templateRect = new Rect(0, 0, this.ActualWidth, 0);
            for (int i = 0; i < list.Count && (originIndex == -1 || pointIndex == -1); i++) {
                templateRect.Y += accumulatedHeight;
                Rect rect = templateRect;
                rect.Height = list[i].Height;
                accumulatedHeight += rect.Height;
                if (originIndex == -1 && rect.Contains(origin)) {
                    originIndex = i;
                }

                if (pointIndex == -1 && rect.Contains(point)) {
                    pointIndex = i;
                }
            }

            if (originIndex == -1 || pointIndex == -1) {
                return;
            }

            int index = Math.Min(originIndex, pointIndex);
            int length = Math.Abs(pointIndex - originIndex);

            this.SelectionRectangle = new SelectionRect(new FrameSpan(begin, duration), index, length);
            if (this.PART_SelectionRange != null) {
                this.PART_SelectionRange.Visibility = Visibility.Visible;
                double y = 0d;
                for (int i = 0; i < index; i++)
                    y += list[i].Height;

                this.PART_SelectionRange.Margin = new Thickness(TimelineUtils.FrameToPixel(begin, zoom), y, 0, 0);
                this.PART_SelectionRange.Width = TimelineUtils.FrameToPixel(duration, zoom);

                double height = 0d;
                for (int i = index, j = index + length; i < j; i++)
                    height += list[i].Height;
                this.PART_SelectionRange.Height = height;
            }
        }

        public void ClearSelection() {
            if (this.PART_SelectionRange != null) {
                this.PART_SelectionRange.Visibility = Visibility.Collapsed;
            }

            this.ClearValue(SelectionRectangleProperty);
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
            if (e.Delta == 0) {
                return;
            }

            ModifierKeys mods = Keyboard.Modifiers;
            if ((mods & ModifierKeys.Alt) != 0) {
                if (e.OriginalSource is DependencyObject src) {
                    while (src != null && !ReferenceEquals(src, this) && !(src is TimelineTrackControl)) {
                        src = VisualTreeUtils.GetParent(src);
                    }

                    if (src is TimelineTrackControl track) {
                        track.Height = Maths.Clamp(track.Height + (e.Delta / 120d) * 20d, track.MinHeight, track.MaxHeight);
                    }
                }

                e.Handled = true;
            }
            else if ((mods & ModifierKeys.Control) != 0) {
                if (!(this.PART_ScrollViewer is ScrollViewer scroller)) {
                    return;
                }

                e.Handled = true;
                bool shift = (mods & ModifierKeys.Shift) != 0;
                double multiplier = (shift ? 0.1 : 0.25);
                if (e.Delta > 0) {
                    multiplier = 1d + multiplier;
                }
                else {
                    multiplier = 1d - multiplier;
                }

                double oldzoom = this.UnitZoom;
                double newzoom = Math.Max(oldzoom * multiplier, TimelineUtils.MinUnitZoom);
                double minzoom = scroller.ViewportWidth / (scroller.ExtentWidth / oldzoom); // add 0.000000000000001 to never disable scroll bar
                newzoom = Math.Max(minzoom, newzoom);
                this.UnitZoom = newzoom; // let the coerce function clamp the zoom value
                newzoom = this.UnitZoom;

                // managed to get zooming towards the cursor working
                double mouse_x = e.GetPosition(scroller).X;
                double target_offset = (scroller.HorizontalOffset + mouse_x) / oldzoom;
                double scaled_target_offset = target_offset * newzoom;
                double new_offset = scaled_target_offset - mouse_x;
                scroller.ScrollToHorizontalOffset(new_offset);
            }
            else if (this.PART_ScrollViewer != null && (mods & ModifierKeys.Shift) != 0) {
                if (e.Delta < 0) {
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                    this.PART_ScrollViewer.LineRight();
                }
                else {
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                    this.PART_ScrollViewer.LineLeft();
                }

                e.Handled = true;
            }
        }

        private object lastItem;

        protected override DependencyObject GetContainerForItemOverride() {
            object item = this.lastItem;
            this.lastItem = null;
            switch (item) {
                case VideoTrackViewModel _: return new VideoTrackControl();
                case AudioTrackViewModel _: return new AudioTrackControl();
            }

            throw new Exception("Could not create track container for item: " + item);
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            if (item is TimelineTrackControl) {
                return true;
            }

            this.lastItem = item;
            return false;
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.PART_ItemsPresenter != null) {
                this.PART_ItemsPresenter.Width = TimelineUtils.FrameToPixel(this.MaxDuration, this.UnitZoom);
            }

            foreach (TimelineTrackControl track in this.GetTrackContainers()) {
                track.OnUnitZoomChanged();
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
            foreach (TimelineTrackControl trackControl in this.GetTrackContainers()) {
                trackControl.UnselectAll();
            }

            clip.Track.MakeSingleSelection(clip);
        }
    }
}