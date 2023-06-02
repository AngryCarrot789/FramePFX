using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace FrameControlEx.Controls {
    public class SelectionAdorder : Adorner {
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register(
                "Background",
                typeof(Brush),
                typeof(SelectionAdorder),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((SelectionAdorder) d).InvalidateResources()));

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(
                "BorderBrush",
                typeof(Brush),
                typeof(SelectionAdorder),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((SelectionAdorder) d).InvalidateResources()));

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register(
                "BorderThickness",
                typeof(Thickness),
                typeof(SelectionAdorder),
                new FrameworkPropertyMetadata(default(Thickness), FrameworkPropertyMetadataOptions.AffectsRender, (d, e) => ((SelectionAdorder) d).InvalidateResources()));

        public static readonly DependencyProperty RenderRectProperty =
            DependencyProperty.Register(
                "RenderRect",
                typeof(Rect),
                typeof(SelectionAdorder),
                new FrameworkPropertyMetadata(default(Rect), FrameworkPropertyMetadataOptions.AffectsRender));

        public Brush Background {
            get => (Brush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        public Brush BorderBrush {
            get => (Brush) this.GetValue(BorderBrushProperty);
            set => this.SetValue(BorderBrushProperty, value);
        }

        public Thickness BorderThickness {
            get => (Thickness) this.GetValue(BorderThicknessProperty);
            set => this.SetValue(BorderThicknessProperty, value);
        }

        public Rect RenderRect {
            get => (Rect) this.GetValue(RenderRectProperty);
            set => this.SetValue(RenderRectProperty, value);
        }

        private Pen penL;
        private Pen penT;
        private Pen penR;
        private Pen penB;

        public SelectionAdorder(UIElement adornedElement) : base(adornedElement) {
            this.IsHitTestVisible = false;
            this.Opacity = 0.5d;
        }

        private void InvalidateResources() {
            this.penL = null;
            this.penT = null;
            this.penR = null;
            this.penB = null;
        }

        private static Pen NewPen(Brush brush, double thickness) {
            Pen pen = new Pen(brush, thickness);
            if (brush.IsFrozen)
                pen.Freeze();
            return pen;
        }

        public void SetRect(double x, double y, double w, double h) {
            // this.Margin = new Thickness(x, y, 0, 0);
            // this.Width = w;
            // this.Height = h;
            // this.InvalidateVisual();
            this.RenderRect = new Rect(x, y, w, h);
        }

        protected override void OnRender(DrawingContext dc) {
            Rect rect = this.RenderRect;
            dc.PushClip(new RectangleGeometry(new Rect(new Point(), this.AdornedElement.RenderSize)));
            dc.PushTransform(new TranslateTransform(rect.X, rect.Y));
            this.DoRender(dc);
            dc.Pop();
            dc.Pop();
        }

        private void DoRender(DrawingContext dc) {
            Rect rect = this.RenderRect;
            Thickness border = this.BorderThickness;
            Brush borderBrush;
            if ((border.Left > 0d || border.Top > 0d || border.Right > 0d || border.Bottom > 0d) && (borderBrush = this.BorderBrush) != null) {
                bool isUniform = Math.Abs(border.Left - border.Top) < 0.01d && Math.Abs(border.Left - border.Right) < 0.01d && Math.Abs(border.Left - border.Bottom) < 0.01d;
                Pen penMain = this.penL ?? (this.penL = NewPen(borderBrush, border.Left));
                if (isUniform) {
                    double thicc = penMain.Thickness * 0.5d;
                    Rect uniform = new Rect(new Point(thicc, thicc), new Point(rect.Width - thicc, rect.Height - thicc));
                    dc.DrawRectangle(null, penMain, uniform);
                }
                else {
                    if (border.Left > 0) {
                        double thicc = penMain.Thickness * 0.5;
                        dc.DrawLine(penMain, new Point(thicc, 0.0), new Point(thicc, rect.Height));
                    }

                    if (border.Top > 0) {
                        Pen pen = this.penT ?? (this.penT = NewPen(borderBrush, border.Top));
                        double thicc = pen.Thickness * 0.5;
                        Point point0 = new Point(0.0, thicc);
                        Point point1 = new Point(rect.Width, thicc);
                        dc.DrawLine(pen, point0, point1);
                    }

                    if (border.Right > 0) {
                        Pen pen = this.penR ?? (this.penR = NewPen(borderBrush, border.Right));
                        double thicc = pen.Thickness * 0.5;
                        Point point0 = new Point(rect.Width - thicc, 0.0);
                        Point point1 = new Point(rect.Width - thicc, rect.Height);
                        dc.DrawLine(pen, point0, point1);
                        rect.Width -= pen.Thickness;
                    }

                    if (border.Bottom > 0) {
                        Pen pen = this.penB ?? (this.penB = NewPen(borderBrush, border.Bottom));
                        double thicc = pen.Thickness * 0.5;
                        Point point0 = new Point(0.0, rect.Height - thicc);
                        Point point1 = new Point(rect.Width, rect.Height - thicc);
                        dc.DrawLine(pen, point0, point1);
                    }
                }
            }

            Brush background = this.Background;
            if (background != null) {
                double x = rect.Width - border.Right;
                double y = rect.Height - border.Bottom;
                Point p1 = new Point(border.Left, border.Top);
                Point p2 = new Point(x, y);

                if (!(p2.X <= p1.X) && !(p2.Y <= p1.Y)) {
                    dc.DrawRectangle(background, null, new Rect(p1, p2));
                }
            }
        }
    }
}