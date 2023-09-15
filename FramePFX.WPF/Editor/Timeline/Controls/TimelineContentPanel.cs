using System;
using System.Windows;
using System.Windows.Controls;
using FramePFX.WPF.Utils;

namespace FramePFX.WPF.Editor.Timeline.Controls {
    public class TimelineContentPanel : Grid {
        public TimelineControl Timeline { get; private set; }

        public TimelineContentPanel() {
            this.Loaded += (sender, args) => {
                this.Timeline = VisualTreeUtils.FindParent<TimelineControl>(this);
            };

            this.Unloaded += (s, e) => {
                this.Timeline = null;
            };
        }

        protected override Size MeasureOverride(Size constraint) {
            Size size = base.MeasureOverride(constraint);
            if (this.Timeline == null) {
                return size;
            }

            double width = this.Timeline.MaxDuration * this.Timeline.UnitZoom;
            if (double.IsNaN(size.Width) || double.IsPositiveInfinity(size.Width)) {
                return new Size(width, size.Height);
            }

            return new Size(Math.Max(width, size.Width), size.Height);
        }
    }
}