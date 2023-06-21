using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Utils;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Editor.Automation {
    [TemplatePart(Name = nameof(PART_Canvas), Type = typeof(Canvas))]
    public class AutomationSequenceEditor : Control {
        public static readonly DependencyProperty KeyFrameBrushProperty =
            DependencyProperty.Register(
                "KeyFrameBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(Brushes.OrangeRed, FrameworkPropertyMetadataOptions.AffectsRender, (o, e) => ((AutomationSequenceEditor) o).OnKeyFrameBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty CurveBrushProperty =
            DependencyProperty.Register(
                "CurveBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(Brushes.OrangeRed, FrameworkPropertyMetadataOptions.AffectsRender, (o, e) => ((AutomationSequenceEditor) o).OnCurveBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty MouseOverBrushProperty =
            DependencyProperty.Register(
                "MouseOverBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(Brushes.WhiteSmoke, FrameworkPropertyMetadataOptions.AffectsRender, (o, e) => ((AutomationSequenceEditor) o).OnMouseOverBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public Brush MouseOverBrush {
            get => (Brush) this.GetValue(MouseOverBrushProperty);
            set => this.SetValue(MouseOverBrushProperty, value);
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(
                "ItemsSource",
                typeof(IEnumerable),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (o, e) => ((AutomationSequenceEditor) o).OnItemsSourcePropertyChanged((IEnumerable) e.OldValue, (IEnumerable) e.NewValue)));

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(1d, (o, e) => ((AutomationSequenceEditor) o).RegenerateAllPoints()));

        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public Brush KeyFrameBrush {
            get => (Brush) this.GetValue(KeyFrameBrushProperty);
            set => this.SetValue(KeyFrameBrushProperty, value);
        }

        public Brush CurveBrush {
            get => (Brush) this.GetValue(CurveBrushProperty);
            set => this.SetValue(CurveBrushProperty, value);
        }

        public IEnumerable ItemsSource {
            get => (IEnumerable) this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        private Pen keyFramePen;
        private Pen curvePen;
        private Pen curvePenTransparent;
        private Pen mouseOverPen;

        private Pen KeyFramePen => this.keyFramePen ?? (this.keyFramePen = new Pen(this.KeyFrameBrush ?? Brushes.OrangeRed, 1d));
        private Pen CurvePenTransparent => this.curvePenTransparent ?? (this.curvePenTransparent = new Pen(Brushes.Transparent, 3d));
        private Pen CurvePen => this.curvePen ?? (this.curvePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, 1d));
        private Pen MouseOverPen => this.mouseOverPen ?? (this.mouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, 1d));

        private Canvas PART_Canvas;

        // internal key frame storage; recalculated whenever the ItemsSource changes
        private List<KeyFramePoint> keyFrameList;
        private KeyFramePoint mouseDown;
        private Point mouseDownPoint;
        private bool justCaptured;

        private readonly PropertyChangedEventHandler propertyChangedEventHandler;
        private ScrollViewer scroller;

        public AutomationSequenceEditor() {
            this.propertyChangedEventHandler = this.OnKeyFrameViewModelPropertyChanged;
            this.Loaded += this.OnLoaded;
            this.IsHitTestVisible = true;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.keyFrameList == null || this.keyFrameList.Count < 1) {
                return;
            }

            this.mouseDownPoint = e.GetPosition(this);
            foreach (KeyFramePoint keyFrame in this.keyFrameList) {
                if (keyFrame.RenderPoint is Point point) {
                    Rect rect = new Rect(point.X - 4d, point.Y - 4d, 8d, 8d);
                    if (RectContains(rect, this.mouseDownPoint)) {
                        keyFrame.IsCaptured = true;
                        keyFrame.InvalidateVisual();
                        this.InvalidateVisual();
                        this.mouseDown = keyFrame;
                        e.Handled = true;
                        this.justCaptured = true;
                        this.CaptureMouse();
                        return;
                    }
                }
            }

            this.mouseDown = null;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            if (this.mouseDown != null) {
                this.mouseDown.IsCaptured = false;
                this.mouseDown = null;
            }

            if (this.IsMouseCaptured) {
                this.ReleaseMouseCapture();
            }

            this.InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.mouseDown != null) {
                    this.mouseDown.IsCaptured = false;
                    this.mouseDown = null;
                }

                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.InvalidateVisual();
            }

            if (this.mouseDown == null) {
                return;
            }

            long min = this.mouseDown.prev?.KeyFrame.Timestamp ?? 0;
            long max = this.mouseDown.next?.KeyFrame.Timestamp ?? 10000;

            Point mPos = e.GetPosition(this);
            if (this.justCaptured) {
                this.mouseDownPoint = mPos;
                this.justCaptured = false;
                return;
            }

            double diffX = mPos.X;

            {
                long negativeFrames = 0L;
                long offset = (long) (diffX / this.UnitZoom);
                if (offset != 0) {
                    long time = this.mouseDown.KeyFrame.Timestamp;
                    if ((time + offset) < 0) {
                        negativeFrames = offset;
                        offset = -time;
                    }

                    if (offset != 0) {
                        this.mouseDown.KeyFrame.Timestamp = Maths.Clamp(offset, min, max);
                        SetValueForScaledMousePoint(this.mouseDown, this.mouseDown.RenderArea, mPos);
                    }
                }
            }

            this.mouseDownPoint = mPos;
        }

        private void ClearKeyFrameList() {
            if (this.keyFrameList == null) {
                return;
            }

            foreach (KeyFramePoint keyFrame in this.keyFrameList) {
                keyFrame.KeyFrame.PropertyChanged -= this.propertyChangedEventHandler;
            }

            this.keyFrameList.Clear();
            this.keyFrameList = null;
        }

        private void CompletelyInvalidateRender() {
            if (this.keyFrameList != null) {
                foreach (KeyFramePoint keyFrame in this.keyFrameList) {
                    keyFrame.InvalidateVisual();
                }
            }

            this.InvalidateVisual();
        }

        private List<KeyFramePoint> GetOrGenerateKeyFrameList() {
            if (this.keyFrameList != null) {
                return this.keyFrameList;
            }

            if (this.ItemsSource is IEnumerable enumerable) {
                this.keyFrameList = new List<KeyFramePoint>();
                KeyFramePoint prev = null;
                foreach (object item in enumerable) {
                    if (item is KeyFrameViewModel keyFrame) {
                        keyFrame.PropertyChanged += this.propertyChangedEventHandler;
                        KeyFramePoint keyF = new KeyFramePoint(keyFrame) {
                            prev = prev
                        };

                        if (prev != null) {
                            prev.next = keyF;
                        }

                        this.keyFrameList.Add(keyF);
                        prev = keyF;
                    }
                }
            }

            return this.keyFrameList;
        }

        private void OnKeyFrameViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            // if (this.keyFrameList != null) {
            //     foreach (KeyFramePoint keyPoint in this.keyFrameList) {
            //         keyPoint.InvalidateVisual();
            //     }
            // }

            if (this.keyFrameList != null && sender is KeyFrameViewModel keyFrame && this.keyFrameList.Find(x => x.KeyFrame == keyFrame) is KeyFramePoint found) {
                found.InvalidateVisual();
            }
            else if (this.keyFrameList != null) {
                this.ClearKeyFrameList();
            }

            this.InvalidateVisual();
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.scroller = VisualTreeUtils.FindVisualParent<ScrollViewer>(this);
            if (this.scroller == null) {
                return;
            }

            this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
            this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
        }

        private void OnScrollerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
            this.CompletelyInvalidateRender();
        }

        private void OnScrollerOnSizeChanged(object sender, SizeChangedEventArgs e) {
            this.CompletelyInvalidateRender();
        }

        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.PART_Canvas = (Canvas) this.GetTemplateChild(nameof(this.PART_Canvas)) ?? throw new Exception("Missing canvas part");
        }

        protected virtual void OnKeyFrameBrushPropertyChanged(Brush oldValue, Brush newValue) {
            this.keyFramePen = null;
        }

        protected virtual void OnCurveBrushPropertyChanged(Brush oldValue, Brush newValue) {
            this.curvePen = null;
        }

        protected virtual void OnMouseOverBrushPropertyChanged(Brush oldValue, Brush newValue) {
            this.mouseOverPen = null;
        }

        protected virtual void OnItemsSourcePropertyChanged(IEnumerable oldValue, IEnumerable newValue) {
            if (oldValue is INotifyCollectionChanged) {
                ((INotifyCollectionChanged) oldValue).CollectionChanged -= this.OnCollectionChanged;
            }

            if (newValue is INotifyCollectionChanged) {
                ((INotifyCollectionChanged) newValue).CollectionChanged += this.OnCollectionChanged;
            }

            this.ClearKeyFrameList();
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            this.ClearKeyFrameList();
            this.RegenerateAllPoints();
        }

        private void RegenerateAllPoints() {
            this.CompletelyInvalidateRender();
        }

        protected override void OnRender(DrawingContext dc) {
            List<KeyFramePoint> list = this.GetOrGenerateKeyFrameList();
            if (list == null || list.Count < 1) {
                return;
            }

            double zoom = this.UnitZoom;
            Rect rect;
            if (this.scroller == null) {
                rect = new Rect(new Point(), this.RenderSize);
            }
            else {
                Point location = this.TranslatePoint(new Point(), this.scroller);
                double bound_l = this.scroller.HorizontalOffset;
                double bound_t = this.scroller.VerticalOffset;
                double bound_r = this.scroller.HorizontalOffset + this.scroller.ViewportWidth;
                double bound_b = this.scroller.VerticalOffset + this.scroller.ViewportHeight;

                double x = Maths.Clamp(location.X, bound_l, bound_r);
                double y = Maths.Clamp(location.Y, bound_t, bound_b);
                double right = Maths.Clamp(x + this.ActualWidth, bound_l, bound_r);
                double bottom = Maths.Clamp(y + this.ActualHeight, bound_t, bound_b);
                // rect = new Rect(x, y, Math.Max(0, right - x), Math.Max(0, bottom - y));
                rect = new Rect(x, bound_t, Math.Max(0, right - x), Math.Max(0, bottom - y));
            }

            KeyFramePoint first = list[0], prev = first;
            this.DrawFirstKeyFrameLine(dc, first, zoom, rect);
            for (int i = 1; i < list.Count; i++) {
                KeyFramePoint keyFrame = list[i];
                this.DrawKeyFramesAndLine(dc, prev, keyFrame, zoom, rect);
                prev = keyFrame;
            }

            this.DrawLastKeyFrameLine(dc, prev, zoom, rect);
            if (ReferenceEquals(first, prev)) {
                // there's only 1 key frame
                this.DrawKeyFramePoint(dc, prev, zoom, rect);
            }
        }

        private static Point GetPoint(KeyFramePoint keyFrame, double zoom, Rect rect) {
            if (keyFrame.RenderPoint is Point point) {
                return point;
            }

            AutomationKey key = keyFrame.KeyFrame.OwnerSequence.Key;
            double px = keyFrame.KeyFrame.Timestamp * zoom;
            double offset_y;
            switch (keyFrame.KeyFrame) {
                case KeyFrameDoubleViewModel frame when key.Descriptor is KeyDescriptorDouble fd:
                    offset_y = Maths.Map(frame.Value, fd.Minimum, fd.Maximum, 0, rect.Height);
                    break;
                case KeyFrameLongViewModel frame when key.Descriptor is KeyDescriptorLong fd:
                    offset_y = Maths.Map(frame.Value, fd.Minimum, fd.Maximum, 0, rect.Height);
                    break;
                case KeyFrameBooleanViewModel frame:
                    double offset = (rect.Height / 100) * 10;
                    offset_y = frame.Value ? (rect.Height - offset) : offset;
                    break;
                case KeyFrameVector2ViewModel frame when key.Descriptor is KeyDescriptorVector2 fd:
                    // huh... not sure what to do here
                    offset_y = Maths.Map(frame.Value.X, fd.Minimum.X, fd.Maximum.X, 0, rect.Height);
                    break;
                default: return default;
            }

            point = new Point(px, rect.Height - offset_y);
            keyFrame.RenderArea = rect;
            keyFrame.RenderPoint = point;
            return point;
        }

        private static bool SetValueForScaledMousePoint(KeyFramePoint keyFrame, Rect rect, Point point) {
            AutomationKey key = keyFrame.KeyFrame.OwnerSequence.Key;
            switch (keyFrame.KeyFrame) {
                case KeyFrameDoubleViewModel frame when key.Descriptor is KeyDescriptorDouble fd:
                    frame.Value = Maths.Clamp(Maths.Map(point.Y, rect.Height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum);
                    break;
                case KeyFrameLongViewModel frame when key.Descriptor is KeyDescriptorLong fd:
                    frame.Value = (long) Maths.Clamp(Maths.Map(point.Y, rect.Height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum);
                    break;
                case KeyFrameBooleanViewModel frame:
                    double offset = (rect.Height / 100) * 30;
                    double bound_b = rect.Height - offset;
                    if (point.Y >= bound_b) {
                        frame.Value = false;
                    }
                    else if (point.Y < offset) {
                        frame.Value = true;
                    }
                    else {
                        return false;
                    }

                    return true;
                case KeyFrameVector2ViewModel frame when key.Descriptor is KeyDescriptorVector2 fd:
                    // huh... not sure what to do here
                    frame.Value = new Vector2((float) Maths.Map(point.Y, 0, rect.Height, fd.Minimum.X, fd.Maximum.X), frame.Value.Y);
                    break;
                default: return false;
            }

            return true;
        }

        // draw a horizontal line at the key's Y pos
        private void DrawFirstKeyFrameLine(DrawingContext dc, KeyFramePoint key, double zoom, Rect rect) {
            Point p2 = GetPoint(key, zoom, rect);
            Point p1 = new Point(0, p2.Y);
            if (RectContains(rect, p1) || RectContains(rect, p2)) {
                dc.DrawLine(this.CurvePenTransparent, p1, p2);
                dc.DrawLine(this.CurvePen, p1, p2);
            }
        }

        // draw a horizontal line at the key's Y pos
        private void DrawLastKeyFrameLine(DrawingContext dc, KeyFramePoint key, double zoom, Rect rect) {
            Point a = GetPoint(key, zoom, rect);
            Point b = new Point(rect.Right, a.Y);
            if (RectContains(rect, a) || RectContains(rect, b)) {
                dc.DrawLine(this.CurvePenTransparent, a, b);
                dc.DrawLine(this.CurvePen, a, b);
            }
        }

        // draw a line from a and b (using a's line type, e.g. linear, bezier), then draw a and b
        private void DrawKeyFramesAndLine(DrawingContext dc, KeyFramePoint a, KeyFramePoint b, double zoom, Rect rect) {
            this.DrawKeyFrameLine(dc, a, b, zoom, rect);
            this.DrawKeyFramePoint(dc, a, zoom, rect);
            this.DrawKeyFramePoint(dc, b, zoom, rect);
        }

        private void DrawKeyFramePoint(DrawingContext dc, KeyFramePoint keyFrame, double zoom, Rect rect) {
            Point point = GetPoint(keyFrame, zoom, rect);
            Rect area = new Rect(point.X - 3d, point.Y - 3d, 6d, 6d);
            if (true) { // RectContains(rect, area)
                dc.DrawEllipse(Brushes.Transparent, keyFrame.IsCaptured ? this.MouseOverPen : this.KeyFramePen, point, 3d, 3d);
            }
        }

        private void DrawKeyFrameLine(DrawingContext dc, KeyFramePoint a, KeyFramePoint b, double zoom, Rect rect) {
            Point p1 = GetPoint(a, zoom, rect);
            Point p2 = GetPoint(b, zoom, rect);
            if (RectContains(rect, p1) || RectContains(rect, p2)) {
                dc.DrawLine(this.CurvePenTransparent, p1, p2);
                dc.DrawLine(this.CurvePen, p1, p2);
            }
        }

        public static bool RectContains(Rect rect, Point p) {
            return p.X >= rect.Left && p.X <= rect.Right && p.Y >= rect.Top && p.Y <= rect.Bottom;
        }

        public static bool RectContains(Rect rect, Rect r) {
            return r.Right > rect.Left && r.Left < rect.Right && r.Bottom > rect.Top && r.Top < rect.Bottom;
        }
    }
}