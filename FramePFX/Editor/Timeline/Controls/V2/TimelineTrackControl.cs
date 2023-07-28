using System.Windows;
using System.Windows.Controls;
using FramePFX.Utils;

namespace FramePFX.Editor.Timeline.Controls.V2 {
    public class TimelineTrackControl : Control {
        public TimelineControlV2 Timeline { get; private set; }

        public TimelineTrackControl() {
            this.Loaded += this.OnLoaded;
            this.Unloaded += this.OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e) {
            this.Timeline = VisualTreeUtils.FindVisualChild<TimelineControlV2>(this, false);
        }

        private void OnUnloaded(object sender, RoutedEventArgs e) {
            this.Timeline = null;
        }
    }
}