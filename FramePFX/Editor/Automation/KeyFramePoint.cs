using System.Windows;
using System.Windows.Media;
using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Editor.Automation {
    public class KeyFramePoint {
        public readonly KeyFrameViewModel KeyFrame;
        public Rect RenderArea;
        public Point? RenderPoint;
        public KeyFramePoint prev;
        public KeyFramePoint next;

        public bool IsMoving;
        public bool IsSelected;

        protected KeyFramePoint(KeyFrameViewModel keyFrame) {
            this.KeyFrame = keyFrame;
        }

        public static KeyFramePoint ForKeyFrame(KeyFrameViewModel keyFrame) {
            return keyFrame is KeyFrameVector2ViewModel ? new KeyFramePointVec2(keyFrame) : new KeyFramePoint(keyFrame);
        }

        public void InvalidateRenderData() {
            this.RenderPoint = null;
        }

        public virtual void RenderEllipse(AutomationSequenceEditor editor, DrawingContext dc, double zoom, Rect rect) {
            Point point = AutomationSequenceEditor.GetPoint(this, zoom, rect);
            Rect area = new Rect(point.X - 3d, point.Y - 3d, 6d, 6d);
            if (AutomationSequenceEditor.RectContains(rect, area)) {
                dc.DrawEllipse(Brushes.Transparent, this.IsMoving ? editor.MouseOverPen : editor.KeyFramePen, point, 3d, 3d);
            }
        }
    }

    public class KeyFramePointVec2 : KeyFramePoint {
        public KeyFramePointVec2(KeyFrameViewModel keyFrame) : base(keyFrame) {

        }

        public override void RenderEllipse(AutomationSequenceEditor editor, DrawingContext dc, double zoom, Rect rect) {
            base.RenderEllipse(editor, dc, zoom, rect);
            Point vecPoint = AutomationSequenceEditor.GetVec2SubPoint(this, zoom, rect);
            dc.DrawEllipse(Brushes.Transparent, AutomationSequenceEditor.Vec2Pen, vecPoint, 3d, 3d);
        }
    }
}