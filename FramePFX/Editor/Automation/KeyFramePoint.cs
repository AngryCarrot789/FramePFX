using System.CodeDom;
using System.Windows;
using FramePFX.Core.Automation.ViewModels.Keyframe;

namespace FramePFX.Editor.Automation {
    public class KeyFramePoint {
        public readonly KeyFrameViewModel KeyFrame;
        public Rect RenderArea;
        public Point? RenderPoint;
        public KeyFramePoint prev;
        public KeyFramePoint next;

        public bool IsCaptured;

        public KeyFramePoint(KeyFrameViewModel keyFrame) {
            this.KeyFrame = keyFrame;
        }

        public void InvalidateVisual() {
            this.RenderPoint = null;
        }
    }
}