using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Core.Automation.Keyframe;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Utils;
using FramePFX.Utils;
using Rect = System.Windows.Rect;
using Vector = System.Windows.Vector;

namespace FramePFX.Editor.Automation {
    public class AutomationSequenceEditor : Control {
        public const double EllipseRadius = 2.5d;
        public const double EllipseThickness = 1d;
        public const double EllipseHitRadius = 12d;
        public const double LineThickness = 2d;
        public const double LineHitThickness = 12d;

        public static readonly DependencyProperty OverrideModeBrushProperty =
            DependencyProperty.Register(
                "OverrideModeBrush",
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

        public static readonly DependencyProperty PlacementPlaneBrushProperty =
            DependencyProperty.Register(
                "PlacementPlaneBrush",
                typeof(Brush),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(Brushes.SlateGray));

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
                new PropertyMetadata(1d, (o, e) => ((AutomationSequenceEditor) o).InvalidKeyFrameDataAndRender()));

        public static readonly DependencyProperty FrameBeginProperty =
            DependencyProperty.Register(
                "FrameBegin",
                typeof(long),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(0L));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(10000L));

        public static readonly DependencyProperty IsPlacementPlaneEnabledProperty =
            DependencyProperty.Register(
                "IsPlacementPlaneEnabled",
                typeof(bool),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(BoolBox.False, FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush OverrideModeBrush {
            get => (Brush) this.GetValue(OverrideModeBrushProperty);
            set => this.SetValue(OverrideModeBrushProperty, value);
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

        public Brush PlacementPlaneBrush {
            get => (Brush) this.GetValue(PlacementPlaneBrushProperty);
            set => this.SetValue(PlacementPlaneBrushProperty, value);
        }

        public AutomationSequenceViewModel Sequence {
            get => (AutomationSequenceViewModel) this.GetValue(SequenceProperty);
            set => this.SetValue(SequenceProperty, value);
        }

        public bool IsOverrideEnabled {
            get => (bool) this.GetValue(IsOverrideEnabledProperty);
            set => this.SetValue(IsOverrideEnabledProperty, value.Box());
        }

        internal bool isOverrideEnabled;

        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long FrameBegin {
            get => (long) this.GetValue(FrameBeginProperty);
            set => this.SetValue(FrameBeginProperty, value);
        }

        public long FrameDuration {
            get => (long) this.GetValue(FrameDurationProperty);
            set => this.SetValue(FrameDurationProperty, value);
        }

        public bool IsPlacementPlaneEnabled {
            get => (bool) this.GetValue(IsPlacementPlaneEnabledProperty);
            set => this.SetValue(IsPlacementPlaneEnabledProperty, value.Box());
        }

        private Pen keyOverridePen;
        private Pen keyFramePen;
        private Pen curvePen;
        private Pen keyFrameTransparentPen;
        private Pen transparentPenLine;
        private Pen mouseOverPen;
        private Pen lineOverridePen;
        private Pen lineMouseOverPen;
        private Pen overrideModeValueLinePen;

        internal readonly List<KeyFramePoint> backingList;
        internal readonly Dictionary<KeyFrameViewModel, KeyFramePoint> vmToPoint;
        private ScrollViewer scroller;
        private readonly PropertyChangedEventHandler keyFramePropertyChangedEventHandler;

        private KeyFramePoint captured;
        private Point lastMousePoint;
        private Point originMousePoint;
        private bool isCaptureInitialised;
        private KeyFramePoint lastMouseOver;
        private LineHitType captureLineHit;
        private DragMode? dragMode;

        private static readonly Brush TransparentBrush = Brushes.Transparent; // Brushes.Yellow

        internal Pen KeyOverridePen => this.keyOverridePen ?? (this.keyOverridePen = new Pen(this.OverrideModeBrush ?? Brushes.DarkGray, EllipseThickness));
        internal Pen KeyFramePen => this.keyFramePen ?? (this.keyFramePen = new Pen(this.KeyFrameBrush ?? Brushes.OrangeRed, EllipseThickness));
        internal Pen KeyFrameMouseOverPen => this.mouseOverPen ?? (this.mouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, EllipseThickness));
        internal Pen KeyFrameTransparentPen => this.keyFrameTransparentPen ?? (this.keyFrameTransparentPen = new Pen(TransparentBrush, EllipseHitRadius));
        internal Pen LineOverridePen => this.lineOverridePen ?? (this.lineOverridePen = new Pen(this.OverrideModeBrush ?? Brushes.DarkGray, LineThickness));
        internal Pen LinePen => this.curvePen ?? (this.curvePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness));
        internal Pen LineMouseOverPen => this.lineMouseOverPen ?? (this.lineMouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, LineThickness));
        internal Pen LineTransparentPen => this.transparentPenLine ?? (this.transparentPenLine = new Pen(TransparentBrush, LineHitThickness));
        internal Pen OverrideModeValueLinePen {
            get {
                if (this.overrideModeValueLinePen == null) {
                    this.overrideModeValueLinePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness) {DashStyle = new DashStyle(new List<double>() {2d, 2d}, 0d)};
                }

                return this.overrideModeValueLinePen;
            }
        }

        public AutomationSequenceEditor() {
            this.backingList = new List<KeyFramePoint>();
            this.vmToPoint = new Dictionary<KeyFrameViewModel, KeyFramePoint>();
            this.keyFramePropertyChangedEventHandler = this.OnKeyFrameViewModelPropertyChanged;
            this.Loaded += this.OnLoaded;
            this.IsHitTestVisible = true;
        }

        public int GetPointIndexByKeyFrame(KeyFrameViewModel keyFrame) {
            //List<KeyFramePoint> list = this.backingList;
            //for (int i = 0, c = list.Count; i < c; i++) {
            //    KeyFramePoint frame = list[i];
            //    if (frame.Index != i) {
            //        #if DEBUG
            //        Debugger.Break();
            //        throw new Exception($"Invalid index: {i} != {frame.Index}");
            //        #else
            //        frame.Index = i;
            //        #endif
            //    }
            //    if (ReferenceEquals(frame.keyFrame, keyFrame)) {
            //        return i;
            //    }
            //}

            return this.vmToPoint.TryGetValue(keyFrame, out KeyFramePoint point) ? point.Index : -1;
        }

        public KeyFramePoint GetPointByKeyFrame(KeyFrameViewModel keyFrame) {
            int index = this.GetPointIndexByKeyFrame(keyFrame);
            return index != -1 ? this.backingList[index] : null;
        }

        public bool TryGetPointByKeyFrame(KeyFrameViewModel keyFrame, out KeyFramePoint point) {
            return (point = this.GetPointByKeyFrame(keyFrame)) != null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.scroller = VisualTreeUtils.FindParent<ScrollViewer>(this);
            if (this.scroller == null) {
                return;
            }

            this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
            this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
        }

        #region Key Frame Creation/Deletion

        public KeyFrameViewModel CreateKeyFrameAt(AutomationSequenceViewModel sequence, Point point, bool capturePoint) {
            return this.CreateKeyFrameAt(sequence, point, ref capturePoint);
        }

        public KeyFrameViewModel CreateKeyFrameAt(AutomationSequenceViewModel sequence, Point point, ref bool capturePoint) {
            long timestamp = (long) Math.Round(point.X / this.UnitZoom);
            KeyFrameViewModel keyFrame;
            switch (sequence.Model.DataType) {
                case AutomationDataType.Float:   sequence.AddKeyFrame(keyFrame = new KeyFrameFloatViewModel(new KeyFrameFloat(timestamp, ((KeyDescriptorFloat) sequence.Key.Descriptor).DefaultValue))); break;
                case AutomationDataType.Double:  sequence.AddKeyFrame(keyFrame = new KeyFrameDoubleViewModel(new KeyFrameDouble(timestamp, ((KeyDescriptorDouble) sequence.Key.Descriptor).DefaultValue))); break;
                case AutomationDataType.Long:    sequence.AddKeyFrame(keyFrame = new KeyFrameLongViewModel(new KeyFrameLong(timestamp, ((KeyDescriptorLong) sequence.Key.Descriptor).DefaultValue))); break;
                case AutomationDataType.Boolean: sequence.AddKeyFrame(keyFrame = new KeyFrameBooleanViewModel(new KeyFrameBoolean(timestamp, ((KeyDescriptorBoolean) sequence.Key.Descriptor).DefaultValue))); break;
                case AutomationDataType.Vector2: sequence.AddKeyFrame(keyFrame = new KeyFrameVector2ViewModel(new KeyFrameVector2(timestamp, ((KeyDescriptorVector2) sequence.Key.Descriptor).DefaultValue))); break;
                default: throw new ArgumentOutOfRangeException();
            }

            if (this.TryGetPointByKeyFrame(keyFrame, out KeyFramePoint keyFramePoint)) {
                keyFramePoint.SetValueForMousePoint(point);
                if (capturePoint) {
                    this.SetPointCaptured(keyFramePoint, true, LineHitType.None);
                    this.isCaptureInitialised = false;
                }
            }
            else {
                // this shouldn't really happen, because when a new key frame is created and added to the sequence, the
                // collection change events should result in a new KeyFramePoint being created at some point
                Debug.WriteLine($"Failed to get point by key frame: {keyFrame}");
                capturePoint = false;
            }

            return keyFrame;
        }

        public void RemoveKeyFrameAt(AutomationSequenceViewModel sequence, int index) {
            sequence.RemoveKeyFrameAt(index);
        }

        #endregion

        #region Key point creation/deletion

        private void CreatePoints(int index, List<KeyFrameViewModel> keyFrames, bool attachPropertyChangedEvent = true) {
            int i, lc = this.backingList.Count, kc = keyFrames.Count;
            for (i = index; i < lc; i++) {
                this.backingList[i].Index += kc;
            }

            for (i = 0; i < kc; i++) {
                KeyFrameViewModel keyFrame = keyFrames[i];
                if (this.vmToPoint.ContainsKey(keyFrame)) {
                    throw new Exception("Point was already added");
                }

                KeyFramePoint point = KeyFramePoint.ForKeyFrame(this, keyFrame);
                point.Index = index + i;
                this.backingList.Insert(point.Index, point);
                this.vmToPoint[keyFrame] = point;
                if (attachPropertyChangedEvent) {
                    keyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
                }
            }
        }

        // Do not call unless the view model has been updated accordingly!
        // This is only invoked via the collection changed handlers
        private void RemovePointAt(int index, bool detatchPropertyChangedEvent = true) {
            KeyFramePoint point = this.backingList[index];
            if (point == this.captured) {
                this.ClearCapture();
            }

            for (int i = index + 1, c = this.backingList.Count; i < c; i++) {
                this.backingList[i].Index--;
            }

            if (detatchPropertyChangedEvent) {
                point.keyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
            }

            this.backingList.RemoveAt(index);
            if (!this.vmToPoint.Remove(point.keyFrame)) {
                throw new Exception("Point was not stored in the backing map");
            }
        }

        private void RemovePoints(int index, List<KeyFrameViewModel> keyFrames, bool detatchPropertyChangedEvent = true) {
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
                        KeyFramePoint point = this.backingList[index + i];
                        if (!ReferenceEquals(point.keyFrame, keyFrames[i])) {
                            throw new Exception("Invalid removal index");
                        }
                    }

                    for (int i = index + count; i < this.backingList.Count; i++) {
                        KeyFramePoint point = this.backingList[i];
                        if (detatchPropertyChangedEvent) {
                            point.keyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
                        }

                        if (ReferenceEquals(point, this.captured)) {
                            this.ClearCapture();
                        }

                        point.Index -= count;
                        if (!this.vmToPoint.Remove(point.keyFrame)) { // will corrupt entire sequence editor but that's my fault if it happens
                            throw new Exception("Point was not stored in the backing map");
                        }
                    }

                    this.backingList.RemoveRange(index, count);
                }
            }
            else if (keyFrames.Count == 1) {
                KeyFrameViewModel toRemove = keyFrames[0];
                if (index == -1 && (index = this.GetPointIndexByKeyFrame(toRemove)) == -1) {
                    throw new Exception("Item was never added");
                }

                KeyFramePoint removedPoint = this.backingList[index];
                if (!ReferenceEquals(removedPoint.keyFrame, toRemove)) {
                    throw new Exception("Invalid removal index: key point reference mis-match");
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
                    keyFrame.keyFrame.PropertyChanged -= this.keyFramePropertyChangedEventHandler;
                }
            }

            this.backingList.Clear();
            this.vmToPoint.Clear();
            if (this.captured != null) {
                this.ClearCapture();
            }
        }

        private void GenerateBackingList(AutomationSequenceViewModel sequence) {
            this.ClearKeyFrameList();

            ReadOnlyObservableCollection<KeyFrameViewModel> list = sequence.KeyFrames;
            for (int i = 0, c = list.Count; i < c; i++) {
                KeyFrameViewModel keyFrame = sequence.KeyFrames[i];
                keyFrame.PropertyChanged += this.keyFramePropertyChangedEventHandler;
                KeyFramePoint kf = KeyFramePoint.ForKeyFrame(this, keyFrame);
                kf.Index = i;
                this.backingList.Add(kf);
                this.vmToPoint[keyFrame] = kf;
            }
        }

        #endregion

        #region Key point capture

        private void ClearCapture(bool releaseMouseCapture = true) {
            if (this.captured == null) {
                return;
            }

            this.captured.IsMovingPoint = false;
            this.captured.IsPointSelected = false;
            this.captured = null;
            this.isCaptureInitialised = false;
            this.dragMode = null;
            if (releaseMouseCapture && this.IsMouseCaptured) {
                this.ReleaseMouseCapture();
            }
        }

        private bool ignoreMouseMove;
        private WriteableBitmap bitmap;

        private void SetPointCaptured(KeyFramePoint point, bool captureMouse, LineHitType lineHit) {
            this.captured = point;
            point.IsMovingPoint = true;
            point.IsPointSelected = true;
            this.isCaptureInitialised = true;
            this.captureLineHit = lineHit;
            this.dragMode = null;
            if (captureMouse && !this.IsMouseCaptured) {
                this.ignoreMouseMove = true;
                this.CaptureMouse();
                this.ignoreMouseMove = false;
            }
        }

        #endregion

        #region Event handlers

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
            this.isOverrideEnabled = newValue;
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
                        this.CreatePoints(index, e.NewItems.Cast<KeyFrameViewModel>().ToList());
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

            this.InvalidKeyFrameDataAndRender();
        }

        private void OnKeyFrameViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            KeyFrameViewModel keyFrame = (KeyFrameViewModel) sender;
            AutomationSequenceViewModel seq = this.Sequence;
            if (seq != null && !ReferenceEquals(keyFrame, seq.OverrideKeyFrame)) {
                KeyFramePoint point = this.backingList.First(x => x.keyFrame == keyFrame);
                point.InvalidateRenderData();
                point.Prev?.InvalidateRenderData();
            }

            this.InvalidateVisual();
        }

        private void OnScrollerOnScrollChanged(object sender, ScrollChangedEventArgs e) {
            this.InvalidKeyFrameDataAndRender();
        }

        private void OnScrollerOnSizeChanged(object sender, SizeChangedEventArgs e) {
            this.InvalidKeyFrameDataAndRender();
        }

        #endregion

        #region User Input Handling

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            if (this.backingList.Count < 1) {
                return;
            }

            this.lastMousePoint = e.GetPosition(this);
            if (this.GetIntersection(ref this.lastMousePoint, out KeyFramePoint hitKey, out LineHitType lineHit)) {
                if (this.captured != null) {
                    this.ClearCapture(lineHit != LineHitType.None);
                }

                this.SetPointCaptured(hitKey, true, lineHit);
                this.InvalidateVisual();
                e.Handled = true;
                return;
            }

            this.captured = null;
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            Point mPos = e.GetPosition(this);
            if (this.IsPlacementPlaneEnabled && this.Sequence is AutomationSequenceViewModel sequence) {
                this.CreateKeyFrameAt(sequence, mPos, true);
                e.Handled = true;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e) {
            base.OnMouseDoubleClick(e);
            if (e.ChangedButton != MouseButton.Left) {
                return;
            }

            Point mPos = e.GetPosition(this);
            if (this.GetIntersection(ref mPos, out KeyFramePoint hitKey, out LineHitType lineHit)) {
                if (this.captured != null) {
                    this.ClearCapture(lineHit != LineHitType.None);
                }

                if (lineHit == LineHitType.None) {
                    e.Handled = true;
                    hitKey.keyFrame.OwnerSequence.RemoveKeyFrameAt(hitKey.Index);
                }
                else if (this.Sequence is AutomationSequenceViewModel sequence) {
                    if (this.isCaptureInitialised) {
                        this.lastMousePoint = mPos;
                        this.isCaptureInitialised = false;
                    }

                    this.CreateKeyFrameAt(sequence, mPos, true);
                }

                this.InvalidateVisual();
                e.Handled = true;
            }
            else if (this.Sequence is AutomationSequenceViewModel sequence) {
                if (this.isCaptureInitialised) {
                    this.lastMousePoint = mPos;
                    this.isCaptureInitialised = false;
                }

                this.CreateKeyFrameAt(sequence, mPos, true);
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            if (this.captured != null) {
                if (this.isCaptureInitialised && this.captureLineHit == LineHitType.None && this.Sequence is AutomationSequenceViewModel sequence) {
                    int index = this.vmToPoint.TryGetValue(this.captured.keyFrame, out KeyFramePoint p) ? p.Index : -1; // this.backingList.IndexOf(this.captured);
                    if (index == -1) {
                        throw new Exception("Captured key frame not found in the backing list?");
                    }
                    else {
                        this.RemoveKeyFrameAt(sequence, index);
                    }
                }

                this.ClearCapture();
            }

            this.InvalidateVisual();
        }

        private void UpdateMouseOver(Point point, bool invalidateRender = true) {
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
            if (invalidateRender) {
                this.InvalidateVisual();
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            if (this.backingList.Count < 1 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0) {
                this.IsPlacementPlaneEnabled = true;
            }

            this.UpdateMouseOver(e.GetPosition(this), !this.IsPlacementPlaneEnabled);
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (this.lastMouseOver != null) {
                this.lastMouseOver.LastLineHitType = LineHitType.None;
                this.lastMouseOver.IsMouseOverPoint = false;
                this.lastMouseOver = null;
            }

            if (this.IsPlacementPlaneEnabled) {
                this.IsPlacementPlaneEnabled = false;
            }

            this.InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e) {
            base.OnMouseMove(e);
            if (this.ignoreMouseMove) {
                return;
            }

            Point mPos = e.GetPosition(this);
            this.UpdateMouseOver(mPos, false);

            if (e.LeftButton != MouseButtonState.Pressed) {
                if (this.captured != null) {
                    this.ClearCapture();
                }

                this.InvalidateVisual();
                return;
            }

            if (this.captured == null) {
                return;
            }

            // TODO: add minimum and maximum dependency properties
            KeyFramePoint prev = this.captured.Prev;
            KeyFramePoint next = this.captured.Next;

            long min = prev?.keyFrame.Timestamp ?? (this.FrameBegin);
            long max = next?.keyFrame.Timestamp ?? (this.FrameBegin + this.FrameDuration - 1);

            if (this.isCaptureInitialised) {
                this.lastMousePoint = mPos;
                this.originMousePoint = mPos;
                this.isCaptureInitialised = false;
                return;
            }

            Vector mPosDiff = mPos - this.lastMousePoint;
            bool isShiftPressed = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            bool isAltPressed = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            if (!(this.dragMode is DragMode mode)) {
                if (this.captureLineHit == LineHitType.None) {
                    if (isShiftPressed) {
                        if (!Maths.Equals(Math.Abs(mPosDiff.Y), 0d)) {
                            this.dragMode = mode = DragMode.VerticalKeyFrame;
                        }
                        else if (!Maths.Equals(Math.Abs(mPosDiff.X), 0d)) {
                            this.dragMode = mode = DragMode.HorizontalKeyFrame;
                        }
                        else {  // return; // no mouse movement???
                            this.dragMode = mode = DragMode.FullKeyFrame;
                        }
                    }
                    else {
                        this.dragMode = mode = DragMode.FullKeyFrame;
                    }
                }
                else if (isAltPressed) {
                    this.dragMode = mode = DragMode.LineCurveAmount;
                }
                else {
                    mode = DragMode.None;
                    this.dragMode = null;
                }
            }

            if (mode == DragMode.LineCurveAmount) {
                // double diff = mPos.Y - this.originMousePoint.Y;
                // double mapped = Maths.Map(60d - diff, -60d, 60d, -1d, 1d);
                // this.captured.keyFrame.CurveBendAmount = Maths.Clamp(mapped, -1d, 1d);
                // this.captured.InvalidateRenderData();
            }
            else {
                if (mode == DragMode.FullKeyFrame || mode == DragMode.HorizontalKeyFrame) {
                    long newTime = Math.Max(0, (long) Math.Round(mPos.X / this.UnitZoom));
                    long oldTime = this.captured.keyFrame.Timestamp;
                    if ((oldTime + newTime) < 0) {
                        newTime = -oldTime;
                    }

                    this.captured.keyFrame.Timestamp = Maths.Clamp(newTime, min, max);
                }

                if (mode == DragMode.FullKeyFrame || mode == DragMode.VerticalKeyFrame) {
                    this.captured.SetValueForMousePoint(mPos);
                }
            }

            this.UpdateMouseOver(mPos);
            this.lastMousePoint = mPos;
        }

        #endregion

        #region Rendering

        // protected override void OnRender(DrawingContext dc) {
        //     List<KeyFramePoint> list = this.backingList;
        //     if (list.Count < 1) {
        //         if (this.IsPlacementPlaneEnabled) {
        //             dc.DrawRectangle(this.PlacementPlaneBrush, null, new Rect(new Point(), this.RenderSize));
        //         }
        //
        //         return;
        //     }
        //
        //     if (this.isOverrideEnabled) {
        //         dc.PushOpacity(0.5d);
        //     }
        //
        //     Rect visible;
        //     if (this.scroller == null) {
        //         visible = new Rect(new Point(), this.RenderSize);
        //     }
        //     else {
        //         Point location = this.TranslatePoint(new Point(), this.scroller);
        //         double x = location.X > 0 ? 0 : -location.X;
        //         double y = location.Y > 0 ? 0 : -location.Y;
        //         double w = Math.Min(Math.Min(location.X, 0) + this.ActualWidth, this.scroller.ViewportWidth);
        //         double h = Math.Min(Math.Min(location.Y, 0) + this.ActualHeight, this.scroller.ViewportHeight);
        //         visible = new Rect(x, y, Math.Max(w, 0), Math.Max(h, 0)); // only includes control bounds
        //         // visible = new Rect(x, y, this.scroller.ViewportWidth, this.scroller.ViewportHeight); // includes bounds of entire scroll viewer
        //     }
        //
        //     int end = list.Count - 1;
        //     KeyFramePoint first = list[0], prev = first;
        //     this.DrawFirstKeyFrameLine(dc, first, ref visible);
        //     if (end == 0) {
        //         this.DrawLastKeyFrameLine(dc, first, ref visible);
        //         first.RenderEllipse(dc, ref visible);
        //     }
        //     else {
        //         for (int i = 1; i < end; i++) {
        //             KeyFramePoint keyFrame = list[i];
        //             DrawKeyFramesAndLine(dc, prev, keyFrame, ref visible);
        //             prev = keyFrame;
        //         }
        //
        //         this.DrawLastKeyFrameLine(dc, list[end], ref visible);
        //         DrawKeyFramesAndLine(dc, prev, list[end], ref visible);
        //     }
        //
        //     if (this.isOverrideEnabled) {
        //         AutomationSequenceViewModel seq = this.Sequence;
        //         if (seq != null) {
        //             double y = this.ActualHeight - KeyPointUtils.GetY(seq.OverrideKeyFrame, this.ActualHeight);
        //             dc.DrawLine(this.OverrideModeValueLinePen, new Point(0, y), new Point(visible.Right, y));
        //         }
        //
        //         dc.Pop();
        //     }
        // }

        public static Rect GetVisibleRect(ScrollViewer scroller, UIElement element) {
            Rect rect;
            Size size = element.RenderSize;
            if (scroller == null) {
                rect = new Rect(0, 0, size.Width, size.Height);
            }
            else {
                Point position = element.TranslatePoint(new Point(), scroller);
                double r1L = scroller.HorizontalOffset;
                double r1T = scroller.VerticalOffset;
                double r1R = r1L + scroller.ViewportWidth;
                double r1B = r1T + scroller.ViewportHeight;
                double r2L = r1L + position.X;
                double r2T = r1T + position.Y;
                double r2R = r2L + size.Width;
                double r2B = r2T + size.Height;
                if (r1L > r2R || r1R < r2L || r1T > r2B || r1B < r2T) {
                    rect = new Rect();
                }
                else {
                    double x1 = Math.Max(r1L, r2L);
                    double y1 = Math.Max(r1T, r2T);
                    double x2 = Math.Min(r1R, r2R);
                    double y2 = Math.Min(r1B, r2B);
                    rect = new Rect(x1 - r2L, y1 - r2T, x2 - x1, y2 - y1);
                }
            }

            return rect;
        }

        protected override void OnRender(DrawingContext dc) {
            List<KeyFramePoint> list = this.backingList;
            if (list.Count < 1) {
                if (this.IsPlacementPlaneEnabled) {
                    dc.DrawRectangle(this.PlacementPlaneBrush, null, new Rect(new Point(), this.RenderSize));
                }

                return;
            }

            Rect visible = GetVisibleRect(this.scroller, this);
            if (this.isOverrideEnabled) {
                dc.PushOpacity(0.5d);
            }

            // SKImageInfo skImageInfo = new SKImageInfo(size1.Width, size1.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            // if (this.bitmap == null || frameInfo.Width != this.bitmap.PixelWidth || frameInfo.Height != this.bitmap.PixelHeight) {
            //     this.bitmap = new WriteableBitmap(
            //         frameInfo.Width, pixelSize.Height,
            //         scaleX == 1d ? 96d : (96d * scaleX),
            //         scaleY == 1d ? 96d : (96d * scaleY),
            //         PixelFormats.Pbgra32, null);
            // }

            // using (SKSurface surface = SKSurface.Create())
            int end = list.Count - 1;
            KeyFramePoint first = list[0], prev = first;
            this.DrawFirstKeyFrameLine(dc, first, ref visible);
            if (end == 0) {
                this.DrawLastKeyFrameLine(dc, first, ref visible);
                first.RenderEllipse(dc, ref visible);
            }
            else {
                for (int i = 1; i < end; i++) {
                    KeyFramePoint keyFrame = list[i];
                    DrawKeyFramesAndLine(dc, prev, keyFrame, ref visible);
                    prev = keyFrame;
                }

                this.DrawLastKeyFrameLine(dc, list[end], ref visible);
                DrawKeyFramesAndLine(dc, prev, list[end], ref visible);
            }

            if (this.isOverrideEnabled) {
                AutomationSequenceViewModel seq = this.Sequence;
                if (seq != null) {
                    double y = this.ActualHeight - KeyPointUtils.GetY(seq.OverrideKeyFrame, this.ActualHeight);
                    dc.DrawLine(this.OverrideModeValueLinePen, new Point(0, y), new Point(visible.Right, y));
                }

                dc.Pop();
            }
        }

        private void InvalidKeyFrameDataAndRender() {
            if (this.backingList != null) {
                foreach (KeyFramePoint keyFrame in this.backingList) {
                    keyFrame.InvalidateRenderData();
                }
            }

            this.InvalidateVisual();
        }

        // draw a line from a and b (using a's line type, e.g. linear, bezier), then draw a and b
        private static void DrawKeyFramesAndLine(DrawingContext dc, KeyFramePoint a, KeyFramePoint b, ref Rect rect) {
            a.RenderLine(dc, b, ref rect);
            a.RenderEllipse(dc, ref rect);
            b.RenderEllipse(dc, ref rect);
        }

        // draw a horizontal line at the key's Y pos
        private void DrawFirstKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point p2 = key.GetLocation();
            Point p1 = new Point(0, p2.Y);
            if (RectContains(ref rect, ref p1) || RectContains(ref rect, ref p2)) {
                dc.DrawLine(this.LineTransparentPen, p1, p2);
                dc.DrawLine(this.isOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Head ? this.LineMouseOverPen : this.LinePen), p1, p2);
            }
        }

        // draw a horizontal line at the key's Y pos
        private void DrawLastKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point a = key.GetLocation();
            Point b = ClampRightSide(ref rect, new Point(rect.Right, a.Y));
            if (RectContains(ref rect, ref a) || RectContains(ref rect, ref b)) {
                dc.DrawLine(this.LineTransparentPen, a, b);
                dc.DrawLine(this.isOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Tail ? this.LineMouseOverPen : this.LinePen), a, b);
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

        /// <summary>
        /// </summary>
        /// <param name="rect">[in]</param>
        /// <param name="point">[out]</param>
        /// <returns></returns>
        public static Point ClampBounds(ref Rect rect, Point point) {
            return new Point(Maths.Clamp(point.X, rect.Left, rect.Right), Maths.Clamp(point.Y, rect.Top, rect.Bottom));
        }

        public static Point ClampRightSide(ref Rect rect, Point point) {
            return new Point(Math.Min(point.X, rect.Right), point.Y);
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

        // TODO: iterate through all key frames to find the one which the mouse is closest to maybe?

        public bool GetIntersection(ref Point p, out KeyFramePoint keyFrame, out LineHitType lineHit) {
            return this.GetIntersectionActuallyBinarySearch(ref p, out keyFrame, out lineHit);
            // return this.GetIntersection(ref p, out keyFrame, out lineHit, out _);
        }

        // TODO: binary search maybe?
        /// <summary>
        /// Performs a hit test across each key frame and it's line
        /// </summary>
        /// <param name="pos">The point to test (e.g. mouse cursor)</param>
        /// <param name="keyFrame">The hit key frame, or it's associated line if <param name="lineHit"></param> is true</param>
        /// <param name="lineHit">Whether or not the hit was a line</param>
        /// <returns>True if something was hit, otherwise false, meaning <param name="keyFrame"></param> will be null and <param name="lineHit"></param> will be false</returns>
        public bool GetIntersection(ref Point p, out KeyFramePoint keyFrame, out LineHitType lineHit, out int index) {
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
                        index = i;
                        return true;
                    }
                    else if (IsMouseOverLine(ref p, ref lastPoint, ref point, LineHitThickness)) {
                        if (i > 0) {
                            keyFrame = this.backingList[index = i - 1];
                        }
                        else {
                            index = i;
                        }

                        lineHit = i == 0 ? LineHitType.Head : LineHitType.Normal;
                        return true;
                    }

                    lastPoint = point;
                }

                Point endPoint = new Point(this.ActualWidth, lastPoint.Y);
                if (IsMouseOverLine(ref p, ref lastPoint, ref endPoint, LineHitThickness)) {
                    keyFrame = this.backingList[index = c - 1];
                    lineHit = LineHitType.Tail;
                    return true;
                }
            }

            keyFrame = null;
            lineHit = LineHitType.None;
            index = -1;
            return false;
        }

        private const double R1 = EllipseHitRadius, R2 = R1 * 2d;

        public bool GetIntersectionActuallyBinarySearch(ref Point p, out KeyFramePoint keyFrame, out LineHitType lineHit) {
            List<KeyFramePoint> list = this.backingList;
            int count = list.Count, i = 0, j = count - 1;
            if (count < 1) {
                goto fail;
            }

            Point lastPoint = new Point(0, list[0].GetLocation().Y);

            loop:
            keyFrame = this.backingList[i];
            Point point = keyFrame.GetLocation();
            Rect aabb = new Rect(point.X - R1, point.Y - R1, R2, R2);
            if (RectContains(ref aabb, ref p)) {
                lineHit = LineHitType.None;
                return true;
            }
            else if (IsMouseOverLine(ref p, ref lastPoint, ref point, LineHitThickness)) {
                if (i != 0) {
                    lineHit = LineHitType.Normal;
                    keyFrame = list[i - 1];
                }
                else {
                    lineHit = LineHitType.Head;
                }

                return true;
            }
            else {
                lastPoint = point;
                point = new Point(this.ActualWidth, point.Y);
                if (++i < count) {
                    goto loop;
                }

                if (IsMouseOverLine(ref p, ref lastPoint, ref point, LineHitThickness)) {
                    keyFrame = this.backingList[count - 1];
                    lineHit = LineHitType.Tail;
                    return true;
                }
            }

            fail:
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