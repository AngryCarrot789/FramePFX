using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Utils;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Editor.Automation {
    public class AutomationSequenceEditor : Control {
        public const double EllipseRadius = 3d;
        public const double EllipseHitRadius = 12d;
        public const double LineThickness = 2d;
        public const double LineHitThickness = 12d;

        public static readonly DependencyProperty OverrideBrushProperty =
            DependencyProperty.Register(
                "OverrideBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    Brushes.DarkGray,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnOverrideBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty KeyFrameBrushProperty =
            DependencyProperty.Register(
                "KeyFrameBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    Brushes.OrangeRed,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnKeyFrameBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty CurveBrushProperty =
            DependencyProperty.Register(
                "CurveBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    Brushes.OrangeRed,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnCurveBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty MouseOverBrushProperty =
            DependencyProperty.Register(
                "MouseOverBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    Brushes.WhiteSmoke,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnMouseOverBrushPropertyChanged((Brush) e.OldValue, (Brush) e.NewValue)));

        public static readonly DependencyProperty SequenceProperty =
            DependencyProperty.Register(
                "Sequence",
                typeof(AutomationSequenceViewModel),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnSequencePropertyChanged((AutomationSequenceViewModel) e.OldValue, (AutomationSequenceViewModel) e.NewValue)));

        public static readonly DependencyProperty IsOverrideEnabledProperty =
            DependencyProperty.Register(
                "IsOverrideEnabled",
                typeof(bool),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    BoolBox.False,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnIsOverrideEnabledPropertyChanged((bool) e.OldValue, (bool) e.NewValue)));

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(1d, (o, e) => ((AutomationSequenceEditor) o).RegenerateAllPoints()));


        public static readonly DependencyProperty MinFrameProperty =
            DependencyProperty.Register(
                "MinFrame",
                typeof(long),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(0L));

        public static readonly DependencyProperty MaxFrameProperty =
            DependencyProperty.Register(
                "MaxFrame",
                typeof(long),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(10000L));

        public Brush OverrideBrush {
            get => (Brush) this.GetValue(OverrideBrushProperty);
            set => this.SetValue(OverrideBrushProperty, value);
        }

        public Brush KeyFrameBrush {
            get => (Brush) this.GetValue(KeyFrameBrushProperty);
            set => this.SetValue(KeyFrameBrushProperty, value);
        }

        public Brush CurveBrush {
            get => (Brush) this.GetValue(CurveBrushProperty);
            set => this.SetValue(CurveBrushProperty, value);
        }

        public Brush MouseOverBrush {
            get => (Brush) this.GetValue(MouseOverBrushProperty);
            set => this.SetValue(MouseOverBrushProperty, value);
        }

        public AutomationSequenceViewModel Sequence {
            get => (AutomationSequenceViewModel) this.GetValue(SequenceProperty);
            set => this.SetValue(SequenceProperty, value);
        }

        public bool IsOverrideEnabled {
            get => (bool) this.GetValue(IsOverrideEnabledProperty);
            set => this.SetValue(IsOverrideEnabledProperty, value.Box());
        }

        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long MinFrame {
            get => (long) this.GetValue(MinFrameProperty);
            set => this.SetValue(MinFrameProperty, value);
        }

        public long MaxFrame {
            get => (long) this.GetValue(MaxFrameProperty);
            set => this.SetValue(MaxFrameProperty, value);
        }

        private Pen keyOverridePen;
        private Pen keyFramePen;
        private Pen curvePen;
        private Pen keyFrameTransparentPen;
        private Pen transparentPenLine;
        private Pen mouseOverPen;
        private Pen lineOverridePen;
        private Pen lineMouseOverPen;
        private Pen overrideValuePen;

        private static readonly Brush TransparentBrush = Brushes.Transparent; // Brushes.Yellow

        internal Pen KeyOverridePen => this.keyOverridePen ?? (this.keyOverridePen = new Pen(this.OverrideBrush ?? Brushes.DarkGray, EllipseRadius));
        internal Pen KeyFramePen => this.keyFramePen ?? (this.keyFramePen = new Pen(this.KeyFrameBrush ?? Brushes.OrangeRed, EllipseRadius));
        internal Pen KeyFrameMouseOverPen => this.mouseOverPen ?? (this.mouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, EllipseRadius));
        internal Pen KeyFrameTransparentPen => this.keyFrameTransparentPen ?? (this.keyFrameTransparentPen = new Pen(TransparentBrush, EllipseHitRadius));
        internal Pen LineOverridePen => this.lineOverridePen ?? (this.lineOverridePen = new Pen(this.OverrideBrush ?? Brushes.DarkGray, LineThickness));
        internal Pen LinePen => this.curvePen ?? (this.curvePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness));
        internal Pen LineMouseOverPen => this.lineMouseOverPen ?? (this.lineMouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, LineThickness));
        internal Pen LineTransparentPen => this.transparentPenLine ?? (this.transparentPenLine = new Pen(TransparentBrush, LineHitThickness));
        internal Pen OverrideValuePen {
            get {
                if (this.overrideValuePen == null) {
                    this.overrideValuePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness);
                    this.overrideValuePen.DashStyle = new DashStyle(new List<double>() {2d, 2d}, 0d);
                }

                return this.overrideValuePen;
            }
        }

        // internal key frame storage; recalculated whenever the ItemsSource changes
        internal readonly List<KeyFramePoint> backingList;
        private KeyFramePoint captured;
        private Point mouseDownPoint;
        private bool justCaptured;

        private readonly PropertyChangedEventHandler keyFramePropertyChangedEventHandler;
        private ScrollViewer scroller;

        public AutomationSequenceEditor() {
            this.backingList = new List<KeyFramePoint>();
            this.keyFramePropertyChangedEventHandler = this.OnKeyFrameViewModelPropertyChanged;
            this.Loaded += this.OnLoaded;
            this.IsHitTestVisible = true;
        }

        public int GetPointIndexByKeyFrame(KeyFrameViewModel keyFrame) {
            List<KeyFramePoint> list = this.backingList;
            for (int i = 0, c = list.Count; i < c; i++) {
                if (ReferenceEquals(list[i].KeyFrame, keyFrame)) {
                    return i;
                }
            }

            return -1;
        }

        public KeyFramePoint GetPointByKeyFrame(KeyFrameViewModel keyFrame) {
            int index = this.GetPointIndexByKeyFrame(keyFrame);
            return index != -1 ? this.backingList[index] : null;
        }

        public bool TryGetPointByKeyFrame(KeyFrameViewModel keyFrame, out KeyFramePoint point) {
            return (point = this.GetPointByKeyFrame(keyFrame)) != null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.scroller = VisualTreeUtils.FindVisualParent<ScrollViewer>(this);
            if (this.scroller == null) {
                return;
            }

            this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
            this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
        }

        protected virtual void OnSequencePropertyChanged(AutomationSequenceViewModel oldValue, AutomationSequenceViewModel newValue) {
            if (oldValue != null) {
                oldValue.OverrideKeyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
                ((INotifyCollectionChanged) oldValue.KeyFrames).CollectionChanged -= this.OnCollectionChanged;
            }

            this.ClearKeyFrameList();
            if (newValue != null) {
                newValue.OverrideKeyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
                ((INotifyCollectionChanged) newValue.KeyFrames).CollectionChanged += this.OnCollectionChanged;
                this.GenerateBackingList(newValue);
            }
        }

        protected virtual void OnIsOverrideEnabledPropertyChanged(bool oldValue, bool newValue) {

        }

        private KeyFramePoint CreatePoint(int index, KeyFrameViewModel keyFrame, bool attachPropertyChangedEvent = true) {
            KeyFramePoint point = KeyFramePoint.ForKeyFrame(this, keyFrame);
            point.Index = index;

            List<KeyFramePoint> list = this.backingList;
            for (int i = index; i < list.Count; i++) {
                list[i].Index++;
            }

            this.backingList.Insert(index, point);
            if (attachPropertyChangedEvent) {
                keyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
            }

            return point;
        }

        private void CreatePoints(int index, List<KeyFrameViewModel> keyFrames, bool attachPropertyChangedEvent = true) {
            List<KeyFramePoint> list = this.backingList;
            for (int i = index; i < list.Count; i++) {
                list[i].Index += keyFrames.Count;
            }

            for (int i = 0; i < keyFrames.Count; i++) {
                KeyFrameViewModel keyFrame = keyFrames[i];
                KeyFramePoint point = KeyFramePoint.ForKeyFrame(this, keyFrame);
                point.Index = index + i;
                this.backingList.Insert(point.Index, point);
                if (attachPropertyChangedEvent) {
                    keyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
                }
            }
        }

        private void ClearCapture(bool releaseMouseCapture = true) {
            if (this.captured == null) {
                return;
            }

            this.captured.IsMovingPoint = false;
            this.captured.IsPointSelected = false;
            this.captured = null;
            this.justCaptured = false;
            if (releaseMouseCapture && this.IsMouseCaptured) {
                this.ReleaseMouseCapture();
            }
        }

        private void CapturePoint(KeyFramePoint point, bool captureMouse = true) {
            this.captured = point;
            point.IsMovingPoint = true;
            point.IsPointSelected = true;
            point.InvalidateRenderData();
            this.justCaptured = true;
            if (captureMouse && !this.IsMouseCaptured) {
                this.CaptureMouse();
            }
        }

        private void RemovePointAt(int index, bool detatchPropertyChangedEvent = true) {
            List<KeyFramePoint> list = this.backingList;
            for (int i = index + 1; i < list.Count; i++) {
                list[i].Index--;
            }

            KeyFramePoint point = list[index];
            if (detatchPropertyChangedEvent) {
                point.KeyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
            }

            list.RemoveAt(index);
            if (ReferenceEquals(point, this.captured)) {
                this.ClearCapture();
            }
        }

        private void RemovePoints(int index, List<KeyFrameViewModel> keyFrames, bool detatchPropertyChangedEvent = true) {
            List<KeyFramePoint> pointList = this.backingList;
            int count = keyFrames.Count;
            if (keyFrames.Count > 1) {
                if (index == -1) { // slow double loop
                    foreach (KeyFrameViewModel toRemove in keyFrames) {
                        int j = this.GetPointIndexByKeyFrame(toRemove);
                        if (j == -1) {
                            throw new Exception("Item was never added");
                        }

                        this.RemovePointAt(j);
                    }
                }
                else {
                    for (int i = 0; i < count; i++) {
                        KeyFramePoint point = pointList[index + i];
                        if (!ReferenceEquals(point.KeyFrame, keyFrames[i])) {
                            throw new Exception("Invalid removal index");
                        }

                        if (detatchPropertyChangedEvent) {
                            point.KeyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
                        }

                        if (ReferenceEquals(point, this.captured)) {
                            this.ClearCapture();
                        }
                    }

                    for (int i = index + count; i < pointList.Count; i++) {
                        pointList[i].Index -= count;
                    }

                    this.backingList.RemoveRange(index, count);
                }
            }
            else if (keyFrames.Count == 1) {
                KeyFrameViewModel toRemove = keyFrames[0];
                if (index == -1 && (index = this.GetPointIndexByKeyFrame(toRemove)) == -1) {
                    throw new Exception("Item was never added");
                }

                KeyFramePoint removedPoint = pointList[index];
                if (!ReferenceEquals(removedPoint.KeyFrame, toRemove)) {
                    throw new Exception("Invalid removal index");
                }

                this.RemovePointAt(index);
                if (ReferenceEquals(removedPoint, this.captured)) {
                    this.ClearCapture();
                }
            }
        }

        private void ClearKeyFrameList(bool detatchPropertyChangedEvent = true) {
            if (detatchPropertyChangedEvent) {
                foreach (KeyFramePoint keyFrame in this.backingList) {
                    keyFrame.KeyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
                }
            }

            this.backingList.Clear();
            if (this.captured != null) {
                this.ClearCapture();
            }
        }

        private void GenerateBackingList(AutomationSequenceViewModel sequence) {
            this.ClearKeyFrameList();

            int i = 0;
            foreach (KeyFrameViewModel keyFrame in sequence.KeyFrames) {
                keyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
                KeyFramePoint keyF = KeyFramePoint.ForKeyFrame(this, keyFrame);
                keyF.Index = i++;
                this.backingList.Add(keyF);
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            AutomationSequenceViewModel sequence = this.Sequence;
            if (sequence == null) {
                throw new Exception($"Dependency property sequence is unavailable");
            }

            switch (e.Action) {
                case NotifyCollectionChangedAction.Add: {
                    if (e.NewItems != null) {
                        int index = e.NewStartingIndex == -1 ? this.backingList.Count : e.NewStartingIndex;
                        this.CreatePoints(index, e.NewItems.Cast<KeyFrameViewModel>().ToList(), true);
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Remove:  {
                    if (e.OldItems == null) {
                        if (e.OldStartingIndex != -1) {
                            this.RemovePointAt(e.OldStartingIndex);
                        }
                    }
                    else {
                        this.RemovePoints(e.OldStartingIndex, e.OldItems.Cast<KeyFrameViewModel>().ToList());
                    }

                    break;
                }
                case NotifyCollectionChangedAction.Replace: {
                    throw new Exception("Cannot handle replace, for now");
                }
                case NotifyCollectionChangedAction.Move: {
                    throw new Exception("Cannot handle move, for now");
                }
                case NotifyCollectionChangedAction.Reset: {
                    this.GenerateBackingList((AutomationSequenceViewModel) sender);
                    break;
                }
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void RegenerateAllPoints() {
            this.CompletelyInvalidateRender();
        }

        private void OnKeyFrameViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            KeyFrameViewModel keyFrame = (KeyFrameViewModel) sender;
            AutomationSequenceViewModel seq = this.Sequence;
            if (seq != null && !ReferenceEquals(keyFrame, seq.OverrideKeyFrame)) {
                KeyFramePoint point = this.backingList.First(x => x.KeyFrame == keyFrame);
                point.InvalidateRenderData();
            }

            this.InvalidateVisual();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            List<KeyFramePoint> list = this.backingList;
            int c = list.Count;
            if (c < 1) {
                return;
            }

            this.mouseDownPoint = e.GetPosition(this);
            if (this.GetIntersection(ref this.mouseDownPoint, out KeyFramePoint hitKey, out LineHitType lineHit)) {
                if (this.captured != null) {
                    this.ClearCapture(lineHit != LineHitType.None);
                }

                if (lineHit == LineHitType.None) {
                    this.CapturePoint(hitKey);
                }

                this.InvalidateVisual();
                e.Handled = true;
                return;
            }

            this.captured = null;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            Point mPos = e.GetPosition(this);
            if (!this.GetIntersection(ref mPos, out KeyFramePoint hitKey, out LineHitType lineHit)) {
                return;
            }

            if (this.captured != null) {
                this.ClearCapture(lineHit != LineHitType.None);
            }

            if (lineHit == LineHitType.None) {
                e.Handled = true;
                hitKey.KeyFrame.OwnerSequence.RemoveKeyFrame(hitKey.KeyFrame);
            }
            else if (this.Sequence is AutomationSequenceViewModel sequence) {
                if (this.justCaptured) {
                    this.mouseDownPoint = mPos;
                    this.justCaptured = false;
                }

                long timestamp = (long) Math.Round(mPos.X / this.UnitZoom);
                KeyFrameViewModel keyFrame;
                switch (sequence.Model.DataType) {
                    case AutomationDataType.Double:
                        sequence.AddKeyFrame(keyFrame = new KeyFrameDoubleViewModel(new KeyFrameDouble(timestamp, ((KeyDescriptorDouble) sequence.Key.Descriptor).DefaultValue)));
                        break;
                    case AutomationDataType.Long:
                        sequence.AddKeyFrame(keyFrame = new KeyFrameLongViewModel(new KeyFrameLong(timestamp, ((KeyDescriptorLong) sequence.Key.Descriptor).DefaultValue)));
                        break;
                    case AutomationDataType.Boolean:
                        sequence.AddKeyFrame(keyFrame = new KeyFrameBooleanViewModel(new KeyFrameBoolean(timestamp, ((KeyDescriptorBoolean) sequence.Key.Descriptor).DefaultValue)));
                        break;
                    case AutomationDataType.Vector2:
                        sequence.AddKeyFrame(keyFrame = new KeyFrameVector2ViewModel(new KeyFrameVector2(timestamp, ((KeyDescriptorVector2) sequence.Key.Descriptor).DefaultValue)));
                        break;
                    default: throw new ArgumentOutOfRangeException();
                }

                if (this.TryGetPointByKeyFrame(keyFrame, out KeyFramePoint point)) {
                    point.SetValueForMousePoint(mPos);
                    this.CapturePoint(point);
                }
            }

            this.InvalidateVisual();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            if (this.captured != null) {
                this.ClearCapture();
            }

            this.InvalidateVisual();
        }

        private KeyFramePoint lastMouseOver;

        private void UpdateMouseOver(Point point) {
            if (this.lastMouseOver != null) {
                this.lastMouseOver.LastLineHitType = LineHitType.None;
                this.lastMouseOver.IsMouseOverPoint = false;
                this.lastMouseOver = null;
            }

            if (!this.GetIntersection(ref point, out KeyFramePoint keyFrame, out LineHitType lineHit)) {
                return;
            }

            this.lastMouseOver = keyFrame;
            this.lastMouseOver.IsMouseOverPoint = lineHit == LineHitType.None;
            this.lastMouseOver.LastLineHitType = lineHit;
            this.InvalidateVisual();
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            this.UpdateMouseOver(e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (this.lastMouseOver != null) {
                this.lastMouseOver.LastLineHitType = LineHitType.None;
                this.lastMouseOver.IsMouseOverPoint = false;
                this.lastMouseOver = null;
                this.InvalidateVisual();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            Point mPos = e.GetPosition(this);
            this.UpdateMouseOver(mPos);

            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.captured != null) {
                    this.ClearCapture();
                }

                if (this.IsMouseCaptured) {
                    this.ReleaseMouseCapture();
                }

                this.InvalidateVisual();
            }

            if (this.captured == null) {
                return;
            }

            // TODO: add minimum and maximum dependency properties

            long min = this.captured.Prev?.KeyFrame.Timestamp ?? this.MinFrame;
            long max = this.captured.Next?.KeyFrame.Timestamp ?? this.MaxFrame;

            if (this.justCaptured) {
                this.mouseDownPoint = mPos;
                this.justCaptured = false;
                return;
            }

            long offset = Math.Max(0, (long) Math.Round(mPos.X / this.UnitZoom));
            long time = this.captured.KeyFrame.Timestamp;
            if ((time + offset) < 0) {
                offset = -time;
            }

            this.captured.KeyFrame.Timestamp = Maths.Clamp(offset, min, max);
            if (!(this.captured is KeyFramePointVec2)) {
                this.captured.SetValueForMousePoint(mPos);
            }

            this.UpdateMouseOver(mPos);
            this.mouseDownPoint = mPos;
        }

        private void CompletelyInvalidateRender() {
            if (this.backingList != null) {
                foreach (KeyFramePoint keyFrame in this.backingList) {
                    keyFrame.InvalidateRenderData();
                }
            }

            this.InvalidateVisual();
        }

        private void OnScrollerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
            this.CompletelyInvalidateRender();
        }

        private void OnScrollerOnSizeChanged(object sender, SizeChangedEventArgs e) {
            this.CompletelyInvalidateRender();
        }

        #region Rendering

        protected override void OnRender(DrawingContext dc) {
            List<KeyFramePoint> list = this.backingList;
            if (list.Count < 1) {
                return;
            }

            if (this.IsOverrideEnabled) {
                dc.PushOpacity(0.5d);
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

            int end = list.Count - 1;
            KeyFramePoint first = list[0], prev = first;
            this.DrawFirstKeyFrameLine(dc, first, ref rect);
            if (end == 0) {
                this.DrawLastKeyFrameLine(dc, first, ref rect);
                first.RenderEllipse(dc, ref rect);
            }
            else {
                for (int i = 1; i < end; i++) {
                    KeyFramePoint keyFrame = list[i];
                    this.DrawKeyFramesAndLine(dc, prev, keyFrame, zoom, ref rect);
                    prev = keyFrame;
                }

                this.DrawLastKeyFrameLine(dc, list[end], ref rect);
                this.DrawKeyFramesAndLine(dc, prev, list[end], zoom, ref rect);
            }

            if (this.IsOverrideEnabled) {
                AutomationSequenceViewModel seq = this.Sequence;
                if (seq != null) {
                    double y = this.ActualHeight - KeyPointUtils.GetY(seq.OverrideKeyFrame, this.ActualHeight);
                    dc.DrawLine(this.OverrideValuePen, new Point(0, y), new Point(rect.Right, y));
                }

                dc.Pop();
            }
        }

        public static Point GetVec2SubPoint(KeyFramePointVec2 keyFrame, double zoom, ref Rect rect) {
            if (!(keyFrame.KeyFrame is KeyFrameVector2ViewModel keyFrameVec)) {
                throw new Exception("Not a vec2");
            }

            KeyDescriptorVector2 desc = (KeyDescriptorVector2) keyFrameVec.OwnerSequence.Key.Descriptor;
            Vector2 vector = keyFrameVec.Value;
            double offset_y_a = Maths.Map(vector.X, desc.Minimum.X, desc.Maximum.X, 0, rect.Height) * zoom;
            double offset_y_b = Maths.Map(vector.Y, desc.Minimum.Y, desc.Maximum.Y, 0, rect.Height);
            return new Point(offset_y_a, rect.Height - offset_y_b);
        }

        // draw a line from a and b (using a's line type, e.g. linear, bezier), then draw a and b
        private void DrawKeyFramesAndLine(DrawingContext dc, KeyFramePoint a, KeyFramePoint b, double zoom, ref Rect rect) {
            this.DrawKeyFrameLine(dc, a, b, ref rect);
            a.RenderEllipse(dc, ref rect);
            b.RenderEllipse(dc, ref rect);
        }

        private void DrawKeyFrameLine(DrawingContext dc, KeyFramePoint a, KeyFramePoint b, ref Rect rect) {
            Point p1 = a.GetLocation();
            Point p2 = b.GetLocation();
            if (RectContains(ref rect, ref p1) || RectContains(ref rect, ref p2)) {
                dc.DrawLine(this.LineTransparentPen, p1, p2);
                if (a.LastLineHitType != LineHitType.Head && a.LastLineHitType != LineHitType.Tail) {
                    dc.DrawLine(this.IsOverrideEnabled ? this.LineOverridePen : (a.LastLineHitType != LineHitType.None ? this.LineMouseOverPen : this.LinePen), p1, p2);
                }
                else {
                    dc.DrawLine(this.IsOverrideEnabled ? this.LineOverridePen : this.LinePen, p1, p2);
                }
            }
        }

        // draw a horizontal line at the key's Y pos
        private void DrawFirstKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point p2 = key.GetLocation();
            Point p1 = new Point(0, p2.Y);
            if (RectContains(ref rect, ref p1) || RectContains(ref rect, ref p2)) {
                dc.DrawLine(this.LineTransparentPen, p1, p2);
                dc.DrawLine(this.IsOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Head ? this.LineMouseOverPen : this.LinePen), p1, p2);
            }
        }

        // draw a horizontal line at the key's Y pos
        private void DrawLastKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point a = key.GetLocation();
            Point b = new Point(rect.Right, a.Y);
            if (RectContains(ref rect, ref a) || RectContains(ref rect, ref b)) {
                dc.DrawLine(this.LineTransparentPen, a, b);
                dc.DrawLine(this.IsOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Tail ? this.LineMouseOverPen : this.LinePen), a, b);
            }
        }

        #endregion

        #region Hit/collision testing

        // using `ref` instead of `in`, because mutable struct and `in` are a recipe for horrible performance

        public static bool RectContains(ref Rect rect, ref Point p) {
            return p.X >= rect.Left && p.X <= rect.Right && p.Y >= rect.Top && p.Y <= rect.Bottom;
        }

        public static bool RectContains(ref Rect rect, ref Rect r) {
            return r.Right > rect.Left && r.Left < rect.Right && r.Bottom > rect.Top && r.Top < rect.Bottom;
        }

        // using ref here halves the stack size (on 64 bit)
        public static bool IsMouseOverLine(ref Point p, ref Point a, ref Point b, double thickness) {
            double c1 = Math.Abs((b.X - a.X) * (a.Y - p.Y) - (a.X - p.X) * (b.Y - a.Y));
            double c2 = Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
            if ((c1 / c2) > (thickness / 2)) {
                return false;
            }

            double ht = thickness / 2d;
            double minX = Math.Min(a.X, b.X) - ht;
            double maxX = Math.Max(a.X, b.X) + ht;
            double minY = Math.Min(a.Y, b.Y) - ht;
            double maxY = Math.Max(a.Y, b.Y) + ht;
            return p.X >= minX && p.X <= maxX && p.Y >= minY && p.Y <= maxY;
        }

        /// <summary>
        /// Performs a hit test across each key frame and it's line
        /// </summary>
        /// <param name="p">The point to test (e.g. mouse cursor)</param>
        /// <param name="keyFrame">The hit key frame, or it's associated line if <param name="lineHit"></param> is true</param>
        /// <param name="lineHit">Whether or not the hit was a line</param>
        /// <returns>True if something was hit, otherwise false, meaning <param name="keyFrame"></param> will be null and <param name="lineHit"></param> will be false</returns>
        public bool GetIntersection(ref Point p, out KeyFramePoint keyFrame, out LineHitType lineHit) {
            List<KeyFramePoint> list = this.backingList;
            int c = list.Count;
            if (c > 0) {
                Point lastPoint = new Point(0, list[0].GetLocation().Y);
                for (int i = 0; i < c; i++) {
                    keyFrame = this.backingList[i];
                    Point point = keyFrame.GetLocation();

                    // lazy; AABB intersection
                    const double r1 = EllipseHitRadius, r2 = r1 * 2d;
                    Rect point_area = new Rect(point.X - r1, point.Y - r1, r2, r2);
                    if (RectContains(ref point_area, ref p)) {
                        lineHit = LineHitType.None;
                        return true;
                    }
                    else if (IsMouseOverLine(ref p, ref lastPoint, ref point, LineHitThickness)) {
                        if (i > 0) {
                            keyFrame = this.backingList[i - 1];
                        }

                        lineHit = i == 0 ? LineHitType.Head : LineHitType.Normal;
                        return true;
                    }

                    lastPoint = point;
                }

                Point endPoint = new Point(this.ActualWidth, lastPoint.Y);
                if (IsMouseOverLine(ref p, ref lastPoint, ref endPoint, LineHitThickness)) {
                    keyFrame = this.backingList[c - 1];
                    lineHit = LineHitType.Tail;
                    return true;
                }
            }

            keyFrame = null;
            lineHit = LineHitType.None;
            return false;
        }

        #endregion

        #region standard property change handlers

        protected virtual void OnOverrideBrushPropertyChanged(Brush oldValue, Brush newValue) => this.keyOverridePen = null;
        protected virtual void OnKeyFrameBrushPropertyChanged(Brush oldValue, Brush newValue) => this.keyFramePen = null;
        protected virtual void OnCurveBrushPropertyChanged(Brush oldValue, Brush newValue) => this.curvePen = null;
        protected virtual void OnMouseOverBrushPropertyChanged(Brush oldValue, Brush newValue) => this.mouseOverPen = null;

        #endregion
    }
}