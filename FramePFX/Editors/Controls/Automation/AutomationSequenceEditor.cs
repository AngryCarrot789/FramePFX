//
// Copyright (c) 2023-2024 REghZy
//
// This file is part of FramePFX.
//
// FramePFX is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
//
// FramePFX is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with FramePFX. If not, see <https://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Utils;
using FramePFX.Utils;
using FramePFX.Utils.Visuals;
using SkiaSharp;

namespace FramePFX.Editors.Controls.Automation {
    public class AutomationSequenceEditor : FrameworkElement {
        public const double EllipseRadius = 2.5d;
        public const double EllipseThickness = 1d;
        public const double EllipseHitRadius = 12d;
        public const double LineThickness = 2d;
        public const double LineHitThickness = 12d;

        public const double MaximumFloatingPointRange = 10000;

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
                typeof(AutomationSequence),
                typeof(AutomationSequenceEditor),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    (o, e) => ((AutomationSequenceEditor) o).OnSequencePropertyChanged((AutomationSequence) e.OldValue, (AutomationSequence) e.NewValue)));

        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(1d, (o, e) => ((AutomationSequenceEditor) o).InvalidKeyFrameDataAndRender()));

        public static readonly DependencyProperty FrameDurationProperty =
            DependencyProperty.Register(
                "FrameDuration",
                typeof(long),
                typeof(AutomationSequenceEditor),
                new PropertyMetadata(10000L));

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

        public AutomationSequence Sequence {
            get => (AutomationSequence) this.GetValue(SequenceProperty);
            set => this.SetValue(SequenceProperty, value);
        }

        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public long FrameDuration {
            get => (long) this.GetValue(FrameDurationProperty);
            set => this.SetValue(FrameDurationProperty, value);
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
        private Pen empyListLinePen;

        internal readonly List<KeyFramePoint> backingList;
        internal readonly Dictionary<KeyFrame, KeyFramePoint> vmToPoint;
        private ScrollViewer scroller;

        private KeyFramePoint captured;
        private Point lastMousePoint;
        private Point originMousePoint;
        private Point curveMousePoint;
        private bool isCaptureInitialised;
        private KeyFramePoint lastMouseOver;
        private LineHitType captureLineHit;
        private DragMode? dragMode;

        // the range of the automation parameter is too large to be rendered accurately
        // (e.g. -infinity to +infinity or -100000 to +100000)
        // allowing this to be user-adjustable via the mouse would be ineffective as a single
        // pixel would equal a float/double value of like 100,000 so :/
        public bool IsValueRangeHuge;

        private static readonly Brush TransparentBrush = Brushes.Transparent; // Brushes.Yellow

        internal Pen KeyOverridePen => this.keyOverridePen ?? (this.keyOverridePen = new Pen(this.OverrideModeBrush ?? Brushes.DarkGray, EllipseThickness));
        internal Pen KeyFramePen => this.keyFramePen ?? (this.keyFramePen = new Pen(this.KeyFrameBrush ?? Brushes.OrangeRed, EllipseThickness));
        internal Pen KeyFrameMouseOverPen => this.mouseOverPen ?? (this.mouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, EllipseThickness));
        internal Pen KeyFrameTransparentPen => this.keyFrameTransparentPen ?? (this.keyFrameTransparentPen = new Pen(TransparentBrush, EllipseHitRadius));
        internal Pen LineOverridePen => this.lineOverridePen ?? (this.lineOverridePen = new Pen(this.OverrideModeBrush ?? Brushes.DarkGray, LineThickness));
        internal Pen LinePen => this.curvePen ?? (this.curvePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness));
        internal Pen LineMouseOverPen => this.lineMouseOverPen ?? (this.lineMouseOverPen = new Pen(this.MouseOverBrush ?? Brushes.White, LineThickness));
        internal Pen LineTransparentPen => this.transparentPenLine ?? (this.transparentPenLine = new Pen(TransparentBrush, LineHitThickness));
        internal Pen OverrideModeValueLinePen => this.overrideModeValueLinePen ?? (this.overrideModeValueLinePen = new Pen(this.CurveBrush ?? Brushes.DarkGray, LineThickness) {DashStyle = new DashStyle(new List<double>() {2d, 2d}, 0d)});
        internal Pen EmpyListLinePen => this.empyListLinePen ?? (this.empyListLinePen = new Pen(this.CurveBrush ?? Brushes.OrangeRed, LineThickness) {DashStyle = new DashStyle(new List<double>() {2d, 2d}, 0d)});

        public AutomationSequenceEditor() {
            this.backingList = new List<KeyFramePoint>();
            this.vmToPoint = new Dictionary<KeyFrame, KeyFramePoint>();
            this.Loaded += this.OnLoaded;
            this.IsHitTestVisible = true;
        }

        public int GetPointIndexByKeyFrame(KeyFrame keyFrame) {
            return this.vmToPoint.TryGetValue(keyFrame, out KeyFramePoint point) ? point.Index : -1;
        }

        public KeyFramePoint GetPointByKeyFrame(KeyFrame keyFrame) {
            int index = this.GetPointIndexByKeyFrame(keyFrame);
            return index != -1 ? this.backingList[index] : null;
        }

        public bool TryGetPointByKeyFrame(KeyFrame keyFrame, out KeyFramePoint point) {
            return (point = this.GetPointByKeyFrame(keyFrame)) != null;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.scroller = VisualTreeUtils.GetParent<ScrollViewer>(this);
            if (this.scroller == null) {
                return;
            }

            this.scroller.SizeChanged += this.OnScrollerOnSizeChanged;
            this.scroller.ScrollChanged += this.OnScrollerOnScrollChanged;
        }

        #region Key Frame Creation/Deletion

        public KeyFrame CreateKeyFrameAt(AutomationSequence sequence, Point point, bool capturePoint) {
            return this.CreateKeyFrameAt(sequence, point, ref capturePoint);
        }

        public KeyFrame CreateKeyFrameAt(AutomationSequence sequence, Point point, ref bool capturePoint) {
            long timestamp = (long) Math.Round(point.X / this.UnitZoom);
            KeyFrame keyFrame = KeyFrame.CreateInstance(sequence, timestamp);
            sequence.AddKeyFrame(keyFrame);
            if (this.TryGetPointByKeyFrame(keyFrame, out KeyFramePoint keyFramePoint)) {
                if (!this.IsValueRangeHuge) {
                    keyFramePoint.SetValueForMousePoint(point);
                }

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

        #endregion

        #region Key point creation/deletion

        private void CreatePoints(int index, List<KeyFrame> keyFrames) {
            int i, lc = this.backingList.Count, kc = keyFrames.Count;
            for (i = index; i < lc; i++) {
                this.backingList[i].Index += kc;
            }

            for (i = 0; i < kc; i++) {
                KeyFrame keyFrame = keyFrames[i];
                if (this.vmToPoint.ContainsKey(keyFrame)) {
                    throw new Exception("Point was already added");
                }

                KeyFramePoint point = KeyFramePoint.ForKeyFrame(this, keyFrame);
                point.Index = index + i;
                this.backingList.Insert(point.Index, point);
                this.vmToPoint[keyFrame] = point;
                keyFrame.FrameChanged += this.OnKeyFramePositionChanged;
                keyFrame.ValueChanged += this.OnKeyFrameValueChanged;
            }
        }

        private void OnKeyFrameValueChanged(KeyFrame keyframe) {
            this.InvalidateRenderForKeyFrame(keyframe);
        }

        private void OnKeyFramePositionChanged(KeyFrame keyframe, long oldframe, long newframe) {
            this.InvalidateRenderForKeyFrame(keyframe);
        }

        // Do not call unless the view model has been updated accordingly!
        // This is only invoked via the collection changed handlers
        private void RemovePointAt(int index) {
            KeyFramePoint point = this.backingList[index];
            if (point == this.captured) {
                this.ClearCapture();
            }

            for (int i = index + 1, c = this.backingList.Count; i < c; i++) {
                this.backingList[i].Index--;
            }

            point.keyFrame.FrameChanged -= this.OnKeyFramePositionChanged;
            point.keyFrame.ValueChanged -= this.OnKeyFrameValueChanged;
            this.backingList.RemoveAt(index);
            if (!this.vmToPoint.Remove(point.keyFrame)) {
                throw new Exception("Point was not stored in the backing map");
            }
        }

        private void RemovePoints(int index, List<KeyFrame> keyFrames, bool detatchPropertyChangedEvent = true) {
            int count = keyFrames.Count;
            if (keyFrames.Count > 1) {
                if (index == -1) {
                    // slow double loop
                    foreach (KeyFrame toRemove in keyFrames) {
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
                            point.keyFrame.FrameChanged -= this.OnKeyFramePositionChanged;
                            point.keyFrame.ValueChanged -= this.OnKeyFrameValueChanged;
                        }

                        if (ReferenceEquals(point, this.captured)) {
                            this.ClearCapture();
                        }

                        point.Index -= count;
                        if (!this.vmToPoint.Remove(point.keyFrame)) {
                            // will corrupt entire sequence editor but that's my fault if it happens
                            throw new Exception("Point was not stored in the backing map");
                        }
                    }

                    this.backingList.RemoveRange(index, count);
                }
            }
            else if (keyFrames.Count == 1) {
                KeyFrame toRemove = keyFrames[0];
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
                    keyFrame.keyFrame.FrameChanged -= this.OnKeyFramePositionChanged;
                    keyFrame.keyFrame.ValueChanged -= this.OnKeyFrameValueChanged;
                }
            }

            this.backingList.Clear();
            this.vmToPoint.Clear();
            if (this.captured != null) {
                this.ClearCapture();
            }
        }

        private void GenerateBackingList(AutomationSequence sequence) {
            this.ClearKeyFrameList();

            IList<KeyFrame> list = sequence.KeyFrames;
            for (int i = 0, c = list.Count; i < c; i++) {
                KeyFrame keyFrame = sequence.KeyFrames[i];
                keyFrame.FrameChanged += this.OnKeyFramePositionChanged;
                keyFrame.ValueChanged += this.OnKeyFrameValueChanged;
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
        public bool isOverrideEnabled;

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

        protected virtual void OnSequencePropertyChanged(AutomationSequence oldValue, AutomationSequence newValue) {
            if (oldValue != null) {
                oldValue.OverrideStateChanged -= this.OnIsOverridedEnabledChanged;
                oldValue.DefaultKeyFrame.FrameChanged -= this.OnKeyFramePositionChanged;
                oldValue.DefaultKeyFrame.ValueChanged -= this.OnKeyFrameValueChanged;
                oldValue.KeyFrameAdded -= this.OnKeyFrameAdded;
                oldValue.KeyFrameRemoved -= this.OnKeyFrameRemoved;

                // ((INotifyCollectionChanged) oldValue.KeyFrames).CollectionChanged -= this.OnCollectionChanged;
            }

            this.ClearKeyFrameList();
            if (newValue != null) {
                newValue.OverrideStateChanged += this.OnIsOverridedEnabledChanged;
                newValue.DefaultKeyFrame.FrameChanged += this.OnKeyFramePositionChanged;
                newValue.DefaultKeyFrame.ValueChanged += this.OnKeyFrameValueChanged;
                newValue.KeyFrameAdded += this.OnKeyFrameAdded;
                newValue.KeyFrameRemoved += this.OnKeyFrameRemoved;
                // ((INotifyCollectionChanged) newValue.KeyFrames).CollectionChanged += this.OnCollectionChanged;
                this.SetupRenderingInfo(newValue);
                this.GenerateBackingList(newValue);
                this.isOverrideEnabled = newValue.IsOverrideEnabled;
            }
            else {
                this.isOverrideEnabled = false;
            }
        }

        private void OnKeyFrameAdded(AutomationSequence sequence, KeyFrame keyframe, int index) {
            this.CreatePoints(index, new List<KeyFrame>() {keyframe});
            this.InvalidKeyFrameDataAndRender();
        }

        private void OnKeyFrameRemoved(AutomationSequence sequence, KeyFrame keyframe, int index) {
            this.RemovePointAt(index);
            this.InvalidKeyFrameDataAndRender();
        }

        // this.GenerateBackingList((AutomationSequence) sender);

        private void OnIsOverridedEnabledChanged(AutomationSequence sequence) {
            this.isOverrideEnabled = sequence.IsOverrideEnabled;
            this.InvalidateVisual();
        }

        private static bool IsValueTooLarge(float min, float max) {
            return float.IsInfinity(min) || float.IsInfinity(max) || Maths.GetRange(min, max) > MaximumFloatingPointRange;
        }

        private static bool IsValueTooLarge(double min, double max) {
            return double.IsInfinity(min) || double.IsInfinity(max) || Maths.GetRange(min, max) > MaximumFloatingPointRange;
        }

        private static bool IsValueTooLarge(long min, long max) {
            return Maths.GetRange(min, max) > MaximumFloatingPointRange;
        }

        [SwitchAutomationDataType]
        private void SetupRenderingInfo(AutomationSequence sequence) {
            this.IsValueRangeHuge = false;
            switch (sequence.Parameter.DataType) {
                case AutomationDataType.Float: {
                    ParameterDescriptorFloat desc = (ParameterDescriptorFloat) sequence.Parameter.Descriptor;
                    this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                    break;
                }
                case AutomationDataType.Double: {
                    ParameterDescriptorDouble desc = (ParameterDescriptorDouble) sequence.Parameter.Descriptor;
                    this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                    break;
                }
                case AutomationDataType.Long: {
                    ParameterDescriptorLong desc = (ParameterDescriptorLong) sequence.Parameter.Descriptor;
                    this.IsValueRangeHuge = IsValueTooLarge(desc.Minimum, desc.Maximum);
                    break;
                }
                case AutomationDataType.Boolean: break;
                case AutomationDataType.Vector2: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }


        private void InvalidateRenderForKeyFrame(KeyFrame keyFrame) {
            AutomationSequence seq = this.Sequence;
            if (seq != null && !ReferenceEquals(keyFrame, seq.DefaultKeyFrame)) {
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

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnPreviewMouseLeftButtonDown(e);
            if (!(this.Sequence is AutomationSequence sequence)) {
                return;
            }

            Point mPos = e.GetPosition(this);
            this.lastMousePoint = mPos;
            if (this.backingList.Count < 1) {
                if (this.isCaptureInitialised) {
                    this.isCaptureInitialised = false;
                }

                KeyFrame kf = this.CreateKeyFrameAt(sequence, mPos, true);
                KeyFramePoint kfp = this.GetPointByKeyFrame(kf);
                kfp.InitialPreventRemoveOnMouseUp = true;
                this.SetPointCaptured(kfp, true, LineHitType.None);
            }
            else {
                if (this.GetIntersection(ref mPos, out KeyFramePoint hitKey, out LineHitType lineHit)) {
                    if (this.captured != null) {
                        this.ClearCapture(lineHit != LineHitType.None);
                    }

                    if (lineHit != LineHitType.None) {
                        if (this.isCaptureInitialised) {
                            this.isCaptureInitialised = false;
                        }

                        this.CreateKeyFrameAt(sequence, mPos, true);
                    }
                    else {
                        this.SetPointCaptured(hitKey, true, lineHit);
                        if (hitKey.InitialPreventRemoveOnMouseUp) {
                            hitKey.InitialPreventRemoveOnMouseUp = false;
                            this.isCaptureInitialised = false;
                        }
                    }

                    this.InvalidateVisual();
                    e.Handled = true;
                }
                else {
                    if (this.isCaptureInitialised) {
                        this.isCaptureInitialised = false;
                    }

                    this.CreateKeyFrameAt(sequence, mPos, true);
                }
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            if (this.captured != null) {
                if (this.isCaptureInitialised && this.captureLineHit == LineHitType.None && this.Sequence is AutomationSequence sequence) {
                    KeyFrame kf = this.vmToPoint.TryGetValue(this.captured.keyFrame, out KeyFramePoint p) ? p.keyFrame : null;
                    if (kf == null) {
                        throw new Exception("Captured key frame not found in the backing list?");
                    }
                    else if (p.InitialPreventRemoveOnMouseUp) {
                        p.InitialPreventRemoveOnMouseUp = false;
                    }
                    else {
                        sequence.RemoveKeyFrame(kf, out _);
                        this.ClearCapture();
                    }
                }
            }
            else {
                Point pos = e.GetPosition(this);
                if (this.GetIntersection(ref pos, out KeyFramePoint kf, out LineHitType hitType) && hitType == LineHitType.None) {
                    kf.keyFrame.sequence.RemoveKeyFrame(kf.keyFrame, out _);
                }
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
            this.UpdateMouseOver(e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            if (this.lastMouseOver != null) {
                this.lastMouseOver.LastLineHitType = LineHitType.None;
                this.lastMouseOver.IsMouseOverPoint = false;
                this.lastMouseOver = null;
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

            long min = prev?.keyFrame.Frame ?? 0;
            long max = next?.keyFrame.Frame ?? this.FrameDuration;

            if (this.isCaptureInitialised) {
                this.lastMousePoint = mPos;
                this.originMousePoint = mPos;
                this.curveMousePoint = mPos;
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
                        else {
                            // return; // no mouse movement???
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
                // double diff = mPos.Y - this.curveMousePoint.Y;
                // double mapped = Maths.Map(60d - diff, -60d, 60d, -1d, 1d);
                // double curve = Maths.Clamp(mapped, -1d, 1d);
                // this.captured.keyFrame.CurveBendAmount = curve;
                // this.captured.InvalidateRenderData();
            }
            else {
                if (mode == DragMode.FullKeyFrame || mode == DragMode.HorizontalKeyFrame) {
                    long newTime = Math.Max(0, (long) Math.Round(mPos.X / this.UnitZoom));
                    long oldTime = this.captured.keyFrame.Frame;
                    if ((oldTime + newTime) < 0) {
                        newTime = -oldTime;
                    }

                    this.captured.keyFrame.Frame = Maths.Clamp(newTime, min, max);
                }

                if (!this.IsValueRangeHuge && (mode == DragMode.FullKeyFrame || mode == DragMode.VerticalKeyFrame)) {
                    this.captured.SetValueForMousePoint(mPos);
                }
            }

            this.UpdateMouseOver(mPos);
            this.lastMousePoint = mPos;
        }

        #endregion

        #region Rendering

        private WriteableBitmap bitmapImg;

        protected override void OnRender(DrawingContext dc) {
            Rect visible = UIUtils.GetVisibleRect(this.scroller, this);
            if (visible.Width < 1.0 || visible.Height < 1.0) {
                return;
            }

            List<KeyFramePoint> list = this.backingList;

            // Size size = this.RenderSize;
            // if (this.bitmapImg == null || this.bitmapImg.PixelWidth != (int) size.Width || this.bitmapImg.PixelHeight != (int) size.Height) {
            //     this.bitmapImg = new WriteableBitmap((int) size.Width, (int) size.Height, 96, 96, PixelFormats.Bgra32, null);
            // }
            //
            // this.bitmapImg.Lock();
            // IntPtr handle = this.bitmapImg.BackBuffer;
            //
            // // Rect visible = Getvisible(this.scroller, this);
            // using (SKSurface surface = SKSurface.Create(new SKImageInfo((int) size.Width, (int) size.Height, SKColorType.Bgra8888), handle)) {
            //     surface.Canvas.Clear(SKColors.Transparent);
            //     int end = list.Count - 1;
            //     if (end < 0) {
            //         AutomationSequence seq = this.Sequence;
            //         if (seq != null) {
            //             double y = this.ActualHeight / 2.0;
            //             if (this.IsMouseOver) {
            //                 Point mPos = new Point(Mouse.GetPosition(this).X, y);
            //                 using (SKPaint paint = new SKPaint() {Color = SKColors.OrangeRed}) {
            //                     surface.Canvas.DrawLine(0, (float) y, (float) size.Width, (float) y, paint);
            //                 }
            //
            //                 using (SKPaint paint = new SKPaint() {Color = SKColors.WhiteSmoke}) {
            //                     surface.Canvas.DrawCircle(new SKPoint((float) mPos.X, (float) mPos.Y), (float) EllipseRadius, paint);
            //                 }
            //             }
            //             else {
            //                 using (SKPaint paint = new SKPaint() {Color = SKColors.OrangeRed.WithAlpha(127)}) {
            //                     surface.Canvas.DrawLine(0, (float) y, (float) size.Width, (float) y, paint);
            //                 }
            //             }
            //         }
            //     }
            //     else {
            //         byte opacity = (byte) (this.isOverrideEnabled ? 127 : 255);
            //         KeyFramePoint first = list[0], prev = first;
            //         this.DrawFirstKeyFrameLine(surface, first, ref visible, opacity);
            //         if (end == 0) {
            //             this.DrawLastKeyFrameLine(surface, first, ref visible, opacity);
            //             first.RenderEllipse(surface, ref visible, opacity);
            //         }
            //         else {
            //             for (int i = 1; i < end; i++) {
            //                 KeyFramePoint keyFrame = list[i];
            //                 DrawKeyFramesAndLine(surface, prev, keyFrame, ref visible, opacity);
            //                 prev = keyFrame;
            //             }
            //
            //             this.DrawLastKeyFrameLine(surface, list[end], ref visible, opacity);
            //             DrawKeyFramesAndLine(surface, prev, list[end], ref visible, opacity);
            //         }
            //
            //         if (this.isOverrideEnabled) {
            //             AutomationSequence seq = this.Sequence;
            //             if (seq != null) {
            //                 double y = this.ActualHeight - KeyPointUtils.GetYHelper(this, seq.DefaultKeyFrame, this.ActualHeight);
            //                 dc.DrawLine(this.OverrideModeValueLinePen, new Point(0, y), new Point(size.Width, y));
            //             }
            //         }
            //     }
            //
            //     this.bitmapImg.AddDirtyRect(new Int32Rect(0, 0, (int) size.Width, (int) size.Height));
            //     this.bitmapImg.Unlock();
            //     dc.DrawImage(this.bitmapImg, new Rect(0, 0, size.Width, size.Height));
            //     // dc.DrawImage(this.bitmapImg, visible);
            // }

            int end = list.Count - 1;
            if (end < 0) {
                AutomationSequence seq = this.Sequence;
                if (seq != null) {
                    double y = this.ActualHeight / 2.0;
                    dc.DrawLine(this.LineTransparentPen, new Point(0, y), new Point(visible.Right, y));
                    if (this.IsMouseOver) {
                        Point mPos = new Point(Mouse.GetPosition(this).X, y);
                        dc.DrawLine(this.EmpyListLinePen, new Point(0, y), new Point(visible.Right, y));
                        dc.DrawEllipse(this.MouseOverBrush, this.KeyFrameMouseOverPen, mPos, EllipseRadius, EllipseRadius);
                    }
                    else {
                        dc.PushOpacity(0.5d);
                        dc.DrawLine(this.EmpyListLinePen, new Point(0, y), new Point(visible.Right, y));
                        dc.Pop();
                    }
                }
            }
            else {
                dc.PushClip(new RectangleGeometry(visible));
                if (this.isOverrideEnabled) {
                    dc.PushOpacity(0.5d);
                }

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
                    AutomationSequence seq = this.Sequence;
                    if (seq != null) {
                        double y = this.ActualHeight - KeyPointUtils.GetYHelper(this, seq.DefaultKeyFrame, this.ActualHeight);
                        dc.DrawLine(this.OverrideModeValueLinePen, new Point(0, y), new Point(visible.Right, y));
                    }

                    dc.Pop();
                }

                dc.Pop();
            }
        }

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

        private static void DrawKeyFramesAndLine(SKSurface surface, KeyFramePoint a, KeyFramePoint b, ref Rect rect, byte opacity) {
            a.RenderLine(surface, b, ref rect, opacity);
            a.RenderEllipse(surface, ref rect, opacity);
            b.RenderEllipse(surface, ref rect, opacity);
        }

        public static bool IsLineVisibleInRect(ref Rect visibleArea, ref Point p1, ref Point p2, double thickness) {
            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            double length = (p2 - p1).Length;

            Vector offset = new Vector(thickness / 2.0 * Math.Sin(angle), thickness / 2.0 * Math.Cos(angle));

            Point[] linePoints = new Point[] {
                new Point(p1.X - offset.X, p1.Y + offset.Y),
                new Point(p2.X - offset.X, p2.Y + offset.Y),
                new Point(p2.X + offset.X, p2.Y - offset.Y),
                new Point(p1.X + offset.X, p1.Y - offset.Y)
            };

            // Create a rotated rectangle that bounds the line with thickness
            Rect lineRect = new Rect(linePoints[0], linePoints[2]);

            // Check if the rotated rectangle intersects with the visible area
            return visibleArea.IntersectsWith(lineRect);
        }

        // draw a horizontal line at the key's Y pos
        private void DrawFirstKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point p2 = key.GetLocation();
            Point p1 = new Point(0, p2.Y);
            if (IsLineVisibleInRect(ref rect, ref p1, ref p2, LineThickness)) {
                dc.DrawLine(this.LineTransparentPen, p1, p2);
                dc.DrawLine(this.isOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Head ? this.LineMouseOverPen : this.LinePen), p1, p2);
            }
        }

        // draw a horizontal line at the key's Y pos
        private void DrawLastKeyFrameLine(DrawingContext dc, KeyFramePoint key, ref Rect rect) {
            Point a = key.GetLocation();
            Point b = new Point(rect.Right, a.Y);
            if (RectContains(ref rect, ref a) || RectContains(ref rect, ref b)) {
                dc.DrawLine(this.LineTransparentPen, a, b);
                dc.DrawLine(this.isOverrideEnabled ? this.LineOverridePen : (key.LastLineHitType == LineHitType.Tail ? this.LineMouseOverPen : this.LinePen), a, b);
            }
        }

        private void DrawFirstKeyFrameLine(SKSurface surface, KeyFramePoint key, ref Rect rect, byte opacity) {
            Point p2 = key.GetLocation();
            Point p1 = new Point(0, p2.Y);
            if (IsLineVisibleInRect(ref rect, ref p1, ref p2, LineThickness)) {
                SKColor colour = this.isOverrideEnabled ? SKColors.DarkGray : (key.LastLineHitType == LineHitType.Head ? SKColors.White : SKColors.OrangeRed);
                using (SKPaint paint = new SKPaint() {Color = colour.WithAlpha(opacity)}) {
                    surface.Canvas.DrawLine(p1.AsSkia(), p2.AsSkia(), paint);
                }
            }
        }

        private void DrawLastKeyFrameLine(SKSurface surface, KeyFramePoint key, ref Rect rect, byte opacity) {
            Point a = key.GetLocation();
            Point b = new Point(rect.Right, a.Y);
            if (RectContains(ref rect, ref a) || RectContains(ref rect, ref b)) {
                SKColor colour = this.isOverrideEnabled ? SKColors.DarkGray : (key.LastLineHitType == LineHitType.Tail ? SKColors.White : SKColors.OrangeRed);
                using (SKPaint paint = new SKPaint() {Color = colour.WithAlpha(opacity)}) {
                    surface.Canvas.DrawLine(a.AsSkia(), b.AsSkia(), paint);
                }
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

        // Function to calculate the Euclidean distance between two points
        private static double Distance(Point p1, Point p2) {
            double dx = p2.X - p1.X;
            double dy = p2.Y - p1.Y;
            return Math.Sqrt(dx * dx + dy * dy);
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

        public bool GetIntersection(ref Point p, out KeyFramePoint keyFrame, out LineHitType lineHit) {
            List<KeyFramePoint> list = this.backingList;
            int count = list.Count, i = 0;
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

        private const double R1 = EllipseHitRadius, R2 = R1 * 2d;

        #endregion

        #region standard property change handlers

        protected virtual void OnOverrideBrushPropertyChanged(Brush oldValue, Brush newValue) => this.keyOverridePen = null;
        protected virtual void OnKeyFrameBrushPropertyChanged(Brush oldValue, Brush newValue) => this.keyFramePen = null;
        protected virtual void OnCurveBrushPropertyChanged(Brush oldValue, Brush newValue) => this.curvePen = null;
        protected virtual void OnMouseOverBrushPropertyChanged(Brush oldValue, Brush newValue) => this.mouseOverPen = null;

        #endregion
    }
}