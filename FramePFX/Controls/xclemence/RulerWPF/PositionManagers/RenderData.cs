using System.Windows;

namespace FramePFX.Controls.xclemence.RulerWPF.PositionManagers
{
    public readonly struct RenderData
    {
        public readonly Rect DrawingArea;
        public readonly Size AvailableSize;
        public readonly double PixelStep;
        public readonly double ValueStep;

        public RenderData(Rect drawingArea, Size availableSize, double pixelStep, double valueStep)
        {
            this.DrawingArea = drawingArea;
            this.AvailableSize = availableSize;
            this.PixelStep = pixelStep;
            this.ValueStep = valueStep;
        }
    }
}