using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    // https://www.codeproject.com/Articles/209560/ListBox-drag-selection
    public sealed class SelectionAdorner : Adorner {
        public static readonly DependencyProperty BackgroundProperty = Border.BackgroundProperty.AddOwner(typeof(SelectionAdorner));
        public static readonly DependencyProperty BorderBrushProperty = Border.BorderBrushProperty.AddOwner(typeof(SelectionAdorner));

        public Brush Background {
            get => (Brush) this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
        }

        public Brush BorderBrush {
            get => (Brush) this.GetValue(BorderBrushProperty);
            set => this.SetValue(BorderBrushProperty, value);
        }

        // Gets or sets the area of the selection rectangle.
        public Rect SelectionArea { get; set; }

        // Initializes a new instance of the SelectionAdorner class.
        public SelectionAdorner(UIElement parent) : base(parent) {
            // Make sure the mouse doesn't see us.
            this.IsHitTestVisible = false;

            // We only draw a rectangle when we're enabled.
            this.IsEnabledChanged += (sender, args) => ((SelectionAdorner) sender).InvalidateVisual();
        }

        // Participates in rendering operations that are directed by the layout system.
        protected override void OnRender(DrawingContext dc) {
            base.OnRender(dc);
            if (this.IsEnabled) {
                // Make the lines snap to pixels (add half the pen width [0.5])
                double[] x = {this.SelectionArea.Left + 0.5, this.SelectionArea.Right + 0.5};
                double[] y = {this.SelectionArea.Top + 0.5, this.SelectionArea.Bottom + 0.5};
                dc.PushGuidelineSet(new GuidelineSet(x, y));
                dc.DrawRectangle(this.Background, new Pen(SystemColors.HighlightBrush, 1.0), this.SelectionArea);
            }
        }
    }
}