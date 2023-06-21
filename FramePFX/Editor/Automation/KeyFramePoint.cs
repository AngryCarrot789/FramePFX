using System.Numerics;
using System.Windows;
using System.Windows.Media;
using FramePFX.Core.Automation.Keys;
using FramePFX.Core.Automation.ViewModels.Keyframe;
using FramePFX.Core.Utils;
using Rect = System.Windows.Rect;

namespace FramePFX.Editor.Automation {
    public class KeyFramePoint {
        private readonly AutomationSequenceEditor editor;
        public readonly KeyFrameViewModel KeyFrame;
        private Point? RenderPoint;

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
                return index >= this.editor.backingList.Count ? null : this.editor.backingList[index];
            }
        }

        public KeyFramePoint Prev {
            get {
                int index = this.Index - 1;
                return index < this.editor.backingList.Count && index >= 0 ? this.editor.backingList[index] : null;
            }
        }

        protected KeyFramePoint(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame) {
            this.editor = editor;
            this.KeyFrame = keyFrame;
        }

        public static KeyFramePoint ForKeyFrame(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame) {
            return keyFrame is KeyFrameVector2ViewModel ? new KeyFramePointVec2(editor, keyFrame) : new KeyFramePoint(editor, keyFrame);
        }

        public void InvalidateRenderData() {
            this.RenderPoint = null;
        }

        public Point GetLocation() {
            if (this.RenderPoint is Point point) {
                return point;
            }

            AutomationKey key = this.KeyFrame.OwnerSequence.Key;
            double height = this.editor.ActualHeight;
            double px = this.KeyFrame.Timestamp * this.editor.UnitZoom;
            double offset_y;
            switch (this.KeyFrame) {
                case KeyFrameDoubleViewModel frame: {
                    KeyDescriptorDouble desc = (KeyDescriptorDouble) key.Descriptor;
                    offset_y = Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                    break;
                }
                case KeyFrameLongViewModel frame: {
                    KeyDescriptorLong desc = (KeyDescriptorLong) key.Descriptor;
                    offset_y = Maths.Map(frame.Value, desc.Minimum, desc.Maximum, 0, height);
                    break;
                }
                case KeyFrameBooleanViewModel frame: {
                    double offset = (height / 100) * 10;
                    offset_y = frame.Value ? (height - offset) : offset;
                    break;
                }
                case KeyFrameVector2ViewModel _: {
                    offset_y = height / 2d;
                    break;
                }
                default: return default;
            }

            this.RenderPoint = point = new Point(px, height - offset_y);
            return point;
        }

        public virtual void RenderEllipse(DrawingContext dc, ref Rect drawing_area) {
            Point point = this.GetLocation();
            const double r = AutomationSequenceEditor.EllipseRadius;
            const double rH = AutomationSequenceEditor.EllipseHitRadius, rH2 = rH * 2d;
            Rect area = new Rect(point.X - rH, point.Y - rH, rH2, rH2);
            if (AutomationSequenceEditor.RectContains(ref drawing_area, ref area)) {
                dc.DrawEllipse(Brushes.Transparent, this.editor.KeyFrameTransparentPen, point, rH, rH);
                Pen pen = (this.IsMovingPoint || this.IsMouseOverPoint) ? this.editor.KeyFrameMouseOverPen : this.editor.KeyFramePen;
                dc.DrawEllipse(Brushes.Transparent, pen, point, r, r);
            }
        }

        public bool SetValueForMousePoint(Point point) {
            double height = this.editor.ActualHeight;
            if (double.IsNaN(height) || height <= 0d) {
                return false;
            }

            AutomationKey key = this.KeyFrame.OwnerSequence.Key;
            switch (this.KeyFrame) {
                case KeyFrameDoubleViewModel frame when key.Descriptor is KeyDescriptorDouble fd:
                    frame.Value = Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum);
                    break;
                case KeyFrameLongViewModel frame when key.Descriptor is KeyDescriptorLong fd:
                    frame.Value = (long) Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum, fd.Maximum), fd.Minimum, fd.Maximum);
                    break;
                case KeyFrameBooleanViewModel frame:
                    double offset = (height / 100) * 30;
                    double bound_b = height - offset;
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
                case KeyFrameVector2ViewModel frame when key.Descriptor is KeyDescriptorVector2 fd && this is KeyFramePointVec2 v2:
                    double x = Maths.Clamp(Maths.Map(point.X, height, 0, fd.Minimum.X, fd.Maximum.X), fd.Minimum.X, fd.Maximum.X) / this.editor.UnitZoom;
                    double y = Maths.Clamp(Maths.Map(point.Y, height, 0, fd.Minimum.Y, fd.Maximum.Y), fd.Minimum.Y, fd.Maximum.Y);
                    frame.Value = new Vector2((float) x, (float) y);
                    break;
                default: return false;
            }

            return true;
        }
    }

    public class KeyFramePointVec2 : KeyFramePoint {
        public KeyFramePointVec2(AutomationSequenceEditor editor, KeyFrameViewModel keyFrame) : base(editor, keyFrame) {

        }

        public override void RenderEllipse(DrawingContext dc, ref Rect drawing_area) {
            base.RenderEllipse(dc, ref drawing_area);
            // Point vecPoint = AutomationSequenceEditor.GetVec2SubPoint(this, zoom, ref rect);
            // dc.DrawEllipse(Brushes.Transparent, AutomationSequenceEditor.Vec2Pen, vecPoint, 3d, 3d);
        }
    }
}