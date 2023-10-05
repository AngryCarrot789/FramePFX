using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using FramePFX.Automation.Keys;
using FramePFX.Automation.ViewModels.Keyframe;
using FramePFX.Editor.ViewModels;
using FramePFX.Logger;
using FramePFX.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.WPF.Editor.Automation {
    public class KeyFramePoint {
        private readonly AutomationSequenceEditor editor;
        private readonly ProjectViewModel project;
        public readonly KeyFrameViewModel keyFrame;
        private Point? renderPoint;

        private static bool HasLoggedNaN = false;

        /// <summary>
        /// The index of this key frame point in the backing list
        /// </summary>
        public int Index = -1;

        public bool IsMovingPoint;
        public bool IsPointSelected;
        public bool IsMouseOverPoint;
        public LineHitType LastLineHitType;

        public KeyFramePoint Next {
            get {
                int index = this.Index + 1;
                List<KeyFramePoint> list = this.editor.backingList;
                return index > 0 && index < list.Count ? list[index] : null;
            }
        }

        public KeyFramePoint Prev {
            get {
                int index = this.Index - 1;
                List<KeyFramePoint> list = this.editor.backingList;
                return index < list.Count && index >= 0 ? list[index] : null;
            }
        }

        protected KeyFramePoint(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame) {
            this.editor = editor;
            this.keyFrame = keyFrame;
            this.project = keyFrame.OwnerSequence.AutomationData.Owner.Project ?? throw new Exception("Could not get project from key frame");
        }

        public static KeyFramePoint ForKeyFrame(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame) {
            return new KeyFramePoint(editor, keyFrame);
        }

        public void InvalidateRenderData() {
            this.renderPoint = null;
        }

        public Point GetLocation() {
            if (this.renderPoint is Point point) {
                return point;
            }

            double height = this.editor.ActualHeight;
            double px = this.keyFrame.Frame * this.editor.UnitZoom;
            double offset_y = KeyPointUtils.GetYHelper(this.editor, this.keyFrame, height);
            if (double.IsNaN(offset_y) && !HasLoggedNaN) {
                HasLoggedNaN = true;
                AppLogger.WriteLine("KeyFramePoint calculated a Y offset of NaN. This typically means the min/max range was negative or positive infinity, which isn't a great idea");
            }

            this.renderPoint = point = new Point(px, height - offset_y);
            return point;
        }

        public virtual void RenderEllipse(DrawingContext dc, ref Rect drawing_area) {
            Point point = this.GetLocation();
            const double r = AutomationSequenceEditor.EllipseRadius;
            const double rH = AutomationSequenceEditor.EllipseHitRadius, rH2 = rH * 2d;
            Rect area = new Rect(point.X - rH, point.Y - rH, rH2, rH2);
            if (AutomationSequenceEditor.RectContains(ref drawing_area, ref area)) {
                dc.DrawEllipse(Brushes.Transparent, this.editor.KeyFrameTransparentPen, point, rH, rH);
                Pen pen;
                if (this.editor.isOverrideEnabled) {
                    pen = this.editor.KeyOverridePen;
                }
                else if (this.IsMovingPoint || this.IsMouseOverPoint) {
                    pen = this.editor.KeyFrameMouseOverPen;
                }
                else {
                    pen = this.editor.KeyFramePen;
                }

                dc.DrawEllipse(pen.Brush, pen, point, r, r);
            }
        }

        public static bool IsLineVisible(ref Rect rect, ref Point p1, ref Point p2) {
            double leftmost = Math.Min(p1.X, p2.X);
            double rightmost = Math.Max(p1.X, p2.X);
            double topmost = Math.Min(p1.Y, p2.Y);
            double bottommost = Math.Max(p1.Y, p2.Y);
            if (rightmost < rect.Left || leftmost > rect.Right || bottommost < rect.Top || topmost > rect.Bottom) {
                return false;
            }

            return true;
        }

        public virtual void RenderLine(DrawingContext dc, KeyFramePoint target, ref Rect drawing_area) {
            Point p1 = this.GetLocation();
            Point p2 = target.GetLocation();
            // long timeA = this.keyFrame.Timestamp;
            // long timeB = target.keyFrame.Timestamp;
            // if (this.geometry == null) {
            //     const int segments = 40;
            //     this.geometry = new StreamGeometry();
            //     using (StreamGeometryContext geometryContext = this.geometry.Open()) {
            //         geometryContext.BeginFigure(p1, false, false);
            //         for (int i = 1; i <= segments; i++) {
            //             float t = i / (float) segments;
            //             long currentTime = (long) Math.Round(timeA + (timeB - timeA) * t);
            //             double blend = KeyFrame.GetInterpolationMultiplier(currentTime, timeA, timeB, this.keyFrame.CurveBendAmount);
            //             double val = (blend * (p2.Y - p1.Y)) + p1.Y;
            //             Point point = new Point(p1.X + t * (p2.X - p1.X), val);
            //             geometryContext.LineTo(point, true, true);
            //         }
            //     }
            // }

            if (IsLineVisible(ref drawing_area, ref p1, ref p2)) {
                Pen pen;
                if (this.LastLineHitType != LineHitType.Head && this.LastLineHitType != LineHitType.Tail) {
                    pen = this.editor.isOverrideEnabled ? this.editor.LineOverridePen : (this.LastLineHitType != LineHitType.None ? this.editor.LineMouseOverPen : this.editor.LinePen);
                }
                else {
                    pen = this.editor.isOverrideEnabled ? this.editor.LineOverridePen : this.editor.LinePen;
                }

                // TODO: make this work i guess???
                // This renders quite well, but i don't know how to calculate the actual automation value along the bezier
                // double rangeX = Maths.Map(this.keyFrame.CurveBendAmount, -1, 1, 0, 1);
                // double rangeY = Maths.Map(this.keyFrame.CurveBendAmount, 1, -1, 0, 1);
                // Point mp = new Point(Maths.Lerp(p1.X, p2.X, rangeX), Maths.Lerp(p1.Y, p2.Y, rangeY));
                // // AppLogger.WriteLine($"Rendering with curve: {Math.Round(rangeX, 2)} & {Math.Round(rangeY, 2)} ({(int) p1.X},{(int)p1.Y} | {(int) mp.X},{(int)mp.Y} | {(int) p2.X},{(int)p2.Y})");
                // PathGeometry pathGeometry = new PathGeometry() {
                //     Figures = {new PathFigure {StartPoint = p1, Segments = {new BezierSegment(p1, mp, p2, true)}}}
                // };
                // dc.DrawGeometry(null, this.editor.LineTransparentPen, pathGeometry);
                // dc.DrawGeometry(null, pen, pathGeometry);

                // AutomationSequenceEditor.RectContains(ref drawing_area, ref p1) || AutomationSequenceEditor.RectContains(ref drawing_area, ref p2)
                //     dc.DrawGeometry(null, this.editor.LineTransparentPen, this.geometry);
                //     dc.DrawGeometry(null, pen, this.geometry);
                dc.DrawLine(this.editor.LineTransparentPen, p1, p2);
                dc.DrawLine(pen, p1, p2);
            }
        }

        public bool SetValueForMousePoint(Point point) {
            double height = this.editor.ActualHeight;
            if (double.IsNaN(height) || height <= 0d) {
                return false;
            }

            AutomationKey key = this.keyFrame.OwnerSequence.Key;
            switch (this.keyFrame) {
                case KeyFrameFloatViewModel frame when key.Descriptor is KeyDescriptorFloat fd:
                    frame.SetFloatValue((float) Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum));
                    break;
                case KeyFrameDoubleViewModel frame when key.Descriptor is KeyDescriptorDouble fd:
                    frame.SetDoubleValue(Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum));
                    break;
                case KeyFrameLongViewModel frame when key.Descriptor is KeyDescriptorLong fd:
                    frame.SetLongValue((long) Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum));
                    break;
                case KeyFrameBooleanViewModel frame:
                    double offset = (height / 100) * 30;
                    double bound_b = height - offset;
                    if (point.Y >= bound_b) {
                        frame.SetBooleanValue(false);
                    }
                    else if (point.Y < offset) {
                        frame.SetBooleanValue(true);
                    }
                    else {
                        return false;
                    }

                    return true;
                // case KeyFrameVector2ViewModel frame when key.Descriptor is KeyDescriptorVector2 fd && this is KeyFramePointVec2 v2:
                //     double x = Maths.Clamp(Maths.Map(point.X, height, 0, fd.Minimum.X, fd.Maximum.X), fd.Minimum.X, fd.Maximum.X) / this.editor.UnitZoom;
                //     double y = Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum.Y, fd.Maximum.Y), fd.Minimum.Y, fd.Maximum.Y);
                //     frame.Value = new Vector2((float) x, (float) y);
                //     break;
                default: return false;
            }

            return true;
        }

        public bool IsMouseOverLine(ref Point p, ref Point a, ref Point b, double thickness) {
            double bend = this.keyFrame.CurveBendAmount;
            double val = (b.Y - a.Y) / (b.X - a.X);
            double lineY = val * (p.X - a.X) + a.Y;
            double minX = Math.Min(a.X, b.X);
            double maxX = Math.Max(a.X, b.X);
            double rangeY = thickness * bend;
            return lineY >= p.Y - rangeY && lineY <= p.Y + rangeY && p.X >= minX && p.X <= maxX;
        }
    }
}