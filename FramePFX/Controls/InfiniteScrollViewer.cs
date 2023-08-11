using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FramePFX.Controls {
    public class InfiniteScrollViewer : ScrollViewer {
        private double offsetY;

        public InfiniteScrollViewer() {
            this.CanContentScroll = true;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.VerticalContentAlignment = VerticalAlignment.Top;
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e) {
            base.OnMouseWheel(e);

            // MouseWheelDown = -delta
            // MouseWheelUp   = +delta

            if (this.Content is FrameworkElement element) {
                if (e.Delta < 0) {
                    // down
                    this.offsetY += 24;
                }
                else if (e.Delta > 0) {
                    // up
                    this.offsetY -= 24;
                }
                else {
                    return;
                }

                if (this.VerticalOffset >= this.ScrollableHeight) {
                    Thickness thickness = element.Margin;
                    if (this.offsetY > 0) {
                        thickness.Bottom = this.offsetY;
                        thickness.Top = 0;
                    }
                    else if (this.offsetY < 0) {
                        thickness.Bottom = 0;
                        thickness.Top = -this.offsetY;
                    }
                    else {
                        thickness.Bottom = 0;
                        thickness.Top = 0;
                    }

                    element.Margin = thickness;
                }
                else if (this.offsetY != 0) {
                    Thickness thickness = element.Margin;
                    if (this.offsetY > 0) {
                        thickness.Bottom = this.offsetY;
                        thickness.Top = 0;
                    }
                    else if (this.offsetY < 0) {
                        thickness.Bottom = 0;
                        thickness.Top = -this.offsetY;
                    }
                    else {
                        thickness.Bottom = 0;
                        thickness.Top = 0;
                    }

                    element.Margin = thickness;
                }
            }
        }

        protected override void OnScrollChanged(ScrollChangedEventArgs e) {
            base.OnScrollChanged(e);
        }
    }
}