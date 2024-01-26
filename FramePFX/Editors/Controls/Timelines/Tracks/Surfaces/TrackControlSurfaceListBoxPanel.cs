using System;
using System.Windows;
using System.Windows.Controls;

namespace FramePFX.Editors.Controls.Timelines.Tracks.Surfaces {
    /// <summary>
    /// A panel for a <see cref="TrackControlSurfaceListBox"/>, which is used to measure
    /// and arrange the collection of <see cref="TrackControlSurfaceListBoxItem"/>
    /// </summary>
    public class TrackControlSurfaceListBoxPanel : Panel {
        public TrackControlSurfaceListBoxPanel() {
        }

        protected override Size MeasureOverride(Size availableSize) {
            Size size = new Size();
            UIElementCollection items = this.InternalChildren;
            int count = items.Count;
            for (int i = 0; i < count; i++) {
                UIElement element = this.InternalChildren[i];
                element.Measure(availableSize);
                Size dsize = element.DesiredSize;
                size.Width = Math.Max(size.Width, dsize.Width);
                size.Height += dsize.Height;
            }

            if (count > 1) {
                size.Height += (count - 1);
            }

            return size;
        }

        protected override Size ArrangeOverride(Size finalSize) {
            UIElementCollection items = this.InternalChildren;
            int count = items.Count;
            Rect rect = new Rect(finalSize);
            double num = 0.0;
            for (int i = 0; i < count; ++i) {
                UIElement element = items[i];
                if (element != null) {
                    rect.Y += num;
                    num = element.DesiredSize.Height;
                    rect.Height = num;
                    rect.Width = Math.Max(finalSize.Width, element.DesiredSize.Width);

                    element.Arrange(rect);
                    rect.Y += 1;
                }
            }

            return finalSize;

            // double offsetY = 0d;
            // for (int i = 0; i < count; i++) {
            //     UIElement element = this.InternalChildren[i];
            //     Size dsize = element.DesiredSize;
            //     element.Arrange(new Rect(0, offsetY, finalSize.Width, dsize.Height));
            //     offsetY += element.RenderSize.Height + 1d;
            // }
            // return finalSize;
        }
    }
}