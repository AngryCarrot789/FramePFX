using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Media;
using FramePFX.Editors.Automation;
using FramePFX.Editors.Automation.Keyframes;
using FramePFX.Editors.Automation.Params;
using FramePFX.Editors.Utils;
using FramePFX.Utils;
using SkiaSharp;
using Rect = System.Windows.Rect;

namespace FramePFX.Editors.Controls.Automation {
    public class KeyFramePoint {
        private readonly AutomationSequenceEditor editor;
        public readonly KeyFrame keyFrame;
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
        internal bool InitialPreventRemoveOnMouseUp;

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

        protected KeyFramePoint(AutomationSequenceEditor editor, KeyFrame keyFrame) {
            this.editor = editor;
            this.keyFrame = keyFrame;
        }

        public static KeyFramePoint ForKeyFrame(AutomationSequenceEditor editor, KeyFrame keyFrame) {
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
                Debugger.Break();
                // AppLogger.WriteLine("KeyFramePoint calculated a Y offset of NaN. This typically means the min/max range was negative or positive infinity, which isn't a great idea");
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

        public virtual void RenderEllipse(SKSurface surface, ref Rect drawing_area, byte opacity = 255) {
            Point point = this.GetLocation();
            const double r = AutomationSequenceEditor.EllipseRadius;
            const double rH = AutomationSequenceEditor.EllipseHitRadius, rH2 = rH * 2d;
            Rect area = new Rect(point.X - rH, point.Y - rH, rH2, rH2);
            if (AutomationSequenceEditor.RectContains(ref drawing_area, ref area)) {
                SKColor colour;
                if (this.editor.isOverrideEnabled) {
                    colour = SKColors.DarkGray;
                }
                else if (this.IsMovingPoint || this.IsMouseOverPoint) {
                    colour = SKColors.White;
                }
                else {
                    colour = SKColors.OrangeRed;
                }

                using (SKPaint paint = new SKPaint() {Color = colour.WithAlpha(opacity)}) {
                    surface.Canvas.DrawCircle(point.AsSkia(), (float) r, paint);
                }
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

        public virtual void RenderLine(SKSurface suface, KeyFramePoint target, ref Rect drawing_area, byte opacity = 255) {
            Point p1 = this.GetLocation();
            Point p2 = target.GetLocation();
            if (IsLineVisible(ref drawing_area, ref p1, ref p2)) {
                SKColor colour;
                if (this.LastLineHitType != LineHitType.Head && this.LastLineHitType != LineHitType.Tail) {
                    colour = this.editor.isOverrideEnabled ? SKColors.DarkGray : (this.LastLineHitType != LineHitType.None ? SKColors.White : SKColors.OrangeRed);
                }
                else {
                    colour = this.editor.isOverrideEnabled ? SKColors.White : SKColors.OrangeRed;
                }

                using (SKPaint paint = new SKPaint() {Color = colour.WithAlpha(opacity)}) {
                    suface.Canvas.DrawLine(p1.AsSkia(), p2.AsSkia(), paint);
                }
            }
        }

        [SwitchAutomationDataType]
        public bool SetValueForMousePoint(Point point) {
            double height = this.editor.ActualHeight;
            if (double.IsNaN(height) || height <= 0d) {
                return false;
            }

            Parameter key = this.keyFrame.sequence.Parameter;
            switch (this.keyFrame) {
                case KeyFrameFloat frame when key.Descriptor is ParameterDescriptorFloat fd:
                    frame.SetFloatValue((float) Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd);
                    break;
                case KeyFrameDouble frame when key.Descriptor is ParameterDescriptorDouble fd:
                    frame.SetDoubleValue(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd);
                    break;
                case KeyFrameLong frame when key.Descriptor is ParameterDescriptorLong fd:
                    frame.SetLongValue((long) Math.Round(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum)), fd);
                    break;
                case KeyFrameBoolean frame:
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
                // case KeyFrameVector2 frame when key.Descriptor is ParameterDescriptorVector2 fd:
                //     double x = Maths.Clamp(Maths.Map(point.X, height, 0, fd.Minimum.X, fd.Maximum.X), fd.Minimum.X, fd.Maximum.X) / this.editor.UnitZoom;
                //     double y = Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum.Y, fd.Maximum.Y), fd.Minimum.Y, fd.Maximum.Y);
                //     frame.Value = new Vector2((float) x, (float) y);
                //     break;
                default: return false;
            }

            return true;
        }

        public bool IsMouseOverLine(ref Point p, ref Point a, ref Point b, double thickness) {
            double bend = this.keyFrame.curveBend;
            double val = (b.Y - a.Y) / (b.X - a.X);
            double lineY = val * (p.X - a.X) + a.Y;
            double minX = Math.Min(a.X, b.X);
            double maxX = Math.Max(a.X, b.X);
            double rangeY = thickness * bend;
            return lineY >= p.Y - rangeY && lineY <= p.Y + rangeY && p.X >= minX && p.X <= maxX;
        }
    }
}