using System;
using System.Windows.Controls;
using System.Windows;

namespace FramePFX.Timeline {
    public class TimelineControl : ItemsControl {
        public static readonly DependencyProperty UnitZoomProperty =
            DependencyProperty.Register(
                "UnitZoom",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    1d,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((TimelineControl) d).OnUnitZoomChanged((double) e.OldValue, (double) e.NewValue),
                    (d, v) => TimelineUtils.ClampUnitZoom(v)));

        public static readonly DependencyProperty FrameOffsetProperty =
            DependencyProperty.Register(
                "FrameOffset",
                typeof(double),
                typeof(TimelineControl),
                new FrameworkPropertyMetadata(
                    0d,
                    FrameworkPropertyMetadataOptions.None,
                    (d, e) => ((TimelineControl) d).OnFrameOffsetChanged((double) e.OldValue, (double) e.NewValue),
                    (d, e) => (double) e > 0d ? 0d : e));

        private bool isUpdatingUnitZoom;
        private bool isUpdatingFrameOffset;

        /// <summary>
        /// The zoom level of all timeline layers
        /// <para>
        /// This is a value used for converting frames into pixels
        /// </para>
        /// </summary>
        public double UnitZoom {
            get => (double) this.GetValue(UnitZoomProperty);
            set => this.SetValue(UnitZoomProperty, value);
        }

        public double FrameOffset {
            get => (double) this.GetValue(FrameOffsetProperty);
            set => this.SetValue(FrameOffsetProperty, value);
        }

        public TimelineControl() {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            ScrollViewer.SetCanContentScroll(this, false);
        }

        protected override DependencyObject GetContainerForItemOverride() {
            return new TimelineLayerControl() { Timeline = this };
        }

        protected override bool IsItemItsOwnContainerOverride(object item) {
            return item is TimelineLayerControl;
        }

        private void OnUnitZoomChanged(double oldZoom, double newZoom) {
            if (this.isUpdatingUnitZoom)
                return;
            this.isUpdatingUnitZoom = true;
            if (Math.Abs(oldZoom - newZoom) > TimelineUtils.MinUnitZoom) {
                // foreach (TimelineLayerControl element in this.GetLayers()) {
                //     element.UnitZoom = newZoom;
                // }
            }
            this.isUpdatingUnitZoom = false;
        }

        private void OnFrameOffsetChanged(double oldOffset, double newOffset) {
            if (this.isUpdatingFrameOffset)
                return;
            this.isUpdatingFrameOffset = true;
            if (Math.Abs(oldOffset - newOffset) > TimelineUtils.MinUnitZoom) {
                // foreach (TimelineLayerControl element in this.GetLayers()) {
                //     element.FrameOffset = newOffset;
                // }
            }
            this.isUpdatingFrameOffset = false;
        }

        // public IEnumerable<TimelineLayerControl> GetLayers() {
        //     return this.Items.Select(x => this.ItemContainerGenerator.ContainerFromItem(x)).Cast<TimelineLayerControl>();
        // }
    }
}
